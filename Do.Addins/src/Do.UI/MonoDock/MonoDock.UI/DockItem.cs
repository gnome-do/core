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

using Do.UI;
using Do.Addins;
using Do.Platform;
using Do.Universe;
using Do.Universe.Common;
using Do.Addins.CairoUtils;

using MonoDock.Util;

namespace MonoDock.UI
{
	
	
	public class DockItem : IDockItem, IDoDockItem
	{
		IObject item;
		Surface sr, icon_surface;
		List<Wnck.Application> apps;
		
		public string Icon { get { return item.Icon; } }
		public string Description { get { return item.Name; } }
		public IObject IObject { get { return item; } }
		
		public DateTime LastClick { get; set; }
		public DateTime DockAddItem { get; set; }
		
		public int Width { get { return DockPreferences.IconSize; } }
		public int Height { get { return DockPreferences.IconSize; } }
		public bool Scalable { get { return true; } }
		public bool DrawIndicator { get { return HasVisibleApps; } }
		
		public Wnck.Application[] Apps { get { return apps.ToArray (); } }
		public IEnumerable<int> Pids { get { return apps.Select (item => item.Pid); } }
		
		Gdk.Pixbuf pixbuf;
		Gdk.Pixbuf Pixbuf {
			get {
				return pixbuf ?? pixbuf = GetPixbuf ();
			}
		}
		
		bool HasVisibleApps {
			get {
				if (apps == null)
					return false;
				return apps
					.Select ((Wnck.Application app) => app.Windows)
					.Where ((Wnck.Window[] wins) => (wins as Wnck.Window[])
					        .Where ((Wnck.Window win) => !win.IsSkipTasklist)
					        .Any())
					.Any ();
			}
		}	
		
		public DockItem(IObject item)
		{
			apps =  new List<Wnck.Application> ();
			LastClick = DateTime.UtcNow - new TimeSpan (0, 10, 0);
			this.item = item;
			DockPreferences.IconSizeChanged += Dispose;
			
			UpdateApplication ();
		}
		
		public void UpdateApplication ()
		{
			IObject inner = Do.Platform.Core.Unwrap (item);
			if (inner is IApplicationItem) {
				apps = WindowUtils.GetApplicationList ((inner as IApplicationItem).Exec);
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
		
		public Surface GetIconSurface ()
		{
			if (icon_surface == null) {
				icon_surface = new ImageSurface (Cairo.Format.Argb32, DockPreferences.FullIconSize, DockPreferences.FullIconSize);
				Context cr = new Context (icon_surface);
				Gdk.CairoHelper.SetSourcePixbuf (cr, Pixbuf, 0, 0);
				cr.Paint ();
				
				(cr as IDisposable).Dispose ();
				pixbuf.Dispose ();
				pixbuf = null;
			}
			return icon_surface;
		}
		
		public Surface GetTextSurface ()
		{
			if (sr == null)
				sr = Util.GetBorderedTextSurface (item.Name, DockPreferences.TextWidth);
			return sr;
		}
		
		public void Clicked (uint button, IDoController controller)
		{
			if (!apps.Any () || !HasVisibleApps || button == 2) {
				LastClick = DateTime.UtcNow;
				if (IObject is IFileItem)
					controller.PerformDefaultAction (IObject as IItem, new Type[] { typeof (OpenAction), });
				else
					controller.PerformDefaultAction (IObject as IItem, Type.EmptyTypes);
				return;
			}
				
			if (button == 1)
				WindowUtils.PerformLogicalClick (apps);
		}
		
		Gdk.Rectangle icon_region;
		public void SetIconRegion (Gdk.Rectangle region)
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
		
		public bool Equals (IDockItem other)
		{
			DockItem di = other as DockItem;
			if (di == null)
				return false;
			
			return di.IObject.Name+di.IObject.Description+di.IObject.Icon == IObject.Name+IObject.Description+IObject.Icon;
		}

		#region IDisposable implementation 
		
		public void Dispose ()
		{
			if (sr != null) {
				sr.Destroy ();
				sr = null;
			}
			
			if (icon_surface != null) {
				icon_surface.Destroy ();
				icon_surface = null;
			}
			
			if (pixbuf != null) {
				pixbuf.Dispose ();
				pixbuf = null;
			}
		}
		
		#endregion 
		
	}
}
