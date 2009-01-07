// ItemPositionProvider.cs
// 
// Copyright (C) 2009 GNOME Do
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
	
	
	public class ItemPositionProvider
	{
		const int HorizontalBuffer = 7;
		
		DockItemProvider item_provider;
		Rectangle clip_area;
		
		List<IDockItem> DockItems {
			get { return item_provider.DockItems; }
		}
		
		/// <value>
		/// The width of the visible dock
		/// </value>
		public int DockWidth {
			get {
				int val = 2 * HorizontalBuffer;
				foreach (IDockItem di in DockItems)
					val += 2 * DockPreferences.IconBorderWidth + di.Width;
				return val;
			}
		}
		
		public int VerticalBuffer {
			get { return DockPreferences.Reflections ? 10 : 5; }
		}
		
		int IconSize {
			get { return DockPreferences.IconSize; }
		}
		
		int Width {
			get { return clip_area.Width; }
		}
		
		int Height {
			get { return clip_area.Height; }
		}
		
		int ZoomSize {
			get { return DockPreferences.ZoomSize; }
		}
		
		public Rectangle MinimumDockArea { 
			get {
				int x_offset = (Width - DockWidth) / 2;
				return new Gdk.Rectangle (x_offset, Height - IconSize - 2 * VerticalBuffer, DockWidth, IconSize + 2 * VerticalBuffer);
			}
		}
		
		public ItemPositionProvider(DockItemProvider itemProvider, Gdk.Rectangle clipArea)
		{
			item_provider = itemProvider;
			clip_area = clipArea;
		}
		
		public Rectangle DockArea (double zoomByEntryTime, Gdk.Point cursor)
		{
			int start_x, end_x;
			double start_zoom, end_zoom;
			IconZoomedPosition (0, zoomByEntryTime, cursor, out start_x, out start_zoom);
			IconZoomedPosition (DockItems.Count - 1, zoomByEntryTime, cursor, out end_x, out end_zoom);
			
			double x = start_x - start_zoom * (IconSize / 2) - (start_zoom * HorizontalBuffer) - DockPreferences.IconBorderWidth;
			double end = end_x + end_zoom * (IconSize / 2) + (end_zoom * HorizontalBuffer) + DockPreferences.IconBorderWidth;
			
			return new Gdk.Rectangle ((int) x, Height - IconSize - 2 * VerticalBuffer, (int) (end - x), IconSize + 2 * VerticalBuffer);
		}
		
		public int IconUnzoomedPosition (int icon)
		{
			// the first icons center is at dock X + border + IconBorder + half its width
			// it is subtle, but it *is* a mistake to add the half width until the end.  adding
			// premature will add the wrong width.  It hurts the brain.
			if (!DockItems.Any ())
				return 0;
			int startX = MinimumDockArea.X + HorizontalBuffer + DockPreferences.IconBorderWidth;
			for (int i = 0; i < icon; i++)
				startX += DockItems [i].Width + 2 * DockPreferences.IconBorderWidth;
			
			return startX + DockItems [icon].Width / 2;
		}
		
		public void IconZoomedPosition (int icon, double zoomByEntryTime, Gdk.Point cursor, out int position, out double zoom)
		{
			// get our actual center
			int center = IconUnzoomedPosition (icon);
			
			// ZoomPercent is a number greater than 1.  It should never be less than one.  ZoomIn is a range of 0 to 1.
			// we need a number that is 1 when ZoomIn is 0, and ZoomPercent when ZoomIn is 1.  Then we treat this as 
			// if it were the ZoomPercent for the rest of the calculation
			double zoomInPercent = 1 + (DockPreferences.ZoomPercent - 1) * zoomByEntryTime;
			
			// offset from the center of the true position, ranged between 0 and half of the zoom range
			int offset = Math.Min (Math.Abs (cursor.X - center), ZoomSize / 2);
			
			if (ZoomSize / 2.0 == 0) {
				zoom = 1;
			} else {
				// zoom is calculated as 1 through target_zoom (default 2).  The larger your offset, the smaller your zoom
				zoom = 0 - Math.Pow (offset / (ZoomSize / 2.0), 2) + 2;
				zoom = 1 + (zoom - 1) * (zoomInPercent - 1);
				
				offset = (int) (offset * (zoomInPercent - 1) - (zoomInPercent - zoom) * (IconSize * .9));
			}
			
			if (cursor.X > center) {
				center -= offset;
			} else {
				center += offset;
			}
			position = center;
		}
		
		public int IndexAtPosition (int position)
		{
			int startX = MinimumDockArea.X + HorizontalBuffer;
			int width;
			for (int i = 0; i < DockItems.Count; i++) {
				width = DockItems [i].Width + 2 * DockPreferences.IconBorderWidth;
				if (position >= startX && position <= startX + width)
					return i;
				startX += width;
			}
			return -1;
		}
	}
}
