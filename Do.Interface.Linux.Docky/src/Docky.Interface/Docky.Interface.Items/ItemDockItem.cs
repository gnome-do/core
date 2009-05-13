// ItemDockItem.cs
// 
// Copyright (C) 2008 GNOME Do
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Gdk;
using Cairo;
using Mono.Unix;

using Do.Platform;
using Do.Universe;
using Do.Universe.Common;
using Do.Interface;
using Do.Interface.Wink;
using Do.Interface.CairoUtils;

using Docky.Interface.Menus;
using Docky.Utilities;

using Wnck;

namespace Docky.Interface
{
	
	
	public class ItemDockItem : WnckDockItem, IRightClickable
	{
		Item item;
		int window_count;
		uint handle_timer;
		Gdk.Pixbuf drag_pixbuf;
		List<Wnck.Window> windows;
		
		public event EventHandler RemoveClicked;
		
		protected override string Icon { 
			get { return item.Icon; } 
		}
		
		string Name {
			get {
				if (NeedsAttention && AttentionWindows.Any ())
					return AttentionWindows.First ().Name;
				
				if (VisibleWindows.Any () && WindowCount == 1)
					return VisibleWindows.First ().Name;
				
				return Item.Name;
			}
		}
		
		public override Item Item { 
			get { return item; } 
		}
		
		public override IEnumerable<Wnck.Window> Windows { 
			get { return windows; } 
		}
		
		IEnumerable<Wnck.Window> AttentionWindows {
			get { return VisibleWindows.Where (w => w.NeedsAttention ()); }
		}
		
		public IEnumerable<int> Pids { 
			get { return windows.Select (win => win.Pid).ToArray (); } 
		}
		
		public override int WindowCount {
			get { return window_count; }
		}
		
		public ItemDockItem (Item item) : base ()
		{
			Position = -1;
			this.item = item;
			windows = new List<Wnck.Window> ();

			UpdateApplication ();
			NeedsAttention = DetermineUrgencyStatus ();
			
			SetText (Name);
		}
		
		public void UpdateApplication ()
		{
			UnregisterWindowEvents ();
			
			if (item is IApplicationItem) {
				windows = WindowUtils.WindowListForCmd ((item as IApplicationItem).Exec);
				window_count = windows.Where (w => !w.IsSkipTasklist).Count ();
			}
			
			RegisterWindowEvents ();
			SetText (Name);
			SetIconRegionFromCache ();
		}
		
		void RegisterWindowEvents ()
		{
			foreach (Wnck.Window w in VisibleWindows) {
				w.StateChanged += HandleStateChanged;
				w.NameChanged += HandleNameChanged;
			}
		}
		
		void UnregisterWindowEvents ()
		{
			foreach (Wnck.Window w in Windows) {
				try {
					w.StateChanged -= HandleStateChanged;
					w.NameChanged -= HandleNameChanged;
				} catch {}
			}
		}
		
		void HandleStateChanged (object o, StateChangedArgs args)
		{
			if (handle_timer > 0) return;
			// we do this delayed so that we dont get a flood of these events.  Certain windows behave badly.
			handle_timer = GLib.Timeout.Add (100, HandleUpdate);
			window_count = VisibleWindows.Count ();
			SetIconRegionFromCache ();
		}
		
		void HandleNameChanged(object sender, EventArgs e)
		{
			SetText (Name);
		}
		
		bool HandleUpdate ()
		{
			bool needed_attention = NeedsAttention;
			NeedsAttention = DetermineUrgencyStatus ();
			
			if (NeedsAttention != needed_attention) {
				UpdateRequestType req;
				if (NeedsAttention) 
					req = UpdateRequestType.NeedsAttentionSet;
				else
					req = UpdateRequestType.NeedsAttentionUnset;
				OnUpdateNeeded (new UpdateRequestArgs (this, req));
			}
			
			SetText (Name);
			handle_timer = 0;
			return false;
		}
		
		public override Pixbuf GetDragPixbuf ()
		{
			if (drag_pixbuf == null)
				drag_pixbuf = IconProvider.PixbufFromIconName (Icon, DockPreferences.FullIconSize);
			return drag_pixbuf;
		}
		
		public override void HotSeatRequested ()
		{
			if (WindowCount == 0) return;
			
			List<AbstractDockItem> dockitems = new List<AbstractDockItem> ();
					
			foreach (Act act in ActionsForItem (item)) {
				dockitems.Add (new ActionDockItem (act, item));
			}
			
			Docky.Core.DockServices.ItemsService.HotSeatItem (this, dockitems);
			base.HotSeatRequested ();
		}
		
		public override void Clicked (uint button, Gdk.ModifierType state, PointD position)
		{
			SetIconRegionFromCache ();
			base.Clicked (button, state, position);
		}

		
		protected override void Launch ()
		{
			if (Item is IFileItem)
				Services.Core.PerformDefaultAction (Item as Item, new [] { typeof (OpenAction), });
			else
				Services.Core.PerformDefaultAction (Item as Item, Type.EmptyTypes);
		}
		
		public override bool Equals (AbstractDockItem other)
		{
			if (other == null) return false;
			
			ItemDockItem di = other as ItemDockItem;
			return di != null && di.Item != null && Item != null && di.Item.UniqueId == Item.UniqueId;
		}

		#region IDisposable implementation 
		
		public override void Dispose ()
		{
			UnregisterWindowEvents ();
			item = null;
			windows.Clear ();
			
			if (drag_pixbuf != null)
				drag_pixbuf.Dispose ();
			
			base.Dispose ();
		}
		
		#endregion
		
		#region IRightClickable implementation 
		
		public IEnumerable<AbstractMenuArgs> GetMenuItems ()
		{
			bool hasApps = HasVisibleApps;
			
			yield return new SeparatorMenuButtonArgs ();
			
			if (hasApps) {
				foreach (Act act in ActionsForItem (item))
					yield return new LaunchMenuButtonArgs (act, item, act.Name, act.Icon).AsDark ();
			} else {
				foreach (Act act in ActionsForItem (item))
					yield return new LaunchMenuButtonArgs (act, item, act.Name, act.Icon);
			}
			
			if (hasApps) {
				foreach (Wnck.Window window in VisibleWindows) {
					yield return new SeparatorMenuButtonArgs ();
					yield return new WindowMenuButtonArgs (window, window.Name, Icon);
				}
			}

		}
		#endregion
	}
}
