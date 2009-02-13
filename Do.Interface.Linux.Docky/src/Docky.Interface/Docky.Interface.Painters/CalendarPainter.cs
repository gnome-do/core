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

using Do.Interface;

using Docky.Core;
using Docky.Interface;
using Docky.Utilities;

namespace Docky.Interface.Painters
{
	
	
	public class CalendarPainter : AbstractIntegratedPainter
	{
		const int ArrowSize = 8;
		const int Spacing = 25;
		
		DateTime DisplayDate { get; set; }
		
		int DaysInDate {
			get { return DateTime.DaysInMonth (DisplayDate.Year, DisplayDate.Month); }
		}
		
		int TotalWidth {
			get { return DaysInDate * Spacing; }
		}
		
		#region IDockPainter implementation 
		
		protected override Pixbuf GetIcon (int size)
		{
			return IconProvider.PixbufFromIconName ("calendar", DockPreferences.FullIconSize);
		}
		
		protected override void PaintArea (Cairo.Context cr, Gdk.Rectangle dockArea)
		{
			int centerLine = dockArea.Y + dockArea.Height / 2 + 5;
			int topTextLine = centerLine - 15;
			int bottomTextLine = centerLine + 15;
			
			int startX = dockArea.X + (dockArea.Width - TotalWidth) / 2;
			
			cr.MoveTo (startX, centerLine);
			cr.LineTo (startX + TotalWidth, centerLine);
			cr.Color = new Cairo.Color (1, 1, 1, .4);
			cr.Stroke ();
			
			for (int i = 1; i <= DaysInDate; i++) {
				int y = 0;
				string tmp = string.Format ("<b>{0:00}</b>", i);
				DateTime local_date = new DateTime (DisplayDate.Year, DisplayDate.Month, i);
				Cairo.Color color;
				
				if (local_date.DayOfWeek == DayOfWeek.Saturday || local_date.DayOfWeek == DayOfWeek.Sunday) {
					y = topTextLine;
					color = new Cairo.Color (1, 1, 1, .85);
				} else {
					y = bottomTextLine;
					color = new Cairo.Color (1, 1, 1);
				}
				
				if (local_date == DateTime.Today) {
					tmp = string.Format ("<span underline=\"single\">{0}</span>", tmp);
					color = new Cairo.Color (1, .6, .5);
				}
					
				Gdk.Point drawing_point = new Gdk.Point (startX + (i - 1) * Spacing, y);
				DockServices.DrawingService.TextPathAtPoint (cr, tmp, drawing_point, dockArea.Width,
				                                             Pango.Alignment.Left);
				cr.Color = color;
				cr.Fill ();
				
			}
			
			Gdk.Rectangle leftArea = LeftPointArea (dockArea);
			Gdk.Rectangle rightArea = RightPointArea (dockArea);
			
			cr.MoveTo (leftArea.X, leftArea.Y + leftArea.Height / 2);
			cr.LineTo (leftArea.X + leftArea.Width, leftArea.Y);
			cr.LineTo (leftArea.X + leftArea.Width, leftArea.Y + leftArea.Height);
			cr.ClosePath ();
			
			cr.Color = new Cairo.Color (1, 1, 1);
			cr.Fill ();
			
			cr.MoveTo (rightArea.X + rightArea.Width, rightArea.Y + rightArea.Height / 2);
			cr.LineTo (rightArea.X, rightArea.Y);
			cr.LineTo (rightArea.X, rightArea.Y + rightArea.Height);
			cr.ClosePath ();
			
			cr.Color = new Cairo.Color (1, 1, 1);
			cr.Fill ();
			
			
			Gdk.Point month_point = new Gdk.Point (rightArea.X + rightArea.Width + 10, rightArea.Y + rightArea.Height / 2);
			DockServices.DrawingService.TextPathAtPoint (cr, "<b>" + DisplayDate.ToString ("MMMM ") + DisplayDate.Year + "</b>", month_point, 
			                                               dockArea.Width, Pango.Alignment.Center);
			cr.Color = new Cairo.Color (1, 1, 1);
			cr.Fill ();
		}
		
		Gdk.Rectangle LeftPointArea (Gdk.Rectangle dockArea)
		{
			int height = dockArea.Y + 10;
			int startX = dockArea.X + 110;
			
			return new Gdk.Rectangle (startX, height - ArrowSize / 2, ArrowSize, ArrowSize);
		}
		
		Gdk.Rectangle RightPointArea (Gdk.Rectangle dockArea)
		{
			Gdk.Rectangle rect = LeftPointArea (dockArea);
			rect.X += ArrowSize + 4;
			
			return rect;
		}
		
		protected override void ReceiveClick (Gdk.Rectangle dockArea, Gdk.Point cursor)
		{
			Gdk.Rectangle left = LeftPointArea (dockArea);
			Gdk.Rectangle right = RightPointArea (dockArea);
			Gdk.Rectangle dead = new Gdk.Rectangle (left.X - 10, 
			                                        left.Y - 10, 
			                                        right.X + right.Width + 20 - left.X, 
			                                        left.Height + 20);
			if (left.Contains (cursor)) {
				DisplayDate = DisplayDate.AddMonths (-1);
				RequestDraw ();
			} else if (right.Contains (cursor)) {
				DisplayDate = DisplayDate.AddMonths (1);
				RequestDraw ();
			} else if (dead.Contains (cursor)) {
				// do nothing
			} else {
				OnHideRequested ();
			}
		}
		
		#endregion 
		

		
		public CalendarPainter()
		{
		}
		
		public void Summon ()
		{
			DisplayDate = DateTime.Today;
			OnShowRequested ();
		}
		
		void RequestDraw ()
		{
			OnPaintNeeded (new PaintNeededArgs ());
		} 
		
	}
}
