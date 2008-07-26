// PositionWindow.cs
//
//GNOME Do is the legal property of its developers. Please refer to the
//COPYRIGHT file distributed with this
//source distribution.
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
//
//

using System;
using Gdk;
using Gtk;

namespace Do.UI
{
	
	
	public class PositionWindow
	{
		private int monitor;
		private Gtk.Window w, r;
		
		public PositionWindow (Gtk.Window main, Gtk.Window results) {
			this.w = main;
			this.r = results;
			
			Rectangle point;
			Display disp = w.Screen.Display;
			disp.GetPointer(out point.X, out point.Y);
			
			monitor = w.Screen.GetMonitorAtPoint (point.X, point.Y);
		}
		
		public void UpdatePosition (int iconboxWidth, Pane currentPane, 
		                            Rectangle resultsOffset) {
			Gdk.Rectangle geo, main, results;
			
			w.GetPosition (out main.X, out main.Y);
			w.GetSize (out main.Width, out main.Height);
			
			//only change monitors if we are currently not showing the window
			if (!w.Visible) {
				GetMonitor ();
			}

			geo = w.Screen.GetMonitorGeometry (monitor);
			main.X = ((geo.Width - main.Width) / 2) + geo.X;
			main.Y = (int)((geo.Height + geo.Y - main.Height) / 2.5) + geo.Y;
			w.Move (main.X, main.Y);
			
			//position resultsWindow
			//set to false for testing purposes
			if (true) {
				r.GetSize (out results.Width, out results.Height);
				results.Y = main.Y + main.Height + resultsOffset.Y;
				results.X = main.X + iconboxWidth * (int) currentPane + resultsOffset.X;
				r.Move (results.X, results.Y);
			} else {
				r.GetSize (out results.Width, out results.Height);
				results.Y = main.Y + main.Height + resultsOffset.Y;
				results.X = ((geo.Width - results.Width) / 2) + geo.X;
				r.Move (results.X, results.Y);
			}
			
			//uncomment here to get debug output
			//Console.WriteLine ("Repostion Started For Montior {0}:", monitor);
			//Console.WriteLine ("Geo: Width-{0}, Height-{1}, X-{2}, Y-{3}",geo.Width, geo.Height, geo.X, geo.Y);
			//Console.WriteLine ("Main: Width-{0}, Height-{1}, X-{2}, Y-{3}",main.Width, main.Height, main.X, main.Y);
		}
		
		public bool GetMonitor () {
			Rectangle point;
			int tmp;
			tmp = monitor;
			
			Display disp = w.Screen.Display;
			disp.GetPointer(out point.X, out point.Y);
			
			monitor = w.Screen.GetMonitorAtPoint (point.X, point.Y);
			
			if (tmp == monitor)
				return false;
			else
				return true;
		}
	}
}
