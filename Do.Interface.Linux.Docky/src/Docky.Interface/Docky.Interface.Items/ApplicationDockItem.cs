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
using Do.Interface.Wink;
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
		string MaximizeText = Catalog.GetString ("Maximize");
		string CloseText = Catalog.GetString ("Close All");
		
		const int MenuItemMaxCharacters = 50;
		const string WindowIcon = "forward";
		
		string MaximizeIcon {
			get { return "maximize.svg@" + GetType ().Assembly.FullName; }
		}
		
		string MinimizeIcon {
			get { return "minimize.svg@" + GetType ().Assembly.FullName; }
		}
		
		string CloseIcon {
			get { return "close.svg@" + GetType ().Assembly.FullName; }
		}
		
		int windowCount;
		
		Gdk.Rectangle icon_region;
		Gdk.Pixbuf drag_pixbuf;
		
		IEnumerable<Wnck.Window> windows;
		
		string desktop_file;
		bool checked_desktop_file;
		
		public string DesktopFile {
			get {
				if (desktop_file == null && !checked_desktop_file) {
					checked_desktop_file = true;
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
				pbuf = Windows.First ().Icon;
			
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
				foreach (Wnck.Window window in VisibleWindows) {
					if (StringIsValidName (window.ClassGroup.ResClass))
						return window.ClassGroup.ResClass;
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
		
		public override IEnumerable<Wnck.Window> Windows { 
			get { return windows; } 
		}

		public ApplicationDockItem (IEnumerable<Wnck.Window> windows) : base ()
		{
			this.windows = windows;
			windowCount = VisibleWindows.Count ();
			
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
			if ((args.ChangedMask & Wnck.WindowState.SkipTasklist) == Wnck.WindowState.SkipTasklist) {
				if (VisibleWindows.Count () == 0)
					Core.DockServices.ItemsService.ForceUpdate ();
			}
		}
		
		IEnumerable<string> GetIconGuesses ()
		{
			List<string> guesses = new List<string> ();
			
			if (!string.IsNullOrEmpty (Exec)) {
				yield return Exec;
				yield return Exec.Split ('-')[0];
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
			s = s.Trim ();
			if (string.IsNullOrEmpty (s) || s == "<unknown>")
				return false;
			
			foreach (string prefix in WindowUtils.BadPrefixes) {
				if (string.Compare (s, prefix, true) == 0)
					return false;
			}
			
			return true;
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

			return Windows.Any (w => (other as ApplicationDockItem).Windows.Contains (w));
		}
		
		public IEnumerable<AbstractMenuArgs> GetMenuItems ()
		{
			yield return new SeparatorMenuButtonArgs ();
			
			if (DesktopFile == null) {
				yield return new SimpleMenuButtonArgs (() => WindowControl.MinimizeRestoreWindows (VisibleWindows), 
				                                       MinimizeRestoreText, MinimizeIcon).AsDark ();
				
				yield return new SimpleMenuButtonArgs (() => WindowControl.MaximizeWindow (VisibleWindows.First ()), 
				                                       MaximizeText, MaximizeIcon).AsDark ();
				
				yield return new SimpleMenuButtonArgs (() => WindowControl.CloseWindows (VisibleWindows), 
				                                       CloseText, CloseIcon).AsDark ();
			} else {
				Item item = Services.UniverseFactory.NewApplicationItem (DesktopFile) as Item;
				
				foreach (Act act in ActionsForItem (item))
					yield return new LaunchMenuButtonArgs (act, item, act.Name, act.Icon).AsDark ();
			}
			
			foreach (Wnck.Window window in VisibleWindows) {
				yield return new SeparatorMenuButtonArgs ();
				yield return new WindowMenuButtonArgs (window, window.Name, WindowIcon);
			}
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
