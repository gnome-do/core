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

using Mono.Unix;

using Do.Interface.CairoUtils;
using Do.Platform;
using Do.Interface;
using Do.Universe;

using Docky.Utilities;

namespace Docky.Interface
{
	
	
	public class ApplicationDockItem : AbstractDockItem, IRightClickable
	{
		public event EventHandler RemoveClicked;
		
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
		
		string MinimizeRestoreText = Catalog.GetString ("Minimize") + "/" + Catalog.GetString ("Restore");
		string CloseText = Catalog.GetString ("Close All");
		
		const int MenuItemMaxCharacters = 50;
		
		Gdk.Rectangle icon_region;
		
		string Exec {
			get {
				return MaybeGetExecStringForPID (Application.Pid) ?? MaybeGetExecStringForPID (Application.Windows[0].Pid);
			}
		}
		
		#region IDockItem implementation 
		
		public override Pixbuf GetDragPixbuf ()
		{
			return Application.Icon;
		}

		/// <summary>
		/// Returns a Pixbuf suitable for usage in the dock.
		/// </summary>
		/// <returns>
		/// A <see cref="Gdk.Pixbuf"/>
		/// </returns>
		protected override Gdk.Pixbuf GetSurfacePixbuf ()
		{
			Gdk.Pixbuf pbuf = null;
			foreach (string guess in GetIconGuesses ()) {
				if (pbuf != null) {
					pbuf.Dispose ();
					pbuf = null;
				}
				
				bool found = IconProvider.PixbufFromIconName (guess, DockPreferences.FullIconSize, out pbuf);
				if (found && (pbuf.Width == DockPreferences.FullIconSize || pbuf.Height == DockPreferences.FullIconSize))
					break;
				
				pbuf.Dispose ();
				pbuf = null;
			
				string desktopPath = GetDesktopFile (guess);
				if (!string.IsNullOrEmpty (desktopPath)) {
					using (Gnome.DesktopItem di = Gnome.DesktopItem.NewFromFile (desktopPath, Gnome.DesktopItemLoadFlags.OnlyIfExists)) {
						pbuf = IconProvider.PixbufFromIconName (di.GetString ("Icon"), DockPreferences.FullIconSize);
					}
					break;
				}
			}
			
			// we failed to find an icon, lets use an uggggly one
			if (pbuf == null)
				pbuf = Application.Icon;
			
			if (pbuf.Height != DockPreferences.FullIconSize && pbuf.Width != DockPreferences.FullIconSize) {
				double scale = (double)DockPreferences.FullIconSize / Math.Max (pbuf.Width, pbuf.Height);
				Gdk.Pixbuf temp = pbuf.ScaleSimple ((int) (pbuf.Width * scale), (int) (pbuf.Height * scale), Gdk.InterpType.Hyper);
				pbuf.Dispose ();
				pbuf = temp;
			}
			return pbuf;
		}
		
		IEnumerable<string> GetIconGuesses ()
		{
			string [] guesses = new [] { Application.Name.ToLower ().Replace (' ','-'),
				                         Application.Windows[0].Name.ToLower ().Replace (' ','-'),
				                         Application.IconName.ToLower ().Replace (' ','-'),
				                         Application.Windows[0].IconName.ToLower ().Replace (' ','-') };
			foreach (string s in guesses)
				yield return s;
			
			foreach (string s in guesses)
				yield return "gnome-" + s;
			
			if (Application.Name.Length > 4 && Application.Name.Contains (" "))
				yield return Application.Name.Split (' ') [0].ToLower ();
			
			if (!string.IsNullOrEmpty (Exec)) {
				yield return Exec;
				yield return Exec.Split ('-')[0];
			}
		}
		
		string MaybeGetExecStringForPID (int pid)
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
			if (exec == "")
				exec = null;
			
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
				return Application.Name;
			}
		}
		
		public override int WindowCount {
			get {
				return Application.Windows.Where (w => !w.IsSkipTasklist).Count ();
			}
		}
		
		Wnck.Application Application {
			get; set;
		}
		
		#endregion 
		
		public ApplicationDockItem (Wnck.Application application) : base ()
		{
			Application = application;
		}
		
		public override void Clicked (uint button)
		{
			if (button == 1)
				WindowUtils.PerformLogicalClick (new [] {Application});
		}

		public override void SetIconRegion (Gdk.Rectangle region)
		{
			if (icon_region == region)
				return;
			icon_region = region;
			
			foreach (Wnck.Window window in Application.Windows) {
				window.SetIconGeometry (region.X, region.Y, region.Width, region.Height);
			}
		}
		
		public override bool Equals (IDockItem other)
		{
			if (!(other is ApplicationDockItem))
				return false;
			
			return ((other as ApplicationDockItem).Application == Application);
		}
		
		public IEnumerable<AbstractMenuButtonArgs> GetMenuItems ()
		{
			foreach (Wnck.Window window in Application.Windows.Where (win => !win.IsSkipTasklist))
				yield return new WindowMenuButtonArgs (window, window.Name, "forward");
			
			yield return new SeparatorMenuButtonArgs ();
			yield return new SimpleMenuButtonArgs (() => WindowControl.MinimizeRestoreWindows (Application.Windows), MinimizeRestoreText, "down");
			yield return new SimpleMenuButtonArgs (() => WindowControl.CloseWindows (Application.Windows), CloseText, Gtk.Stock.Quit);
		}
	}
}
