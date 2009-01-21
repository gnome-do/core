// LayoutUtils.cs
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
using System.Linq;

using Gdk;

using Docky.Utilities;

namespace Docky.Interface
{
	public enum RelativeMove
	{
		Inward = 0,
		Outward,
		RelativeLeft,
		RelativeRight,
		RelativeUp,
		RelativeDown,
		RealLeft,
		RealRight,
		RealUp,
		RealDown,
	}
	
	public static class LayoutUtils
	{
		static Gdk.Rectangle monitor_geo = Gdk.Screen.Default.GetMonitorGeometry (DockPreferences.Monitor);

		static LayoutUtils ()
		{
			Gdk.Screen.Default.SizeChanged += HandleSizeChanged;
			DockPreferences.MonitorChanged += Recalculate;
		}

		static void HandleSizeChanged (object sender, EventArgs args)
		{
			Recalculate ();
		}

		static void Recalculate ()
		{
			monitor_geo = Gdk.Screen.Default.GetMonitorGeometry (DockPreferences.Monitor);
		}
		
		public static Gdk.Rectangle MonitorGemonetry ()
		{
			return monitor_geo;
		}

		public static Gdk.Point RelativePointToRootPoint (this Gdk.Point relativePoint, Gtk.Window window)
		{
			Gdk.Rectangle main;
			window.GetPosition (out main.X, out main.Y);
			return new Gdk.Point (main.X + relativePoint.X, main.Y + relativePoint.Y);
		}

		public static Gdk.Point RelativeMovePoint (this Gdk.Point startingLocation, int delta, RelativeMove direction)
		{
			int[] vector = null;
			switch (direction) {
			case RelativeMove.RealDown:
				return new Gdk.Point (startingLocation.X, startingLocation.Y + delta);
				
			case RelativeMove.RealLeft:
				return new Gdk.Point (startingLocation.X - delta, startingLocation.Y);
				
			case RelativeMove.RealRight:
				return new Gdk.Point (startingLocation.X + delta, startingLocation.Y);
				
			case RelativeMove.RealUp:
				return new Gdk.Point (startingLocation.X, startingLocation.Y - delta);

			case RelativeMove.Inward:
			case RelativeMove.RelativeUp:
				vector = new [] {0, 0 - delta};
				break;
				
			case RelativeMove.Outward:
			case RelativeMove.RelativeDown:
				vector = new [] {0, delta};
				break;
				
			case RelativeMove.RelativeLeft:
				vector = new [] {0 - delta, 0};
				break;
				
			case RelativeMove.RelativeRight:
				vector = new [] {delta, 0};
				break;
			}

			switch (DockPreferences.Orientation) {
			case DockOrientation.Bottom:
				// do nothing
				break;
			case DockOrientation.Left:
				vector = new [] {0 - vector [1], vector [0]};
				break;
			case DockOrientation.Right:
				vector = new [] {vector [1], 0 - vector [0]};
				break;
			case DockOrientation.Top:
				vector = new [] {vector [0], 0 - vector [1]};
				break;
			}

			return new Gdk.Point (startingLocation.X + vector [0], startingLocation.Y + vector [1]);
		}
	}
}
