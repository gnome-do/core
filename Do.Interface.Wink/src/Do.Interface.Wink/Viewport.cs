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

using Gdk;
using Wnck;

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
			Rectangle geo;
			window.GetGeometry (out geo.X, out geo.Y, out geo.Width, out geo.Height);
			
			geo.X += window.Workspace.ViewportX;
			geo.Y += window.Workspace.ViewportY;
			
			int x = area.X + (geo.X % area.Width);
			int y = area.Y + (geo.Y % area.Height);
			
			x -= window.Workspace.ViewportX;
			y -= window.Workspace.ViewportY;
			
			WindowMoveResizeMask mask = WindowMoveResizeMask.X | WindowMoveResizeMask.Y;
			window.SetGeometry (WindowGravity.Current, mask, x, y, 0, 0);
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
