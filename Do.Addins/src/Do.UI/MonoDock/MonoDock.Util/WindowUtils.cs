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
using System.IO;
using System.Linq;

using Do.Addins;
using Do.Universe;
using Do.Platform;

using MonoDock.UI;

using Wnck;

namespace MonoDock.Util
{
	
	
	public static class WindowUtils
	{
		public static Application[] GetApplications ()
		{
			List<Application> apps = new List<Application> ();
			foreach (Window w in Wnck.Screen.Default.Windows) {
				if (!apps.Contains (w.Application))
					apps.Add (w.Application);
			}
			return apps.ToArray ();
		}
		
		public static string CmdLineForPid (int pid)
		{
			StreamReader reader;
			try {
				string procPath = new [] { "/proc", pid.ToString (), "cmdline" }.Aggregate (Path.Combine);
				reader = new StreamReader (procPath);
			} catch { return null; }
			
			string cmdline = reader.ReadLine ();
			reader.Close ();
			reader.Dispose ();
			return cmdline;
		}
		
		public static List<Application> GetApplicationList (string exec)
		{
			exec = exec.Split (' ')[0];
			List<Application> apps = new List<Application> ();
			Application out_app = null;
			foreach (string dir in Directory.GetDirectories ("/proc")) {
				int pid;
				out_app = null;
				try { pid = Convert.ToInt32 (dir.Substring (6)); } 
				catch { continue; }
				
				string exec_line = CmdLineForPid (pid);
				if (string.IsNullOrEmpty (exec_line))
					continue;
				
				if (exec_line.Contains (exec)) {
					foreach (Application app in GetApplications ()) {
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
		
		public static void CenterAndFocusWindow (this Window w) 
		{
			if (!w.IsInViewport (Wnck.Screen.Default.ActiveWorkspace)) {
				int viewX, viewY, viewW, viewH;
				int midX, midY;
				Screen scrn = Screen.Default;
				Workspace wsp = scrn.ActiveWorkspace;
				
				//get our windows geometry
				w.GetGeometry (out viewX, out viewY, out viewW, out viewH);
				
				//we want to focus on where the middle of the window is
				midX = viewX + (viewW / 2);
				midY = viewY + (viewH / 2);
				
				//The positions given above are relative to the current viewport
				//This makes them absolute
				midX += wsp.ViewportX;
				midY += wsp.ViewportY;
				
				//Check to make sure our middle didn't wrap
				if (midX > wsp.Width) {
					midX %= wsp.Width;
				}
				
				if (midY > wsp.Height) {
					midY %= wsp.Height;
				}
				
				//take care of negative numbers (happens?)
				while (midX < 0)
					midX += wsp.Width;
			
				while (midY < 0)
					midX += wsp.Height;
				
				Wnck.Screen.Default.MoveViewport (midX, midY);
			}
			
			w.Activate (Gtk.Global.CurrentEventTime);
		}
		
		public static void PerformLogicalClick (IEnumerable<Application> apps)
		{
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
					switch (GetClickAction (apps)) {
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
