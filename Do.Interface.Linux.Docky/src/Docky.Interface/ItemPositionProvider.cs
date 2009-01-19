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
	
	
	public class ItemPositionProvider : IDisposable
	{
		const int HorizontalBuffer = 7;
		
		DockItemProvider item_provider;
		Rectangle clip_area;
		
		List<BaseDockItem> DockItems {
			get { return item_provider.DockItems; }
		}
		
		/// <value>
		/// The width of the visible dock
		/// </value>
		public int DockWidth {
			get {
				int val = 2 * HorizontalBuffer;
				foreach (BaseDockItem di in DockItems)
					val += 2 * DockPreferences.IconBorderWidth + di.Width;
				return val;
			}
		}

		public int DockHeight {
			get {
				return IconSize + 2 * VerticalBuffer;
			}
		}
		
		public int VerticalBuffer {
			get { return 5; }
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
				int widthOffset;

				if (DockPreferences.DockIsHorizontal)
					widthOffset = (Width - DockWidth) / 2;
				else
					widthOffset = (Height - DockWidth) / 2;
				
				Gdk.Rectangle rect;
				switch (DockPreferences.Orientation) {
				case DockOrientation.Bottom:
					rect = new Gdk.Rectangle (widthOffset, Height - DockHeight, DockWidth, DockHeight);
					break;
					
				case DockOrientation.Left:
					rect = new Gdk.Rectangle (0, widthOffset, DockHeight, DockWidth);
					break;

				case DockOrientation.Right:
					rect = new Gdk.Rectangle (Width - DockHeight, widthOffset, DockHeight, DockWidth);
					break;

				case DockOrientation.Top:
					rect = new Gdk.Rectangle (widthOffset, 0, DockWidth, DockHeight);
					break;
				default:
					rect = new Gdk.Rectangle (0, 0, 0, 0);
					break;
				}
				rect.X += clip_area.X;
				rect.Y += clip_area.Y;

				return rect;
			}
		}
		
		public ItemPositionProvider(DockItemProvider itemProvider, Gdk.Rectangle clipArea)
		{
			item_provider = itemProvider;
			clip_area = clipArea;
		}
		
		public Rectangle DockArea (double zoomByEntryTime, Gdk.Point cursor)
		{
			Gdk.Point startPosition, endPosition;
			double start_zoom, end_zoom;
			IconZoomedPosition (0, zoomByEntryTime, cursor, out startPosition, out start_zoom);
			IconZoomedPosition (DockItems.Count - 1, zoomByEntryTime, cursor, out endPosition, out end_zoom);
			
			int leftEdge, rightEdge, topEdge, bottomEdge;
			int startEdgeConstant = (int) (start_zoom * (IconSize / 2) + (start_zoom * HorizontalBuffer) + DockPreferences.IconBorderWidth);
			int endEdgeConstant = (int) (end_zoom * (IconSize / 2) + (end_zoom * HorizontalBuffer) + DockPreferences.IconBorderWidth);
			
			switch (DockPreferences.Orientation) {
			case DockOrientation.Bottom:
				leftEdge = startPosition.X - startEdgeConstant;
				rightEdge = endPosition.X + endEdgeConstant;
				bottomEdge = Height;
				topEdge = Height - DockHeight;
				break;
			case DockOrientation.Left:
				topEdge = startPosition.Y - startEdgeConstant;
				bottomEdge = endPosition.Y + endEdgeConstant;
				leftEdge = 0;
				rightEdge = DockHeight;
				break;
			case DockOrientation.Right:
				topEdge = startPosition.Y - startEdgeConstant;
				bottomEdge = endPosition.Y + endEdgeConstant;
				leftEdge = Width - DockHeight;
				rightEdge = Width;
				break;
			case DockOrientation.Top:
				leftEdge = startPosition.X - startEdgeConstant;
				rightEdge = endPosition.X + endEdgeConstant;
				bottomEdge = DockHeight;
				topEdge = 0;
				break;
			default:
				leftEdge = rightEdge = topEdge = bottomEdge = 0;
				break;
			}
			
			Gdk.Rectangle rect = new Gdk.Rectangle (leftEdge, topEdge, Math.Abs (leftEdge - rightEdge), Math.Abs (topEdge - bottomEdge));
			rect.X += clip_area.X;
			rect.Y += clip_area.Y;

			return rect;
		}
		
		public Gdk.Point IconUnzoomedPosition (int icon)
		{
			// the first icons center is at dock X + border + IconBorder + half its width
			// it is subtle, but it *is* a mistake to add the half width until the end.  adding
			// premature will add the wrong width.  It hurts the brain.
			if (!DockItems.Any ())
				return new Gdk.Point (0, 0);

			int startOffset = HorizontalBuffer + DockPreferences.IconBorderWidth;
			
			for (int i = 0; i < icon; i++)
				startOffset += DockItems [i].Width + 2 * DockPreferences.IconBorderWidth;
			
			startOffset += DockItems [icon].Width / 2;

			switch (DockPreferences.Orientation) {
			case DockOrientation.Bottom:
				startOffset += MinimumDockArea.X;
				return new Gdk.Point (startOffset, Height - DockHeight / 2);
				
			case DockOrientation.Top:
				startOffset += MinimumDockArea.X;
				return new Gdk.Point (startOffset, DockHeight / 2);
			
			case DockOrientation.Left:
				startOffset += MinimumDockArea.Y;
				return new Gdk.Point (DockHeight / 2, startOffset);
				
			case DockOrientation.Right:
				startOffset += MinimumDockArea.Y;
				return new Gdk.Point (Width - DockHeight / 2, startOffset);
			default:
				return new Gdk.Point (0, 0);
			}
		}
		
		public void IconZoomedPosition (int icon, double zoomByEntryTime, Gdk.Point cursor, out Gdk.Point position, out double zoom)
		{
			// get our actual center
			Gdk.Point center = IconUnzoomedPosition (icon);

			int cursorOrientedPosition, centerOrientedPosition;
			if (DockPreferences.Orientation == DockOrientation.Bottom ||
			    DockPreferences.Orientation == DockOrientation.Top) {
				cursorOrientedPosition = cursor.X;
				centerOrientedPosition = center.X;
			} else {
				cursorOrientedPosition = cursor.Y;
				centerOrientedPosition = center.Y;
			}
			
			// ZoomPercent is a number greater than 1.  It should never be less than one.
			// ZoomIn is a range of 0 to 1. we need a number that is 1 when ZoomIn is 0, 
			// and ZoomPercent when ZoomIn is 1.  Then we treat this as 
			// if it were the ZoomPercent for the rest of the calculation
			double zoomInPercent = 1 + (DockPreferences.ZoomPercent - 1) * zoomByEntryTime;
			
			// offset from the center of the true position, ranged between 0 and half of the zoom range
			double offset = Math.Min (Math.Abs (cursorOrientedPosition - centerOrientedPosition), ZoomSize / 2);
			
			if (ZoomSize / 2.0 == 0) {
				zoom = 1;
			} else {
				// zoom is calculated as 1 through target_zoom (default 2).  
				// The larger your offset, the smaller your zoom
				zoom = 0 - Math.Pow (offset / (ZoomSize / 2.0), 2) + 2;
				zoom = 1 + (zoom - 1) * (zoomInPercent - 1);
				
				offset = offset * (zoomInPercent - 1) - (zoomInPercent - zoom) * (IconSize * .9);
			}
			
			if (cursorOrientedPosition > centerOrientedPosition) {
				centerOrientedPosition -= (int) offset;
			} else {
				centerOrientedPosition += (int) Math.Ceiling (offset);
			}

			if (!DockItems [icon].Scalable) {
				zoom = 1;
				switch (DockPreferences.Orientation) {
				case DockOrientation.Bottom:
					position = new Gdk.Point (centerOrientedPosition, center.Y);
					break;
				case DockOrientation.Left:
					position = new Gdk.Point (center.X, centerOrientedPosition);
					break;
				case DockOrientation.Right:
					position = new Gdk.Point (center.X, centerOrientedPosition);
					break;
				case DockOrientation.Top:
					position = new Gdk.Point (centerOrientedPosition, center.Y);
					break;
				default:
					position = new Gdk.Point (0,0);
					break;
				}
				return;
			}
			
			int zoomedCenterHeight = VerticalBuffer + (int) (Math.Ceiling (DockItems [icon].Height * zoom / 2));
			switch (DockPreferences.Orientation) {
			case DockOrientation.Bottom:
				position = new Gdk.Point (centerOrientedPosition, Height - zoomedCenterHeight);
				break;
				
			case DockOrientation.Left:
				position = new Gdk.Point (zoomedCenterHeight, centerOrientedPosition);
				break;
				
			case DockOrientation.Right:
				position = new Gdk.Point (Width - zoomedCenterHeight, centerOrientedPosition);
				break;

			case DockOrientation.Top:
				position = new Gdk.Point (centerOrientedPosition, zoomedCenterHeight);
				break;
			default:
				position = new Gdk.Point (0, 0);
				break;
			}
		}

		public int IndexAtPosition (int x, int y)
		{
			return IndexAtPosition (new Gdk.Point (x, y));
		}
		
		public int IndexAtPosition (Gdk.Point location)
		{
			int position = DockPreferences.DockIsHorizontal ? location.X : location.Y;
			
			int startOffset = DockPreferences.DockIsHorizontal ? MinimumDockArea.X + HorizontalBuffer : MinimumDockArea.Y + HorizontalBuffer;
			int width;
			for (int i = 0; i < DockItems.Count; i++) {
				width = DockItems [i].Width + 2 * DockPreferences.IconBorderWidth;
				if (position >= startOffset && position <= startOffset + width)
					return i;
				startOffset += width;
			}
			return -1;
		}

		#region IDisposable implementation 
		
		public void Dispose ()
		{
			item_provider = null;
		}
		
		#endregion 
		
	}
}
