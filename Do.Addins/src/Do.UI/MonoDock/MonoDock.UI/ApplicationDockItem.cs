// ApplicationDockItem.cs
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

using Cairo;

using Do.Addins;
using Do.Addins.CairoUtils;
using Do.Universe;
using Do.UI;

using MonoDock.Util;

namespace MonoDock.UI
{
	
	
	public class ApplicationDockItem : IDockItem
	{
		enum ClickAction {
			Focus,
			Minimize,
			Restore,
		}
		
		static IEnumerable<String> DesktopFilesDirectories {
			get {
				return new string[] {
					"~/.local/share/applications/wine",
					"~/.local/share/applications",
					"/usr/share/applications",
					"/usr/share/applications/kde",
					"/usr/share/applications/kde4",
					"/usr/share/gdm/applications",
					"/usr/local/share/applications",
				};
			}
		}
		
		Wnck.Application application;
		Surface sr, icon_surface;
		
		#region IDockItem implementation 
		public Surface GetIconSurface ()
		{
			if (icon_surface == null) {
				icon_surface = new ImageSurface (Cairo.Format.Argb32, (int) (DockItem.IconSize*DockItem.IconQuality), 
				                                 (int) (DockItem.IconSize*DockItem.IconQuality));
				Context cr = new Context (icon_surface);
				string icon_guess = application.Name.ToLower ().Replace (' ','-');
				Gdk.Pixbuf pbuf = IconProvider.PixbufFromIconName (icon_guess, (int) (DockItem.IconSize*DockItem.IconQuality), false);
				if (pbuf == null || pbuf.Width != (int) (DockItem.IconSize*DockItem.IconQuality) && pbuf.Height != (int) (DockItem.IconSize*DockItem.IconQuality)) {
					string desktop_path = GetDesktopFile (icon_guess);
					if (!string.IsNullOrEmpty (desktop_path)) {
						Gnome.DesktopItem di = Gnome.DesktopItem.NewFromFile (desktop_path, Gnome.DesktopItemLoadFlags.OnlyIfExists);
						if (pbuf != null)
							pbuf.Dispose ();
						pbuf = IconProvider.PixbufFromIconName (di.GetString ("Icon"), (int) (DockItem.IconSize*DockItem.IconQuality));
						di.Dispose ();
					} else {
						icon_guess = "gnome-" + icon_guess;
						Gdk.Pixbuf pbuf2 = IconProvider.PixbufFromIconName (icon_guess, (int) (DockItem.IconSize*DockItem.IconQuality));
						if (pbuf2.Width != (int) (DockItem.IconSize*DockItem.IconQuality) && pbuf2.Height != (int) (DockItem.IconSize*DockItem.IconQuality)) {
							pbuf2.Dispose ();
						} else {
							if (pbuf != null)
								pbuf.Dispose ();
							pbuf = pbuf2;
						}
					}
				}
				if (pbuf == null) {
					pbuf =  IconProvider.PixbufFromIconName (icon_guess, (int) (DockItem.IconSize*DockItem.IconQuality));
				}
				Gdk.CairoHelper.SetSourcePixbuf (cr, pbuf, 0, 0);
				cr.Paint ();
				
				pbuf.Dispose ();
				(cr as IDisposable).Dispose ();
			}
			return icon_surface;
		}
		
		string GetDesktopFile (string base_name)
		{
			foreach (string dir in DesktopFilesDirectories) {
				if (File.Exists (System.IO.Path.Combine (dir, base_name+".desktop")))
					return System.IO.Path.Combine (dir, base_name+".desktop");
				if (File.Exists (System.IO.Path.Combine (dir, "gnome-"+base_name+".desktop")))
					return System.IO.Path.Combine (dir, "gnome-"+base_name+".desktop");
			}
			return null;
		}
		
		public Surface GetTextSurface ()
		{
			if (sr == null) {
				sr = new Cairo.ImageSurface (Cairo.Format.Argb32, DockItem.TextWidth, 20);
				
				Context cr = new Context (sr);
				
				Pango.Layout layout = Pango.CairoHelper.CreateLayout (cr);
				layout.Width = Pango.Units.FromPixels (DockItem.TextWidth);
				layout.SetMarkup ("<b>" + application.Name + "</b>");
				layout.Alignment = Pango.Alignment.Center;
				layout.Ellipsize = Pango.EllipsizeMode.End;
				
				Pango.Rectangle rect1, rect2;
				layout.GetExtents (out rect1, out rect2);
				
				cr.SetRoundedRectanglePath (Pango.Units.ToPixels (rect2.X) - 10, 0, Pango.Units.ToPixels (rect2.Width) + 16, 20, 10);
				cr.Color = new Cairo.Color (0, 0, 0, .6);
				cr.Fill ();
				
				Pango.CairoHelper.LayoutPath (cr, layout);
				
				cr.Color = new Cairo.Color (0, 0, 0);
				cr.Fill ();
				
				cr.Translate (-2, -1);
				Pango.CairoHelper.LayoutPath (cr, layout);
				cr.Color = new Cairo.Color (1, 1, 1);
				cr.Fill ();
				
				(cr as IDisposable).Dispose ();
				layout.Dispose ();
			}
			return sr;
		}
		
		public string Description {
			get {
				return application.Name;
			}
		}
		
		public int Width {
			get {
				return DockItem.IconSize;
			}
		}
		
		public int Height {
			get {
				return DockItem.IconSize;
			}
		}
		
		public bool Scalable {
			get {
				return true;
			}
		}
		
		public Wnck.Application App {
			get { return application; }
		}
		
		public DateTime LastClick { get; set; }
		
		public DateTime DockAddItem { get; set; }
		#endregion 
		
		public ApplicationDockItem(Wnck.Application application)
		{
			LastClick = DateTime.UtcNow - new TimeSpan (0, 10, 0);
			this.application = application;
		}
		
		public void Clicked (uint button)
		{
			if (button == 1) {
				foreach (Wnck.Window window in application.Windows) {
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
			} else if (button == 2) {
				foreach (Wnck.Window window in application.Windows) {
					if (window.IsInViewport (Wnck.Screen.Default.ActiveWorkspace)) {
						window.Close (Gtk.Global.CurrentEventTime);
					}
				}
			}
		}
		
		ClickAction GetClickAction ()
		{
			foreach (Wnck.Window window in application.Windows) {
				if (window.IsMinimized && window.IsInViewport (Wnck.Screen.Default.ActiveWorkspace))
					return ClickAction.Restore;
			}
			
			foreach (Wnck.Window window in application.Windows) {
				if (window.IsActive && window.IsInViewport (Wnck.Screen.Default.ActiveWorkspace))
					return ClickAction.Minimize;
			}
			
			return ClickAction.Focus;
		}

		#region IDisposable implementation 
		
		public void Dispose ()
		{
//			application.Dispose ();
			
			if (sr != null)
				sr.Destroy ();
			
			if (icon_surface != null)
				icon_surface.Destroy ();
		}
		
		#endregion 
		
		
		public bool Equals (IDockItem other)
		{
			if (!(other is ApplicationDockItem))
				return false;
			
			return ((other as ApplicationDockItem).application == application);
		}
	}
}