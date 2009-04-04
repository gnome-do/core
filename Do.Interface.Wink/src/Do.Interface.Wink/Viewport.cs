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
using System.Runtime.InteropServices;
using System.Linq;


using Gdk;
using Wnck;

using Do.Platform;
using Do.Interface.Xlib;

namespace Do.Interface.Wink
{
	public class Viewport
	{
		enum Position {
			Left = 0,
			Right,
			Top,
			Bottom,
		}
		
		private class WindowState {
			public Gdk.Rectangle Area;
			public Wnck.WindowState State;
			
			public WindowState (Gdk.Rectangle area, Wnck.WindowState state)
			{
				Area = area;
				State = state;
			}
		}
		
		Workspace parent;
		Rectangle area;
		Dictionary<Wnck.Window, WindowState> window_states;
		
		public string Name { get; private set; }
		
		internal Rectangle Area {
			get { return area; }
		}
		
		internal Viewport(string name, Rectangle area, Workspace parent)
		{
			this.area = area;
			this.parent = parent;
			Name = name;
			window_states = new Dictionary<Wnck.Window, WindowState> ();
		}
		
		public bool Contains (Gdk.Point point)
		{
			return area.Contains (point);
		}
		
		private IEnumerable<Wnck.Window> RawWindows ()
		{
			foreach (Wnck.Window window in WindowUtils.GetWindows ()) {
				if (WindowCenterInViewport (window) || window.IsSticky)
					yield return window;
			}
		}
		
		public IEnumerable<Wnck.Window> Windows ()
		{
			return RawWindows ().Where (w => !w.IsSkipTasklist && w.WindowType != Wnck.WindowType.Dock);
		}
		
		public void MoveWindowInto (Wnck.Window window)
		{
			if (parent.IsVirtual) {
				Rectangle geo;
				window.GetGeometry (out geo.X, out geo.Y, out geo.Width, out geo.Height);
				
				geo.X += window.Workspace.ViewportX;
				geo.Y += window.Workspace.ViewportY;
				
				int x = area.X + (geo.X % area.Width);
				int y = area.Y + (geo.Y % area.Height);
				
				x -= window.Workspace.ViewportX;
				y -= window.Workspace.ViewportY;
				
				WindowMoveResizeMask mask = WindowMoveResizeMask.X | WindowMoveResizeMask.Y;
				SetWorkaroundGeometry (window, WindowGravity.Current, mask, x, y, 0, 0);
			} else {
				window.MoveToWorkspace (parent);
			}
		}
		
		public bool WindowVisibleInVeiwport (Wnck.Window window)
		{
			Rectangle geo;
			window.GetGeometry (out geo.X, out geo.Y, out geo.Width, out geo.Height);
			geo.X += parent.ViewportX;
			geo.Y += parent.ViewportY;
			
			return area.IntersectsWith (geo);
		}
		
		public bool WindowCenterInViewport (Wnck.Window window)
		{
			Rectangle geo;
			window.GetGeometry (out geo.X, out geo.Y, out geo.Width, out geo.Height);
			geo.X += parent.ViewportX;
			geo.Y += parent.ViewportY;
			
			Point center = new Point (geo.X + geo.Width / 2, geo.Y + geo.Height / 2);
			return Contains (center);
		}
		
		public void RestoreLayout ()
		{
			foreach (Wnck.Window window in Windows ())
				RestoreTemporaryWindowGeometry (window);
			
			window_states.Clear ();
		}
		
		public void Cascade ()
		{
			IEnumerable<Wnck.Window> windows = Windows ().Where (w => !w.IsMinimized);
			if (windows.Count () <= 1) return;
			
			Gdk.Rectangle screenGeo = GetScreenGeoMinusStruts ();
			
			int titleBarSize = GetWindowFrameExtents (windows.First ()) [(int) Position.Top];
			int windowHeight = screenGeo.Height - ((windows.Count () - 1) * titleBarSize);
			int windowWidth = screenGeo.Width - ((windows.Count () - 1) * titleBarSize);
			
			int count = 0;
			int x, y;
			foreach (Wnck.Window window in windows) {
				x = screenGeo.X + titleBarSize * count - parent.ViewportX;
				y = screenGeo.Y + titleBarSize * count - parent.ViewportY;
				
				SetTemporaryWindowGeometry (window, new Gdk.Rectangle (x, y, windowWidth, windowHeight));
				count++;
			}
		}
		
		public void ShowDesktop ()
		{
			if (!ScreenUtils.DesktopShown (parent.Screen))
				ScreenUtils.ShowDesktop (parent.Screen);
			else
				ScreenUtils.UnshowDesktop (parent.Screen);
		}
		
