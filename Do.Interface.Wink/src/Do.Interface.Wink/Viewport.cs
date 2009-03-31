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


using Gdk;
using Wnck;

using Do.Platform;
using Do.Interface.Xlib;

namespace Do.Interface.Wink
{
	
	
	public class Viewport
	{
		Workspace parent;
		Rectangle area;
		
		public string Name { get; private set; }
		
		internal Rectangle Area {
			get { return area; }
		}
		
		internal Viewport(string name, Rectangle area, Workspace parent)
		{
			this.area = area;
			this.parent = parent;
			Name = name;
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
				
				if (string.Compare (window.Screen.WindowManagerName, "compiz", true) == 0) {
					// This is a compiz-ism.  Don't know when they will fix it. You must subtract the top and left
					// frame extents from a move operation to get the window to actually show in the right spot.
					// Save for maybe kwin, I think only compiz uses Viewports anyhow, so this is ok.
					int [] extents = GetWindowFrameExtents (window);
					int left_extent = extents [0];
					int top_extent = extents [2];
					
					x -= left_extent;
					y -= top_extent;
				}
				
				WindowMoveResizeMask mask = WindowMoveResizeMask.X | WindowMoveResizeMask.Y;
				window.SetGeometry (WindowGravity.Current, mask, x, y, 0, 0);
			} else {
				window.MoveToWorkspace (parent);
			}
		}
		
		int[] GetWindowFrameExtents (Wnck.Window window)
		{
			X11Atoms atoms = X11Atoms.Instance;
			
			IntPtr display;
			IntPtr type;
			int format;
			IntPtr prop_return;
			IntPtr nitems, bytes_after;
			int result;
			int [] extents = new int[4];
			
			IntPtr window_handle = (IntPtr) window.Xid;
			
			display = Xlib.Xlib.GdkDisplayXDisplay (Gdk.Screen.Default.Display);
			type = IntPtr.Zero;
			
			result = Xlib.Xlib.XGetWindowProperty (display, window_handle, atoms._NET_FRAME_EXTENTS, (IntPtr) 0,
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
		
		public bool Contains (Gdk.Point point)
		{
			return area.Contains (point);
		}
	}
}
