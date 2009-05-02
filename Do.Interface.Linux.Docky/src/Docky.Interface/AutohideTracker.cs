//  
//  Copyright (C) 2009 GNOME Do
// 
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
// 
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
// 

using System;
using System.Collections.Generic;
using System.Linq;

using Gdk;
using Wnck;

using Do.Interface;
using Do.Interface.Wink;
using Docky.Utilities;

namespace Docky.Interface
{
	
	
	public class AutohideTracker
	{
		DockArea parent;
		bool window_intersecting_other;
		
		Gdk.Rectangle last_known_geo;
		
		public event EventHandler IntersectionChanged;
		
		public bool WindowIntersectingOther {
			get {
				return window_intersecting_other;
			}
			set {
				if (window_intersecting_other == value)
					return;
				
				window_intersecting_other = value;
				if (IntersectionChanged != null)
					IntersectionChanged (this, EventArgs.Empty);
			}
		}
		
		internal AutohideTracker (DockArea parent)
		{
			this.parent = parent;
			Wnck.Screen.Default.ActiveWindowChanged += HandleActiveWindowChanged;
		}

		void HandleGeometryChanged (object sender, EventArgs e)
		{
			Wnck.Window window = sender as Wnck.Window;
			
			Gdk.Rectangle monitor = LayoutUtils.MonitorGemonetry ();
			Gdk.Rectangle geo = window.EasyGeometry ();
			
			geo.X = ((geo.X % monitor.Width) + monitor.Width) % monitor.Width;
			geo.Y = ((geo.Y % monitor.Height) + monitor.Height) % monitor.Height;
			
			if (geo == last_known_geo)
				return;
			
			last_known_geo = geo;
			UpdateWindowIntersect ();
		}

		void HandleActiveWindowChanged (object o, ActiveWindowChangedArgs args)
		{
			if (args.PreviousWindow != null)
				args.PreviousWindow.GeometryChanged -= HandleGeometryChanged;
			
			SetupActiveWindow ();
			
			UpdateWindowIntersect ();
		}
		
		void SetupActiveWindow ()
		{
			Wnck.Window active = Wnck.Screen.Default.ActiveWindow;
			if (active != null) {
				active.GeometryChanged += HandleGeometryChanged; 
				Gdk.Rectangle geo = active.EasyGeometry ();
				Gdk.Rectangle monitor = LayoutUtils.MonitorGemonetry ();
				geo.X = geo.X % monitor.Width;
				geo.Y = geo.Y % monitor.Height;
				last_known_geo = geo;
			}
		}

		public void UpdateWindowIntersect ()
		{
			Gdk.Rectangle adjustedDockArea = parent.MinimumDockArea;
			Gdk.Rectangle geo = LayoutUtils.MonitorGemonetry ();
			
			adjustedDockArea.X = geo.X + (geo.Width - adjustedDockArea.Width) / 2;
			switch (DockPreferences.Orientation) {
			case DockOrientation.Bottom:
				adjustedDockArea.Y = geo.Y + geo.Height - adjustedDockArea.Height;
				break;
			case DockOrientation.Top:
				adjustedDockArea.Y = geo.Y;
				break;
			}
				adjustedDockArea.Inflate (-2, -2);
			
			bool intersect = false;
			try {
				IEnumerable<Wnck.Window> rawWindows = ScreenUtils.ActiveViewport.UnprocessedWindows ();
				Wnck.Window activeWindow = rawWindows
					.Where (w => w.IsActive && w.WindowType != Wnck.WindowType.Desktop)
					.First ();
				
				intersect = rawWindows.Any (w => w.WindowType != Wnck.WindowType.Desktop && 
				                            activeWindow.Pid == w.Pid &&
				                            w.EasyGeometry ().IntersectsWith (adjustedDockArea));
			} catch (Exception e) {
				Do.Platform.Log <AutohideTracker>.Error (e.Message);
			}
			
			WindowIntersectingOther = intersect;
		}
	}
}