		public void Tile ()
		{
			IEnumerable<Wnck.Window> windows = Windows ().Where (w => !w.IsMinimized);
			if (windows.Count () <= 1) return;
			
			Gdk.Rectangle screenGeo = GetScreenGeoMinusStruts ();
			
			int width, height;
			//We are going to tile to a square, so what we want is to find
			//the smallest perfect square all our windows will fit into
			width = (int) Math.Ceiling (Math.Sqrt (windows.Count ()));
			
			//Our height is at least one (e.g. a 2x1)
			height = 1;
			while (width * height < windows.Count ())
				height++;
			
			int windowWidth, windowHeight;
			windowWidth = screenGeo.Width / width;
			windowHeight = screenGeo.Height / height;
			
			int row = 0, column = 0;
			int x, y;
			
			foreach (Wnck.Window window in windows) {
				x = screenGeo.X + (column * windowWidth) - parent.ViewportX;
				y = screenGeo.Y + (row * windowHeight) - parent.ViewportY;
				
				Gdk.Rectangle windowArea = new Gdk.Rectangle (x, y, windowWidth, windowHeight);;
				
				if (window == windows.Last ())
					windowArea.Width *= width - column;
				
				SetTemporaryWindowGeometry (window, windowArea);
				
				column++;
				if (column == width) {
					column = 0;
					row++;
				}
			}
		}
		
		Gdk.Rectangle GetScreenGeoMinusStruts ()
		{
			IEnumerable<int []> struts = RawWindows ()
				.Where (w => w.WindowType == Wnck.WindowType.Dock)
				.Select (w => GetCardinalWindowProperty (w, X11Atoms.Instance._NET_WM_STRUT_PARTIAL));
			
			int [] offsets = new int [4];
			for (int i = 0; i < 4; i++)
				offsets [i] = struts.Max (a => a[i]);
			
			Gdk.Rectangle screenGeo = Area;
			screenGeo.Width -= offsets [(int) Position.Left] + offsets [(int) Position.Right];
			screenGeo.Height -= offsets [(int) Position.Top] + offsets [(int) Position.Bottom];
			screenGeo.X += offsets [(int) Position.Left];
			screenGeo.Y += offsets [(int) Position.Top];
			
			return screenGeo;
		}
		
		int [] GetWindowFrameExtents (Wnck.Window window)
		{
			return GetCardinalWindowProperty (window, X11Atoms.Instance._NET_FRAME_EXTENTS);
		}
		
		int [] GetCardinalWindowProperty (Wnck.Window window, IntPtr atom)
		{
			X11Atoms atoms = X11Atoms.Instance;
			
			IntPtr display;
			IntPtr type;
			int format;
			IntPtr prop_return;
			IntPtr nitems, bytes_after;
			int result;
			int [] extents = new int[12];
			
			IntPtr window_handle = (IntPtr) window.Xid;
			
			display = Xlib.Xlib.GdkDisplayXDisplay (Gdk.Screen.Default.Display);
			type = IntPtr.Zero;
			
			result = Xlib.Xlib.XGetWindowProperty (display, window_handle, atom, (IntPtr) 0,
			                                  (IntPtr) System.Int32.MaxValue, false, atoms.XA_CARDINAL, out type, out format,
			                                  out nitems, out bytes_after, out prop_return);
			
			if (type == atoms.XA_CARDINAL && format == 32) {
				extents = new int [(int) nitems];
				for (int i = 0; i < (int) nitems; i++) {
					extents [i] = Marshal.ReadInt32 (prop_return, i * 4);
				}
			}
			
			return extents;
		}
		
		void SetWorkaroundGeometry (Wnck.Window window, WindowGravity gravity, WindowMoveResizeMask mask, 
		                                     int x, int y, int width, int height)
		{
			if (string.Compare (window.Screen.WindowManagerName, "compiz", true) == 0) {
				// This is a compiz-ism.  Don't know when they will fix it. You must subtract the top and left
				// frame extents from a move operation to get the window to actually show in the right spot.
				// Save for maybe kwin, I think only compiz uses Viewports anyhow, so this is ok.
				int [] extents = GetWindowFrameExtents (window);
				
				x -= extents [(int) Position.Left];
				y -= extents [(int) Position.Top];
			}
			
			window.SetGeometry (gravity, mask, x, y, width, height);
		}
		
		void SetTemporaryWindowGeometry (Wnck.Window window, Gdk.Rectangle area)
		{
			Gdk.Rectangle oldGeo;
			window.GetGeometry (out oldGeo.X, out oldGeo.Y, out oldGeo.Width, out oldGeo.Height);
			
			if (!window_states.ContainsKey (window)) 
				window_states [window] = new WindowState (oldGeo, window.State);
			
			if (window.IsMaximized)
				window.Unmaximize ();
			
			WindowMoveResizeMask mask = WindowMoveResizeMask.Width | 
				                        WindowMoveResizeMask.Height | 
				                        WindowMoveResizeMask.X | 
				                        WindowMoveResizeMask.Y;
			
			SetWorkaroundGeometry (window, WindowGravity.Current, mask, area.X, area.Y, area.Width, area.Height);
		}
		
		void RestoreTemporaryWindowGeometry (Wnck.Window window)
		{
			if (!window_states.ContainsKey (window))
				return;
			
			WindowMoveResizeMask mask = WindowMoveResizeMask.Width | 
				                        WindowMoveResizeMask.Height | 
				                        WindowMoveResizeMask.X | 
				                        WindowMoveResizeMask.Y;
				
			WindowState state = window_states [window];
			SetWorkaroundGeometry (window, WindowGravity.Current, mask, state.Area.X, 
			                       state.Area.Y, state.Area.Width, state.Area.Height);
		}
	}
}
