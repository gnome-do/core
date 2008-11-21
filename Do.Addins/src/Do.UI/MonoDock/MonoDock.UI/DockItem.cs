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
using Do.Universe;
using Do.Addins;
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
		
		public int Width { get { return Preferences.IconSize; } }
		public int Height { get { return Preferences.IconSize; } }
		public bool Scalable { get { return true; } }
		public bool DrawIndicator { get { return apps.Any (); } }
		
		public Wnck.Application[] Apps { get { return apps.ToArray (); } }
		public IEnumerable<int> Pids { get { return apps.Select (item => item.Pid); } }
		
		Gdk.Pixbuf pixbuf;
		Gdk.Pixbuf Pixbuf {
			get {
				return pixbuf ?? pixbuf = GetPixbuf ();
			}
		}
		
		public DockItem(IObject item)
		{
			apps =  new List<Wnck.Application> ();
			LastClick = DateTime.UtcNow - new TimeSpan (0, 10, 0);
			this.item = item;
			Preferences.IconSizeChanged += Dispose;
			
			UpdateApplication ();
		}
		
		public void UpdateApplication ()
		{
			if (item is ApplicationItem) {
				apps = WindowUtils.GetApplicationList ((item as ApplicationItem).Exec);
			}
		}
		
		Gdk.Pixbuf GetPixbuf ()
		{
			Gdk.Pixbuf pbuf = IconProvider.PixbufFromIconName (Icon, (int) (Preferences.IconSize*Preferences.IconQuality));
			
			if (pbuf.Height != Preferences.IconSize*Preferences.IconQuality && pbuf.Width != Preferences.IconSize*Preferences.IconQuality) {
				double scale = (double)Preferences.IconSize*Preferences.IconQuality / Math.Max (pbuf.Width, pbuf.Height);
				Gdk.Pixbuf temp = pbuf.ScaleSimple ((int) (pbuf.Width * scale), (int) (pbuf.Height * scale), InterpType.Bilinear);
				pbuf.Dispose ();
				pbuf = temp;
			}
			
			return pbuf;
		}
		
		public Surface GetIconSurface ()
		{
			if (icon_surface == null) {
				icon_surface = new ImageSurface (Cairo.Format.Argb32, (int) (Preferences.IconSize*Preferences.IconQuality), (int) (Preferences.IconSize*Preferences.IconQuality));
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
				sr = Util.GetBorderedTextSurface (item.Name, Preferences.TextWidth);
			return sr;
		}
		
		public void Clicked (uint button, IDoController controller)
		{
			if (!apps.Any () || button == 2) {
				controller.PerformDefaultAction (IObject as IItem);
				return;
			}
				
			if (button == 1) {
				bool not_in_viewport = true;
				foreach (Wnck.Application application in apps) {
					foreach (Wnck.Window window in application.Windows) {
						if (!window.IsSkipTasklist && window.IsInViewport (Wnck.Screen.Default.ActiveWorkspace))
							not_in_viewport = false;
					}
				}
				
				if (not_in_viewport) {
					foreach (Wnck.Application application in apps) {
						foreach (Wnck.Window window in application.Windows) {
							if (!window.IsSkipTasklist) {
								window.CenterAndFocusWindow ();
								return;
							}
						}
					}
				}
				
				foreach (Wnck.Application app in apps) {
					foreach (Wnck.Window window in app.Windows) {
						switch (GetClickAction ()) {
						case ClickAction.Focus:
							if (window.IsInViewport (Wnck.Screen.Default.ActiveWorkspace))
								window.Activate (Gtk.Global.CurrentEventTime);
							break;
						case ClickAction.Minimize:
							if (window.IsInViewport (Wnck.Screen.Default.ActiveWorkspace))
								window.Minimize ();
							break;
						case ClickAction.Restore:
							if (window.IsInViewport (Wnck.Screen.Default.ActiveWorkspace))
								window.Unminimize (Gtk.Global.CurrentEventTime);
							break;
						}
					}
				}
			}
		}
		
		ClickAction GetClickAction ()
		{
			if (!apps.Any ())
				return ClickAction.None;
			
			foreach (Wnck.Application app in apps) {
				foreach (Wnck.Window window in app.Windows) {
					if (window.IsMinimized && window.IsInViewport (Wnck.Screen.Default.ActiveWorkspace))
						return ClickAction.Restore;
				}
			}
			
			foreach (Wnck.Application app in apps) {
				foreach (Wnck.Window window in app.Windows) {
					if (window.IsActive && window.IsInViewport (Wnck.Screen.Default.ActiveWorkspace))
						return ClickAction.Minimize;
				}
			}
			
			return ClickAction.Focus;
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
