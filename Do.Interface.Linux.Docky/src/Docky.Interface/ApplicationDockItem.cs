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
using System.Linq;

using Cairo;
using Gdk;

using Do.Interface.CairoUtils;
using Do.Platform;
using Do.Interface;
using Do.Universe;

using Docky.Utilities;

namespace Docky.Interface
{
	
	
	public class ApplicationDockItem : AbstractDockItem, IRightClickable
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
		Surface icon_surface;
		Gdk.Rectangle icon_region;
		
		#region IDockItem implementation 
		public override Surface GetIconSurface (Surface sr)
		{
			if (icon_surface == null) {
				icon_surface = sr.CreateSimilar (sr.Content, DockPreferences.FullIconSize, DockPreferences.FullIconSize);
				Context cr = new Context (icon_surface);
				
				Gdk.Pixbuf pbuf = GetIcon ();
				if (pbuf.Width != DockPreferences.FullIconSize || pbuf.Height != DockPreferences.FullIconSize) {
					double scale = (double)DockPreferences.FullIconSize / Math.Max (pbuf.Width, pbuf.Height);
					Gdk.Pixbuf temp = pbuf.ScaleSimple ((int) (pbuf.Width * scale), (int) (pbuf.Height * scale), Gdk.InterpType.Bilinear);
					pbuf.Dispose ();
					pbuf = temp;
				}
				
				Gdk.CairoHelper.SetSourcePixbuf (cr, 
				                                 pbuf, 
				                                 (int) ((DockPreferences.FullIconSize - pbuf.Width) / 2),
				                                 (int) ((DockPreferences.FullIconSize - pbuf.Height) / 2));
				cr.Paint ();
				
				pbuf.Dispose ();
				(cr as IDisposable).Dispose ();
			}
			return icon_surface;
		}
		
		public override Pixbuf GetDragPixbuf ()
		{
			return App.Icon;
		}

		/// <summary>
		/// This method is designed to *attempt* to get a decent pixbuf for the current Application.  There
		/// are no promises and certainly a decent amount of work that can go into it.  Currently it makes a considerable
		/// number of guesses at the icon name looking for Desktop files and for regular pixbufs and can take quite a bit
		/// of time.  Some global caching should be added.
		/// </summary>
		/// <returns>
		/// A <see cref="Gdk.Pixbuf"/>
		/// </returns>
		Gdk.Pixbuf GetIcon ()
		{
			List<string> guesses = new List<string> ();
			guesses.Add (application.Name.ToLower ().Replace (' ','-'));
			guesses.Add (application.Windows[0].Name.ToLower ().Replace (' ','-'));
			guesses.Add (application.IconName.ToLower ().Replace (' ','-'));
			guesses.Add (application.Windows[0].IconName.ToLower ().Replace (' ','-'));
			guesses.Add ("gnome-" + guesses[0]);
			guesses.Add ("gnome-" + guesses[1]);
			guesses.Add ("gnome-" + guesses[2]);
			guesses.Add ("gnome-" + guesses[3]);
			
			if (application.Name.Length > 4 && application.Name.Contains (" "))
				guesses.Add (application.Name.Split (' ') [0].ToLower ());
			
			string exec;
			exec = GetExecStringForPID (application.Pid);
			if (string.IsNullOrEmpty (exec))
				exec = GetExecStringForPID (application.Windows[0].Pid);
			
			
			if (!string.IsNullOrEmpty (exec)) {
				guesses.Add (exec);
				guesses.Add (exec.Split ('-')[0]);
			}
			
			Gdk.Pixbuf pbuf = null;
			foreach (string guess in guesses) {
				string icon_guess = guess;
				if (pbuf != null) {
					pbuf.Dispose ();
					pbuf = null;
				}
				
				bool found = IconProvider.PixbufFromIconName (icon_guess, DockPreferences.FullIconSize, out pbuf);
				if (found && (pbuf.Width == DockPreferences.FullIconSize || 
				                     pbuf.Height == DockPreferences.FullIconSize)) {
					return pbuf;
				} else {
					pbuf.Dispose ();
					pbuf = null;
				}
			
				string desktop_path = GetDesktopFile (icon_guess);
				if (!string.IsNullOrEmpty (desktop_path)) {
					Gnome.DesktopItem di = Gnome.DesktopItem.NewFromFile (desktop_path, Gnome.DesktopItemLoadFlags.OnlyIfExists);
					if (pbuf != null)
						pbuf.Dispose ();
					pbuf = IconProvider.PixbufFromIconName (di.GetString ("Icon"), DockPreferences.FullIconSize);
					di.Dispose ();
					return pbuf;
				}
			}
			
			if (pbuf == null)
				pbuf = IconProvider.PixbufFromIconName (guesses[0], DockPreferences.FullIconSize);
			
			if (pbuf.Height != DockPreferences.FullIconSize && pbuf.Width != DockPreferences.FullIconSize) {
				double scale = (double)DockPreferences.FullIconSize / Math.Max (pbuf.Width, pbuf.Height);
				Gdk.Pixbuf temp = pbuf.ScaleSimple ((int) (pbuf.Width * scale), (int) (pbuf.Height * scale), Gdk.InterpType.Bilinear);
				pbuf.Dispose ();
				pbuf = temp;
			}
			return pbuf;
		}
		
