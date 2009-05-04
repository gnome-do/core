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
		const string ErrorMessage = "Docky could not move the file to the requested Directory.  " + 
			"Please check file name and permissions and try again";
		
		Item element;
		int window_count;
		uint handle_timer;
		bool accepting_drops;
		Gdk.Pixbuf drag_pixbuf;
		Gdk.Rectangle icon_region;
		List<Wnck.Window> windows;
		
		public event EventHandler RemoveClicked;
		
		public override bool IsAcceptingDrops { 
			get { return accepting_drops; } 
		}
		
		protected override string Icon { 
			get { return element.Icon; } 
		}
		
		string Name {
			get {
				if (NeedsAttention && AttentionWindows.Any ())
					return AttentionWindows.First ().Name;
				
				if (VisibleWindows.Any () && WindowCount == 1)
					return VisibleWindows.First ().Name;
				
				return Element.Name;
			}
		}
		
		public Item Element { 
			get { return element; } 
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
		
		public ItemDockItem (Item element) : base ()
		{
			Position = -1;
			this.element = element;
			windows = new List<Wnck.Window> ();

			UpdateApplication ();
			NeedsAttention = DetermineUrgencyStatus ();
			
			if (element is IFileItem && Directory.Exists ((element as IFileItem).Path))
				accepting_drops = true;
			else
				accepting_drops = false;
			
			SetText (Name);
		}
		
		public override bool ReceiveItem (string item)
		{
			bool result = false;
			if (!IsAcceptingDrops)
				return result;
			
			if (item.StartsWith ("file://"))
				item = item.Substring ("file://".Length);
			
			if (File.Exists (item)) {
				try {
					File.Move (item, System.IO.Path.Combine ((Element as IFileItem).Path, System.IO.Path.GetFileName (item)));
					result = true;
				} catch { 
					Services.Notifications.Notify ("Docky Error", ErrorMessage);
				}
			} else if (Directory.Exists (item)) {
				try {
					Directory.Move (item, System.IO.Path.Combine ((Element as IFileItem).Path, System.IO.Path.GetFileName (item)));
					result = true;
				} catch { 
					Services.Notifications.Notify ("Docky Error", ErrorMessage);
				}
			}
			return result;
		}
		
		public void UpdateApplication ()
		{
			UnregisterWindowEvents ();
			
			if (element is IApplicationItem) {
				windows = WindowUtils.WindowListForCmd ((element as IApplicationItem).Exec);
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
					
			foreach (Act act in ActionsForItem (element)) {
				dockitems.Add (new ActionDockItem (act, element));
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
			if (Element is IFileItem)
				Services.Core.PerformDefaultAction (Element as Item, new [] { typeof (OpenAction), });
			else
				Services.Core.PerformDefaultAction (Element as Item, Type.EmptyTypes);
		}
		
		public override void SetIconRegion (Gdk.Rectangle region)
		{
			if (icon_region == region) return;
			icon_region = region;
			SetIconRegionFromCache ();
		}
		
		void SetIconRegionFromCache ()
		{
			VisibleWindows.ForEach (w => w.SetIconGeometry (icon_region.X, icon_region.Y, icon_region.Width, icon_region.Height));
		}
		
		public override bool Equals (AbstractDockItem other)
		{
			if (other == null) return false;
			
			ItemDockItem di = other as ItemDockItem;
			return di != null && di.Element != null && Element != null && di.Element.UniqueId == Element.UniqueId;
		}

		#region IDisposable implementation 
		
		public override void Dispose ()
		{
			UnregisterWindowEvents ();
			element = null;
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
				foreach (Act act in ActionsForItem (element))
					yield return new LaunchMenuButtonArgs (act, element, act.Name, act.Icon).AsDark ();
			} else {
				foreach (Act act in ActionsForItem (element))
					yield return new LaunchMenuButtonArgs (act, element, act.Name, act.Icon);
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
