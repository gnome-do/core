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
		List<Wnck.Application> apps;
		
		public event EventHandler RemoveClicked;
		
		public int Position { get; set; }
		
		public override bool IsAcceptingDrops { 
			get { return accepting_drops; } 
		}
		
		string Icon { 
			get { return element.Icon; } 
		}
		
		public Item Element { 
			get { return element; } 
		}
		
		protected override IEnumerable<Wnck.Application> Applications { 
			get { return apps; } 
		}
		
		public IEnumerable<int> Pids { 
			get { return apps.Select (win => win.Pid).ToArray (); } 
		}
		
		public override int WindowCount {
			get { return window_count; }
		}
		
		IEnumerable<Act> ActionsForItem {
			get {
				IEnumerable<Act> actions = Services.Core.GetActionsForItemOrderedByRelevance (element, false);
				// we want to keep the window operations stable, so we are going to special case them out now.
				// This has a degree of an abstraction break to it however, but it is important to get right
				// until a better solution is found.
				foreach (Act act in actions
				         .Where (act => act.GetType ().Name != "CopyToClipboardAction")
				         .OrderByDescending (act => act.GetType ().Name != "WindowCloseAction")
				         .ThenByDescending (act => act.GetType ().Name != "WindowMinimizeAction")
				         .ThenByDescending (act => act.GetType ().Name != "WindowMaximizeAction")
				         .ThenByDescending (act => act.Relevance))
					yield return act;
			}
		}
		
		public ItemDockItem (Item element) : base ()
		{
			Position = -1;
			this.element = element;
			apps = new List<Wnck.Application> ();

			SetText (element.Name);

			AttentionRequestStartTime = DateTime.UtcNow;
			UpdateApplication ();
			NeedsAttention = DetermineUrgencyStatus ();
			
			if (element is IFileItem && Directory.Exists ((element as IFileItem).Path))
				accepting_drops = true;
			else
				accepting_drops = false;
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
			UnregisterStateChangeEvents ();
			
			if (element is IApplicationItem) {
				apps = WindowUtils.GetApplicationList ((element as IApplicationItem).Exec);
				window_count = Applications.SelectMany (a => a.Windows).Where (w => !w.IsSkipTasklist).Count ();
			}
			
			RegisterStateChangeEvents ();
		}
		
		void RegisterStateChangeEvents ()
		{
			foreach (Application app in Applications) {
				foreach (Wnck.Window w in app.Windows) {
					if (!w.IsSkipTasklist)
						w.StateChanged += OnWindowStateChanged;
				}
			}
		}
		
		void UnregisterStateChangeEvents ()
		{
			foreach (Application app in Applications) {
				foreach (Wnck.Window w in app.Windows) {
					try {
						w.StateChanged -= OnWindowStateChanged;
					} catch {}
				}
			}
		}
		
		void OnWindowStateChanged (object o, StateChangedArgs args)
		{
			if (handle_timer > 0) return;
			// we do this delayed so that we dont get a flood of these events.  Certain windows behave badly.
			handle_timer = GLib.Timeout.Add (100, HandleUpdate);
			window_count = Applications.SelectMany (a => a.Windows).Where (w => !w.IsSkipTasklist).Count ();
			SetIconRegionFromCache ();
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
			
			handle_timer = 0;
			return false;
		}
		
		protected override Gdk.Pixbuf GetSurfacePixbuf (int size)
		{
			Gdk.Pixbuf pbuf = IconProvider.PixbufFromIconName (Icon, size);
			if (pbuf.Height != size && pbuf.Width != size) {
				double scale = (double)DockPreferences.FullIconSize / Math.Max (pbuf.Width, pbuf.Height);
				Gdk.Pixbuf temp = pbuf.ScaleSimple ((int) (pbuf.Width * scale), (int) (pbuf.Height * scale), InterpType.Bilinear);
				pbuf.Dispose ();
				pbuf = temp;
			}
			
			return pbuf;
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
					
			foreach (Act act in ActionsForItem) {
				dockitems.Add (new ActionDockItem (act, element));
			}
			
			Docky.Core.DockServices.ItemsService.HotSeatItem (this, dockitems);
			base.HotSeatRequested ();
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
			UnregisterStateChangeEvents ();
			element = null;
			apps.Clear ();
			
			if (drag_pixbuf != null)
				drag_pixbuf.Dispose ();
			
			base.Dispose ();
		}
		
		#region IRightClickable implementation 
		
		public IEnumerable<AbstractMenuButtonArgs> GetMenuItems ()
		{
			bool hasApps = HasVisibleApps;
			
			if (hasApps) {
				foreach (Wnck.Window window in VisibleWindows)
						yield return new WindowMenuButtonArgs (window, window.Name, Icon);
				yield return new SeparatorMenuButtonArgs ();
			}
			
			foreach (Act act in ActionsForItem)
				yield return new LaunchMenuButtonArgs (act, element, act.Name, act.Icon);

			yield return new SimpleMenuButtonArgs (OnRemoveClicked, Catalog.GetString ("Remove from Dock"), Gtk.Stock.Remove);
		}
		
		#endregion 
		#endregion
		
		void OnRemoveClicked ()
		{
			if (RemoveClicked != null)
				RemoveClicked (this, new EventArgs ());
		}
	}
}
