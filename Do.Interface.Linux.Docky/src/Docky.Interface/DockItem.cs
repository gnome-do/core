// DockItem.cs
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
	
	
	public class DockItem : BaseDockItem, IRightClickable
	{
		Item element;
		List<Wnck.Application> apps;
		Gdk.Rectangle icon_region;
		Gdk.Pixbuf drag_pixbuf;
		bool accepting_drops;
		uint handle_timer;
		int window_count;
		
		public event EventHandler RemoveClicked;
		
		public int Position { get; set; }
		
		public override bool IsAcceptingDrops { 
			get { return accepting_drops; } 
		}
		
		public string Icon { 
			get { return element.Icon; } 
		}
		
		public Item Element { 
			get { return element; } 
		}
		
		public Wnck.Application [] Applications { 
			get { return apps.ToArray (); } 
		}
		
		public IEnumerable<int> Pids { 
			get { return apps.Select (win => win.Pid).ToArray (); } 
		}
		
		public override int WindowCount {
			get { return window_count; }
		}
		
		public IEnumerable<Act> ActionsForItem {
			get {
				IEnumerable<Act> actions = Services.Core.GetActionsForItemOrderedByRelevance (element, false);
				// we want to keep the window operations stable, so we are going to special case them out now.
				// This has a degree of an abstraction break to it however, but it is important to get right
				// until a better solution is found.
				foreach (Act act in actions
				         .OrderByDescending (act => act.GetType ().Name != "WindowCloseAction")
				         .ThenByDescending (act => act.GetType ().Name != "WindowMinimizeAction")
				         .ThenByDescending (act => act.GetType ().Name != "WindowMaximizeAction")
				         .ThenByDescending (act => act.Relevance))
					yield return act;
			}
		}
			
		
		bool HasVisibleApps {
			get {
				if (apps == null)
					return false;
				return apps.SelectMany (app => app.Windows).Any (win => !win.IsSkipTasklist);
			}
		}	
		
		public DockItem (Item element) : base ()
		{
			Position = -1;
			apps =  new List<Wnck.Application> ();
			this.element = element;

			SetText (element.Name);

			AttentionRequestStartTime = DateTime.UtcNow;
			UpdateApplication ();
			NeedsAttention = DetermineAttentionStatus ();
			
			if (element is IFileItem && System.IO.Directory.Exists ((element as IFileItem).Path))
				accepting_drops = true;
			else
				accepting_drops = false;
		}
		
		public override bool ReceiveItem (string item)
		{
			if (!IsAcceptingDrops)
				return false;
			
			if (item.StartsWith ("file://"))
				item = item.Substring ("file://".Length);
			
			if (File.Exists (item)) {
				try {
					File.Move (item, System.IO.Path.Combine ((Element as IFileItem).Path, System.IO.Path.GetFileName (item)));
				} catch { return false; }
				return true;
			} else if (Directory.Exists (item)) {
				try {
					Directory.Move (item, System.IO.Path.Combine ((Element as IFileItem).Path, System.IO.Path.GetFileName (item)));
				} catch { return false; }
				return true;
			}
			return false;
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
			if (handle_timer > 0)
				return;
			// we do this delayed so that we dont get a flood of these events.  Certain windows behave badly.
			handle_timer = GLib.Timeout.Add (100, HandleUpdate);
			window_count = Applications.SelectMany (a => a.Windows).Where (w => !w.IsSkipTasklist).Count ();
			SetIconRegionFromCache ();
		}
		
		bool HandleUpdate ()
		{
			bool needed_attention = NeedsAttention;
			NeedsAttention = DetermineAttentionStatus ();
			
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
		
		bool DetermineAttentionStatus  ()
		{
			foreach (Application app in Applications) {
				if (app.Windows.Any (w => !w.IsSkipTasklist && w.NeedsAttention ()))
					return true;
			}
			return false;
		}
		
		protected override Gdk.Pixbuf GetSurfacePixbuf ()
		{
			Gdk.Pixbuf pbuf = IconProvider.PixbufFromIconName (Icon, DockPreferences.FullIconSize);
			if (pbuf.Height != DockPreferences.FullIconSize && pbuf.Width != DockPreferences.FullIconSize) {
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
		
		public override void Clicked (uint button)
		{
			if (!apps.Any () || !HasVisibleApps || button == 2) {
				AnimationType = ClickAnimationType.Bounce;
				Launch ();
			} else if (button == 1) {
				AnimationType = ClickAnimationType.Darken;
				WindowUtils.PerformLogicalClick (apps);
			}
			
			base.Clicked (button);
		}
		
		void Launch ()
		{
			if (Element is IFileItem)
				Services.Core.PerformDefaultAction (Element as Item, new [] { typeof (OpenAction), });
			else
				Services.Core.PerformDefaultAction (Element as Item, Type.EmptyTypes);
		}
		
		public override void SetIconRegion (Gdk.Rectangle region)
		{
			if (icon_region == region)
				return;
			icon_region = region;
			
			SetIconRegionFromCache ();
		}
		
		void SetIconRegionFromCache ()
		{
			Applications.ForEach (app => app.Windows.Where (w => !w.IsSkipTasklist)
			                      .ForEach (w => w.SetIconGeometry (icon_region.X, icon_region.Y, icon_region.Width, icon_region.Height)));
		}
		
		public override bool Equals (BaseDockItem other)
		{
			DockItem di = other as DockItem;
			return di != null && di.Element.UniqueId == Element.UniqueId;
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
				foreach (Wnck.Window window in Applications.SelectMany (app => app.Windows).Where (w => !w.IsSkipTasklist))
						yield return new WindowMenuButtonArgs (window, window.Name, Icon);
				yield return new SeparatorMenuButtonArgs ();
			}
			
			foreach (Act act in ActionsForItem)
				yield return new LaunchMenuButtonArgs (act, element, act.Name, act.Icon);

			yield return new SimpleMenuButtonArgs (OnRemoveClicked, "Remove from Dock", Gtk.Stock.Remove);
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
