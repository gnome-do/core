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

using Docky.Interface.Menus;
using Docky.Utilities;

namespace Docky.Interface
{
	
	
	public class ApplicationDockItem : WnckDockItem, IRightClickable
	{
		public event EventHandler RemoveClicked;
		
		static readonly IEnumerable<string> DesktopFilesDirectories = new [] {
				"~/.local/share/applications/wine",
				"~/.local/share/applications",
				"/usr/share/applications",
				"/usr/share/applications/kde",
				"/usr/share/applications/kde4",
				"/usr/share/gdm/applications",
				"/usr/local/share/applications",
		};
		
		string MinimizeRestoreText = Catalog.GetString ("Minimize") + "/" + Catalog.GetString ("Restore");
		string CloseText = Catalog.GetString ("Close All");
		
		const int MenuItemMaxCharacters = 50;
		const string WindowIcon = "forward";
		const string MinimizeIcon = "down";
		
		int windowCount;
		
		Gdk.Rectangle icon_region;
		Gdk.Pixbuf drag_pixbuf;
		
		IEnumerable<Wnck.Application> applications;
		
		string desktop_file;
		
		public string DesktopFile {
			get {
				if (desktop_file == null) {
					foreach (string s in GetIconGuesses ()) {
						desktop_file = GetDesktopFile (s);
						if (desktop_file != null)
							break;
					}
				}
				return desktop_file;
			}
		}
		
		string Exec {
			get {
				string exec;
				foreach (Wnck.Application app in Applications) {
					exec = MaybeGetExecStringForPID (app.Pid);
					if (exec != null)
						return exec;
				}

				foreach (Wnck.Window win in VisibleWindows) {
					exec = MaybeGetExecStringForPID (win.Pid);
					if (exec != null)
						return exec;
				}
				return null;
			}
		}
		
		public override Pixbuf GetDragPixbuf ()
		{
			if (drag_pixbuf == null)
				drag_pixbuf = GetSurfacePixbuf (DockPreferences.FullIconSize);
			return drag_pixbuf;
		}
		
		/// <summary>
		/// Returns a Pixbuf suitable for usage in the dock.
		/// </summary>
		/// <returns>
		/// A <see cref="Gdk.Pixbuf"/>
		/// </returns>
		protected override Gdk.Pixbuf GetSurfacePixbuf (int size)
		{
			Gdk.Pixbuf pbuf = null;
			foreach (string guess in GetIconGuesses ()) {
				if (pbuf != null) {
					pbuf.Dispose ();
					pbuf = null;
				}
				
				bool found = IconProvider.PixbufFromIconName (guess, size, out pbuf);
				if (found && (pbuf.Width == size || pbuf.Height == size))
					break;
				
				pbuf.Dispose ();
				pbuf = null;
			
				string desktopPath = GetDesktopFile (guess);
				if (!string.IsNullOrEmpty (desktopPath)) {
					try {
						string icon = Services.UniverseFactory.NewApplicationItem (desktopPath).Icon;
						pbuf = IconProvider.PixbufFromIconName (icon, size);
						break;
					} catch {
						continue;
					}
				}
			}
			
			// we failed to find an icon, lets use an uggggly one
			if (pbuf == null)
				pbuf = Applications.First ().Icon;
			
			if (pbuf.Height != size && pbuf.Width != size ) {
				double scale = (double)size / Math.Max (pbuf.Width, pbuf.Height);
				Gdk.Pixbuf temp = pbuf.ScaleSimple ((int) (pbuf.Width * scale), (int) (pbuf.Height * scale), Gdk.InterpType.Hyper);
				pbuf.Dispose ();
				pbuf = temp;
			}
			return pbuf;
		}
		
		string Name {
			get {
				foreach (Wnck.Application application in Applications) {
					if (StringIsValidName (application.IconName))
						return application.IconName;
					else if (StringIsValidName (application.Name))
						return application.Name;
				}

				foreach (Wnck.Window window in VisibleWindows) {
					if (StringIsValidName (window.IconName))
						return window.IconName;
					else if (StringIsValidName (window.Name))
						return window.Name;
				}
				return "Unknown";
			}
		}
		
		public override int WindowCount {
			get { return windowCount; }
		}
		
		protected override IEnumerable<Wnck.Application> Applications { 
			get { return applications; } 
		}