		string GetExecStringForPID (int pid)
		{
			string exec;
			try {
				// this fails on mono pre 2.0
				exec = System.Diagnostics.Process.GetProcessById (pid).ProcessName.Split (' ')[0];
			} catch { exec = null; }
			
			if (string.IsNullOrEmpty (exec)) {
				try {
					// this works on all versions of mono but is less reliable (because I wrote it)
					exec = WindowUtils.CmdLineForPid (pid).Split (' ')[0];
				} catch { }
			}
			return exec;
		}
		
		string GetDesktopFile (string base_name)
		{
			foreach (string dir in DesktopFilesDirectories) {
				try {
					if (File.Exists (System.IO.Path.Combine (dir, base_name+".desktop")))
						return System.IO.Path.Combine (dir, base_name+".desktop");
					if (File.Exists (System.IO.Path.Combine (dir, "gnome-"+base_name+".desktop")))
						return System.IO.Path.Combine (dir, "gnome-"+base_name+".desktop");
				} catch { return null; }
			}
			return null;
		}
		
		public override  string Description {
			get {
				return application.Name;
			}
		}
		
		public override int WindowCount {
			get {
				return application.Windows.Where (w => !w.IsSkipTasklist).Count ();
			}
		}

		
		public override bool Scalable {
			get {
				return true;
			}
		}
		
		public Wnck.Application App {
			get { return application; }
		}
		
		#endregion 
		
		public ApplicationDockItem (Wnck.Application application) : base ()
		{
			this.application = application;
		}
		
		protected override void OnIconSizeChanged ()
		{
			if (icon_surface != null) {
				icon_surface.Destroy ();
				icon_surface = null;
			}
			
			base.OnIconSizeChanged ();
		}

		
		public override void Clicked (uint button)
		{
			if (button == 1)
				WindowUtils.PerformLogicalClick (new Wnck.Application[] {application});
		}

		public override void SetIconRegion (Gdk.Rectangle region)
		{
			if (icon_region == region)
				return;
			icon_region = region;
			
			foreach (Wnck.Window window in application.Windows) {
				window.SetIconGeometry (region.X, region.Y, region.Width, region.Height);
			}
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
		
		#endregion 
		
		
		public override bool Equals (IDockItem other)
		{
			if (!(other is ApplicationDockItem))
				return false;
			
			return ((other as ApplicationDockItem).application == application);
		}
		
		public IEnumerable<MenuArgs> GetMenuItems ()
		{
			List<MenuArgs> outList = new List<MenuArgs> ();
			foreach (Wnck.Window window in App.Windows) { 
				Wnck.Window copy_win = window;
				if (!copy_win.IsSkipTasklist) {
					string name = copy_win.Name;
					if (name.Length > 50)
						name = name.Substring (0, 47) + "...";
					outList.Add (new MenuArgs ((o, a) => copy_win.CenterAndFocusWindow (), name, "forward", true));
				}
			}
			
			if (outList.Any ()) {
				outList.Add (new SeparatorMenuArgs ());
				outList.Add (new MenuArgs ((o, a) => WindowControl.MinimizeRestoreWindows (App.Windows), 
				                           "Minimize/Restore", 
				                           "down",
				                           true));
				outList.Add (new MenuArgs ((o, a) => WindowControl.CloseWindows (App.Windows), "Close All", Gtk.Stock.Quit, true));
			}
			
			return outList;
		}
	}
}
