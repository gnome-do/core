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
using System.Linq;

using Gdk;
using Cairo;

using Do.Interface;
using Do.Platform;
using Do.Universe;
using Do.Universe.Common;
using Do.Interface.CairoUtils;

using Docky.Utilities;

using Wnck;

namespace Docky.Interface
{
	
	
	public class DockItem : AbstractDockItem, IDoDockItem, IRightClickable
	{
		Element item;
		Surface icon_surface;
		List<Wnck.Application> apps;
		Gdk.Rectangle icon_region;
		
		public string Icon { 
			get { return item.Icon; } 
		}
		
		public override string Description { 
			get { return item.Name; } 
		}
		
		public Element Element { 
			get { return item; } 
		}
		
		public override bool Scalable { 
			get { return true; } 
		}
		
		public override bool DrawIndicator { 
			get { return HasVisibleApps; } 
		}
		
		public Wnck.Application [] Apps { 
			get { return apps.ToArray (); } 
		}
		
		public IEnumerable<int> Pids { 
			get { return apps.Select (item => item.Pid); } 
		}
		
		bool HasVisibleApps {
			get {
				if (apps == null)
					return false;
				return apps
					.Select (app => app.Windows)
					.Any (wins => wins.Any (win => !win.IsSkipTasklist));
			}
		}	
		
		public DockItem (Element item) : base ()
		{
			apps =  new List<Wnck.Application> ();
			this.item = item;
			
			UpdateApplication ();
		}
		
		protected override void OnIconSizeChanged ()
		{
			if (icon_surface != null) {
				icon_surface.Destroy ();
				icon_surface = null;
			}
			
			base.OnIconSizeChanged ();
		}

		
		public void UpdateApplication ()
		{
			if (item is IApplicationItem) {
				apps = WindowUtils.GetApplicationList ((item as IApplicationItem).Exec);
			}
		}
		
		Gdk.Pixbuf GetPixbuf ()
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
		
		public override Surface GetIconSurface (Surface sr)
		{
			if (icon_surface == null) {
				Gdk.Pixbuf pixbuf = GetPixbuf ();
				icon_surface = sr.CreateSimilar (sr.Content, DockPreferences.FullIconSize, DockPreferences.FullIconSize);
				Context cr = new Context (icon_surface);
				Gdk.CairoHelper.SetSourcePixbuf (cr, pixbuf, 0, 0);
				cr.Paint ();
				
				(cr as IDisposable).Dispose ();
				pixbuf.Dispose ();
				pixbuf = null;
			}
			return icon_surface;
		}
		
		public override void Clicked (uint button, IDoController controller)
		{
			if (!apps.Any () || !HasVisibleApps || button == 2) {
				LastClick = DateTime.UtcNow;
				if (Element is IFileItem)
					controller.PerformDefaultAction (Element as Item, new [] { typeof (OpenAction), });
				else
					controller.PerformDefaultAction (Element as Item, Type.EmptyTypes);
				return;
			}
				
			if (button == 1)
				WindowUtils.PerformLogicalClick (apps);
		}
		
		public override void SetIconRegion (Gdk.Rectangle region)
		{
			if (icon_region == region)
				return;
			icon_region = region;
			
			foreach (Wnck.Application application in apps) {
				foreach (Wnck.Window window in application.Windows) {
					window.SetIconGeometry (region.X, region.Y, region.Width, region.Height);
				}
			}
		}
		
		public override bool Equals (IDockItem other)
		{
			DockItem di = other as DockItem;
			if (di == null)
				return false;
			
			return di.Element.UniqueId == Element.UniqueId;
		}

		#region IDisposable implementation 
		
		public override void Dispose ()
		{
			if (icon_surface != null) {
				icon_surface.Destroy ();
				icon_surface = null;
			}
			
			base.Dispose ();
		}

		#region IRightClickable implementation 
		
		public IEnumerable<MenuArgs> GetMenuItems ()
		{
			List<MenuArgs> outList = new List<MenuArgs> ();
			foreach (Application app in Apps) {
				foreach (Wnck.Window window in app.Windows) {
					Wnck.Window copy_win = window;
					if (!copy_win.IsSkipTasklist) {
						outList.Add (new MenuArgs ((o, a) => copy_win.CenterAndFocusWindow (), copy_win.Name, Gtk.Stock.GoForward));
					}
				}
			}
			return outList;
		}
		
		#endregion 
		
		
		#endregion 
		
	}
}
