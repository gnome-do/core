// PositionWindow.cs
//
// GNOME Do is the legal property of its developers. Please refer to the
// COPYRIGHT file distributed with this source distribution.
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

using Do.Platform;

namespace Do.Interface
{
	
	
	public class PositionWindow
	{
		Gtk.Window w, r;
		
		public PositionWindow (Gtk.Window main, Gtk.Window results)
		{
			this.w = main;
			this.r = results;
		}

		int GetMonitor ()
		{
			Display disp;
			Rectangle point;

			disp = w.Screen.Display;
			disp.GetPointer(out point.X, out point.Y);
			return w.Screen.GetMonitorAtPoint (point.X, point.Y);
		}
		
		public void UpdatePosition (int iconboxWidth, Pane currentPane, Rectangle resultsOffset)
		{
			UpdatePosition (iconboxWidth, currentPane, resultsOffset, new Gdk.Rectangle ());
		}

		protected Rectangle CalculateBasePosition (Rectangle screen, Rectangle window, Rectangle offset)
		{
			Rectangle result = window;

			result.X = ((screen.Width - window.Width) / 2) + screen.X + offset.X;
			result.Y = (int)((screen.Height - window.Height) / 2.5) + screen.Y + offset.Y;

			return result;
		}

		public void UpdatePosition (int iconboxWidth, Pane currentPane, Rectangle resultsOffset, Rectangle normalOffset) {
			Gtk.Application.Invoke (delegate {
				Gdk.Rectangle geo, main, results;
				
				w.GetPosition (out main.X, out main.Y);
				w.GetSize (out main.Width, out main.Height);
			
				geo = w.Screen.GetMonitorGeometry (GetMonitor ());
				main = CalculateBasePosition (geo, main, normalOffset);
				w.Move (main.X, main.Y);
				
				if (r == null) return;
				//position resultsWindow
				r.GetSize (out results.Width, out results.Height);
				results.Y = main.Y + main.Height + resultsOffset.Y;
				results.X = main.X + iconboxWidth * (int) currentPane + resultsOffset.X;
				r.Move (results.X, results.Y);
			});
		}
		
	}
}
