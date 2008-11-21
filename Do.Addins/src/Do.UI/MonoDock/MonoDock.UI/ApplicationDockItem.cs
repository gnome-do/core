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
				icon_surface = new ImageSurface (Cairo.Format.Argb32, (int) (Preferences.IconSize*Preferences.IconQuality), 
				                                 (int) (Preferences.IconSize*Preferences.IconQuality));
				Context cr = new Context (icon_surface);
				
				Gdk.Pixbuf pbuf = GetIcon ();
				
				Gdk.CairoHelper.SetSourcePixbuf (cr, pbuf, 0, 0);
				cr.Paint ();
				
				pbuf.Dispose ();
				(cr as IDisposable).Dispose ();
			}
			return icon_surface;
		}
		
		Gdk.Pixbuf GetIcon ()
		{
			string[] guesses = new string[10];
			guesses[0] = application.Name.ToLower ().Replace (' ','-');
			guesses[1] = application.IconName.ToLower ().Replace (' ','-');
			guesses[2] = application.Windows[0].Name.ToLower ().Replace (' ','-');
			guesses[3] = application.Windows[0].IconName.ToLower ().Replace (' ','-');
			guesses[4] = "gnome-" + guesses[0];
			guesses[5] = "gnome-" + guesses[1];
			guesses[6] = "gnome-" + guesses[2];
			guesses[7] = "gnome-" + guesses[3];
			guesses[8] = System.Diagnostics.Process.GetProcessById (application.Pid).ProcessName;
			guesses[9] = System.Diagnostics.Process.GetProcessById (application.Pid).ProcessName.Split ('-')[0];
			
			Gdk.Pixbuf pbuf = null;
			foreach (string guess in guesses) {
				string icon_guess = guess;
				if (pbuf != null)
					pbuf.Dispose ();
				
				pbuf = IconProvider.PixbufFromIconName (icon_guess, (int) (Preferences.IconSize*Preferences.IconQuality), false);
				if (pbuf != null && (pbuf.Width == (int) (Preferences.IconSize*Preferences.IconQuality) || 
				                     pbuf.Height == (int) (Preferences.IconSize*Preferences.IconQuality)))
					return pbuf;
			
				string desktop_path = GetDesktopFile (icon_guess);
				if (!string.IsNullOrEmpty (desktop_path)) {
					Gnome.DesktopItem di = Gnome.DesktopItem.NewFromFile (desktop_path, Gnome.DesktopItemLoadFlags.OnlyIfExists);
					if (pbuf != null)
						pbuf.Dispose ();
					pbuf = IconProvider.PixbufFromIconName (di.GetString ("Icon"), (int) (Preferences.IconSize*Preferences.IconQuality));
					di.Dispose ();
					return pbuf;
				}
			}
			
			if (pbuf == null) {
				pbuf =  IconProvider.PixbufFromIconName (guesses[0], (int) (Preferences.IconSize*Preferences.IconQuality));
			}
			
			if (pbuf.Height != Preferences.IconSize*Preferences.IconQuality && pbuf.Width != Preferences.IconSize*Preferences.IconQuality) {
				double scale = (double)Preferences.IconSize*Preferences.IconQuality / Math.Max (pbuf.Width, pbuf.Height);
				Gdk.Pixbuf temp = pbuf.ScaleSimple ((int) (pbuf.Width * scale), (int) (pbuf.Height * scale), Gdk.InterpType.Bilinear);
				pbuf.Dispose ();
				pbuf = temp;
			}
			return pbuf;
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
			if (sr == null)
				sr = MonoDock.UI.Util.GetBorderedTextSurface (application.Name, Preferences.TextWidth);
			return sr;
		}
		
		public string Description {
			get {
				return application.Name;
			}
		}
		
		public bool DrawIndicator { get { return true; } }
		
		public int Width {
			get {
				return Preferences.IconSize;
			}
		}
		
		public int Height {
			get {
				return Preferences.IconSize;
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
			Preferences.IconSizeChanged += Dispose;
		}
		
		public void Clicked (uint button, IDoController controller)
		{
			if (button == 1)
				WindowUtils.PerformLogicalClick (new Wnck.Application[] {application});
		}

		#region IDisposable implementation 
		
		public void Dispose ()
		{
//			application.Dispose ();
			
			if (sr != null) {
				sr.Destroy ();
				sr = null;
			}
			
			if (icon_surface != null) {
				icon_surface.Destroy ();
				icon_surface = null;
			}
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