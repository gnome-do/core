// WindowUtils.cs
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
using System.Diagnostics;
using System.IO;
using System.Linq;

using Do.Interface;
using Do.Universe;
using Do.Platform;

using Docky.Interface;

using Wnck;

namespace Docky.Utilities
{
	
	
	public static class WindowUtils
	{
		static IEnumerable<string> BadPrefixes {
			get {
				yield return "gksu";
				yield return "sudo";
				yield return "java";
				yield return "python";
				yield return "python2.4";
				yield return "python2.5";
			}
		}
		
		static List<Application> application_list;
		static bool application_list_update_needed;
		
		static WindowUtils ()
		{
			Wnck.Screen.Default.WindowClosed += delegate {
				application_list_update_needed = true;
			};
			
			Wnck.Screen.Default.WindowOpened += delegate {
				application_list_update_needed = true;
			};
			
			Wnck.Screen.Default.ApplicationOpened += delegate {
				application_list_update_needed = true;
			};
			
			Wnck.Screen.Default.ApplicationClosed += delegate {
				application_list_update_needed = true;
			};
		}
		
		/// <summary>
		/// Returns a list of all applications on the default screen
		/// </summary>
		/// <returns>
		/// A <see cref="Application"/> array
		/// </returns>
		public static List<Application> GetApplications ()
		{
			if (application_list == null || application_list_update_needed) {
				application_list = new List<Application> ();
				foreach (Window w in Wnck.Screen.Default.Windows) {
					if (!application_list.Contains (w.Application))
						application_list.Add (w.Application);
				}
			}
			return application_list;
		}
		
		/// <summary>
		/// Gets the command line excec string for a PID
		/// </summary>
		/// <param name="pid">
		/// A <see cref="System.Int32"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public static string CmdLineForPid (int pid)
		{
			StreamReader reader;
			string cmdline = null;
			
			try {
				string procPath = new [] { "/proc", pid.ToString (), "cmdline" }.Aggregate (Path.Combine);
				reader = new StreamReader (procPath);
				cmdline = reader.ReadLine ().Replace (Convert.ToChar (0x0), ' ');
				reader.Close ();
				reader.Dispose ();
			} catch { }
			
			return cmdline;
		}
		
		/// <summary>
		/// Returns a list of applications that match an exec string
		/// </summary>
		/// <param name="exec">
		/// A <see cref="System.String"/>
		/// </param>
		/// <returns>
		/// A <see cref="List"/>
		/// </returns>
		public static List<Application> GetApplicationList (string exec)
		{
			List<Application> apps = new List<Application> ();
			if (string.IsNullOrEmpty (exec))
				return apps;
			
			exec = ProcessExecString (exec);

			Application out_app = null;
			foreach (string dir in Directory.GetDirectories ("/proc")) {
				int pid;
				out_app = null;
				try { pid = Convert.ToInt32 (Path.GetFileName (dir)); } 
				catch { continue; }
				
				string exec_line = CmdLineForPid (pid);
				if (string.IsNullOrEmpty (exec_line))
					continue;

				exec_line = ProcessExecString (exec_line);

				if (exec_line.Contains (exec)) {
					foreach (Application app in GetApplications ()) {
						if (app == null)
							continue;
						
						if (app.Pid == pid) {
							if (app.Windows.Select (win => !win.IsSkipTasklist).Any ())
								out_app = app;
							break;
						}
					}
				}
				
				if (out_app != null)
					apps.Add (out_app);
			}
			return apps;
		}

		public static string ProcessExecString (string exec)
		{
			string [] parts = exec.Split (' ');
			for (int i = 0; i < parts.Length; i++) {
				if (parts [i].StartsWith ("-"))
					continue;
				
				if (parts [i].Contains ("/"))
					parts [i] = parts [i].Split ('/').Last ();
				
				foreach (string prefix in BadPrefixes) {
					if (parts [i].StartsWith (prefix)) {
						parts [i] = parts [i].Substring (prefix.Length);
					}
				}
				
				if (!string.IsNullOrEmpty (parts [i].Trim ())) {
					return parts [i].ToLower ();
				}
			}
			return null;
		}
		
		/// <summary>
		/// Performs the "logical" click action on an entire group of applications
		/// </summary>
		/// <param name="apps">
		/// A <see cref="IEnumerable"/>
		/// </param>
		public static void PerformLogicalClick (IEnumerable<Application> apps)
		{
			bool not_in_viewport = !apps.SelectMany (app => app.Windows)
				.Any (w => !w.IsSkipTasklist && w.IsInViewport (w.Screen.ActiveWorkspace));
			
			bool urgent = apps.Any (app => app.Windows.Any (w => w.NeedsAttention ()));
			
			if (not_in_viewport || urgent) {
				foreach (Wnck.Application application in apps) {
					foreach (Wnck.Window window in application.Windows) {
						if (urgent && !window.NeedsAttention ())
								continue;
						if (!window.IsSkipTasklist) {
							window.CenterAndFocusWindow ();
							return;
						}
					}
				}
			}
			
			List<Window> windows = new List<Window> ();
			foreach (Wnck.Application app in apps)
				windows.AddRange (app.Windows);

			switch (GetClickAction (apps)) {
			case ClickAction.Focus:
				WindowControl.FocusWindows (windows);
				break;
			case ClickAction.Minimize:
				WindowControl.MinimizeWindows (windows);
				break;
			case ClickAction.Restore:
				WindowControl.RestoreWindows (windows);
				break;
			}
		}
		
		static ClickAction GetClickAction (IEnumerable<Application> apps)
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
	}
}
