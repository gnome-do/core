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
		
		string MinimizeRestoreText = Catalog.GetString ("Minimize") + "/" + Catalog.GetString ("Restore");
		string MaximizeText = Catalog.GetString ("Maximize");
		string CloseText = Catalog.GetString ("Close All");
		
		const int MenuItemMaxCharacters = 50;
		
		string MaximizeIcon {
			get { return "maximize.svg@" + GetType ().Assembly.FullName; }
		}
		
		string MinimizeIcon {
			get { return "minimize.svg@" + GetType ().Assembly.FullName; }
		}
		
		string CloseIcon {
			get { return "close.svg@" + GetType ().Assembly.FullName; }
		}
		
		string WindowIcon {
			get {
				if (Launcher == null)
					return "forward";
				return Launcher.Icon;
			}
		}
		
		int windowCount;
		
		Gdk.Pixbuf drag_pixbuf;
		
		IEnumerable<Wnck.Window> windows;
		
		IApplicationItem launcher;
		
		public IApplicationItem Launcher {
			get {
				if (launcher == null && Exec != null) {
					string command = WindowUtils.ProcessExecString (Exec);
					launcher = Services.UniverseFactory.MaybeApplicationItemFromCommand (command);
				}
				return launcher;
			}
		}
		
		public override Item Item {
			get {
				return Launcher as Item;
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
			
			if (Launcher == null) {
				foreach (string guess in GetIconGuesses ()) {
					bool found = IconProvider.PixbufFromIconName (guess, size, out pbuf);
					if (found && (pbuf.Width == size || pbuf.Height == size))
						break;
					
					pbuf.Dispose ();
					pbuf = null;
				}
			} else {
				IconProvider.PixbufFromIconName (Launcher.Icon, size, out pbuf);
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
				if (!VisibleWindows.Any ())
					return "Unknown";
				
				if (NeedsAttention) {
					return VisibleWindows.Where (w => w.NeedsAttention ()).First ().Name;
				}
				if (VisibleWindows.Any () && VisibleWindows.Count () == 1) {
					return VisibleWindows.First ().Name;
				}
				
				foreach (Wnck.Window window in VisibleWindows) {
					if (window.ClassGroup != null) {
						if (StringIsValidName (window.ClassGroup.ResClass))
							return window.ClassGroup.ResClass;
					}
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
			
			SetText (Name);
		}
		
		IEnumerable<string> GetIconGuesses ()
		{
			List<string> guesses = new List<string> ();
			
			// open office hack...
			if (VisibleWindows.Any () &&
			    VisibleWindows.First ().ClassGroup != null &&
			    VisibleWindows.First ().ClassGroup.ResClass.ToLower ().Contains ("openoffice")) {
				yield return "openoffice";
				yield break;
			}
			
			string exec = Exec;
			if (!string.IsNullOrEmpty (exec)) {
				yield return exec;
				yield return exec.Split ('-')[0];
				yield return WindowUtils.ProcessExecString (exec);
			}

			foreach (Wnck.Window win in VisibleWindows) {
				if (!guesses.Contains (PrepName (win.Name)))
					guesses.Add (PrepName (win.Name));
				
				if (!guesses.Contains (PrepName (win.IconName)))
					guesses.Add (PrepName (win.IconName));
				
				if (win.ClassGroup == null)
					continue;
				
				if (!guesses.Contains (PrepName (win.ClassGroup.ResClass)))
					guesses.Add (PrepName (win.ClassGroup.ResClass));
				
				if (!guesses.Contains (PrepName (win.ClassGroup.Name)))
					guesses.Add (PrepName (win.ClassGroup.Name));
			}
			
			foreach (string s in guesses) {
				yield return s;
			}
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
		
		bool StringIsValidName (string s)
		{
			s = s.Trim ();
			if (string.IsNullOrEmpty (s) || s == "<unknown>")
				return false;
			
			foreach (System.Text.RegularExpressions.Regex prefix in WindowUtils.BadPrefixes) {
				if (prefix.IsMatch (s))
					return false;
			}
			
			return true;
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
			
			Item item = Launcher as Item;
			if (item == null) {
				yield return new SimpleMenuButtonArgs (() => WindowControl.MinimizeRestoreWindows (VisibleWindows), 
				                                       MinimizeRestoreText, MinimizeIcon).AsDark ();
				
				yield return new SimpleMenuButtonArgs (() => WindowControl.MaximizeWindow (VisibleWindows.First ()), 
				                                       MaximizeText, MaximizeIcon).AsDark ();
				
				yield return new SimpleMenuButtonArgs (() => WindowControl.CloseWindows (VisibleWindows), 
				                                       CloseText, CloseIcon).AsDark ();
			} else {
				foreach (Act act in ActionsForItem (item))
					yield return new LaunchMenuButtonArgs (act, item, act.Name, act.Icon).AsDark ();
			}
			
			foreach (Wnck.Window window in VisibleWindows) {
				yield return new SeparatorMenuButtonArgs ();
				yield return new WindowMenuButtonArgs (window, window.Name, WindowIcon);
			}
		}
		
		protected override void Launch ()
		{
			if (Launcher != null)
				Launcher.Run ();
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