		public ApplicationDockItem (IEnumerable<Wnck.Application> applications) : base ()
		{
			this.applications = applications;
			windowCount = VisibleWindows.Count ();
			AttentionRequestStartTime = DateTime.UtcNow - new TimeSpan (0, 10, 0);
			
			foreach (Wnck.Window w in VisibleWindows) {
				w.StateChanged += HandleStateChanged;
				w.NameChanged += HandleNameChanged;
			}

			base.SetText (Name);
		}

		void HandleNameChanged(object sender, EventArgs e)
		{
			SetText (Name);
		}

		void HandleStateChanged(object o, Wnck.StateChangedArgs args)
		{
			bool tmp = NeedsAttention;
			NeedsAttention = DetermineUrgencyStatus ();
			if (NeedsAttention != tmp) {
				UpdateRequestType req = (NeedsAttention) ? UpdateRequestType.NeedsAttentionSet : UpdateRequestType.NeedsAttentionUnset;
				if (NeedsAttention)
					AttentionRequestStartTime = DateTime.UtcNow;
				OnUpdateNeeded (new UpdateRequestArgs (this, req));
			}
		}
		
		IEnumerable<string> GetIconGuesses ()
		{
			List<string> guesses = new List<string> ();
			
			if (!string.IsNullOrEmpty (Exec)) {
				yield return Exec;
				yield return Exec.Split ('-')[0];
			}
			
			foreach (Wnck.Application app in Applications) {
				if (!guesses.Contains (PrepName (app.Name)))
					guesses.Add (PrepName (app.Name));
				if (!guesses.Contains (PrepName (app.IconName)))
					guesses.Add (PrepName (app.IconName));
			}

			foreach (Wnck.Window win in VisibleWindows) {
				if (!guesses.Contains (PrepName (win.Name)))
					guesses.Add (PrepName (win.Name));
				if (!guesses.Contains (PrepName (win.IconName)))
					guesses.Add (PrepName (win.IconName));
				if (!guesses.Contains (PrepName (win.ClassGroup.ResClass)))
					guesses.Add (PrepName (win.ClassGroup.ResClass));
			}
			
			foreach (string s in guesses)
				yield return s;
			
			foreach (string s in guesses)
				yield return "gnome-" + s;
			
			if (Name.Length > 4 && Name.Contains (" "))
				yield return Name.Split (' ') [0].ToLower ();
		}

		string PrepName (string s)
		{
			return s.ToLower ().Replace (' ', '-');
		}
		
		string MaybeGetExecStringForPID (int pid)
		{
			string exec = null;
			
			try {
				exec = WindowUtils.ProcessExecString (WindowUtils.CmdLineForPid (pid));
			} catch { }
		
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
		
		bool StringIsValidName (string s)
		{
			return (!string.IsNullOrEmpty (s.Trim ()) && s != "<unknown>");
		}

		public override void SetIconRegion (Gdk.Rectangle region)
		{
			if (icon_region == region)
				return;
			icon_region = region;

			VisibleWindows.ForEach (w => w.SetIconGeometry (region.X, region.Y, region.Width, region.Height));
		}
		
		public override bool Equals (AbstractDockItem other)
		{
			if (!(other is ApplicationDockItem))
				return false;

			return Applications.Any (app => (other as ApplicationDockItem).Applications.Contains (app));
		}
		
		public IEnumerable<AbstractMenuButtonArgs> GetMenuItems ()
		{
			foreach (Wnck.Window window in VisibleWindows)
				yield return new WindowMenuButtonArgs (window, window.Name, WindowIcon);
			
			yield return new SeparatorMenuButtonArgs ();
			
			yield return new SimpleMenuButtonArgs (() => WindowControl.MinimizeRestoreWindows (VisibleWindows), 
			                                       MinimizeRestoreText, MinimizeIcon);
			
			yield return new SimpleMenuButtonArgs (() => WindowControl.CloseWindows (VisibleWindows), 
			                                       CloseText, Gtk.Stock.Quit);
		}

		public override void Dispose ()
		{
			foreach (Wnck.Window w in VisibleWindows) {
				w.StateChanged -= HandleStateChanged;
				w.NameChanged -= HandleNameChanged;
			}
			
			base.Dispose ();
		}

	}
}
