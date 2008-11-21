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

using Do.Addins;
using Do.Universe;
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
		
		public static Application GetApplication (string exec)
		{
			exec = exec.Split (' ')[0];
			Application out_app = null;
			StreamReader reader;
			foreach (string dir in Directory.GetDirectories ("/proc")) {
				int pid;
				try { pid = Convert.ToInt32 (dir.Substring (6)); } 
				catch { continue; }
				
				reader = new StreamReader (Do.Paths.Combine ("/proc", dir, "cmdline"));
				
				string exec_line = reader.ReadLine ();
				if (string.IsNullOrEmpty (exec_line))
					continue;
				
				if (exec_line.Contains (exec)) {
					foreach (Application app in GetApplications ()) {
						if (app.Pid == pid) {
							out_app = app;
							break;
						}
					}
				}
				
				if (out_app != null)
					break;
			}
			
			return out_app;
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
	}
}
