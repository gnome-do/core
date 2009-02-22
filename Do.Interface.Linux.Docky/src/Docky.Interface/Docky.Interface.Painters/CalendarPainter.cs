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

using Cairo;
using Gdk;

using Do.Interface;

using Docky.Core;
using Docky.Interface;
using Docky.Utilities;

namespace Docky.Interface.Painters
{
	
	
	public class CalendarPainter : AbstractIntegratedPainter
	{
		const int LineHeight = 21;
		
		ClockDockItem Clock { get; set; }
		
		public CalendarPainter (ClockDockItem clock) : base (clock as AbstractDockItem)
		{
			Clock = clock;
		}
		
		#region IDockPainter implementation 
		
		protected override int Width {
			get {
				return 700;
			}
		}

		
		protected override void PaintArea (Cairo.Context cr, Gdk.Rectangle paintArea)
		{
			DateTime startDay = DateTime.Today;
			startDay = startDay.AddDays (0 - (int) startDay.DayOfWeek);
			
			int height = paintArea.Height / LineHeight;
			for (int i = 0; i < height; i++)
				RenderLine (cr, paintArea, startDay, i);
			
			DrawVSeparator (cr, 
			                new Gdk.Point (paintArea.X + paintArea.Width / 9, paintArea.Y + 4),
			                new Gdk.Point (paintArea.X + paintArea.Width / 9, paintArea.Y + paintArea.Height - 4));
		}
		
		protected override void ReceiveClick (Gdk.Rectangle paintArea, Gdk.Point cursor)
		{
			OnHideRequested ();
		}
		
		#endregion 
		
		void DrawVSeparator (Context cr, Gdk.Point startPoint, Gdk.Point endPoint)
		{
			cr.Translate (-.5, 0);
			
			cr.MoveTo (startPoint.X, startPoint.Y);
			cr.LineTo (endPoint.X, endPoint.Y);
			cr.LineWidth = 1;
			
			cr.Color = new Cairo.Color (1, 1, 1, .1);
			cr.Stroke ();
			
			cr.MoveTo (startPoint.X + 1, startPoint.Y);
			cr.LineTo (endPoint.X + 1, endPoint.Y);
			
			cr.Color = new Cairo.Color (1, 1, 1, .3);
			cr.Stroke ();
			
			cr.Translate (.5, 0);
		}
		
		void RenderLine (Context cr, Gdk.Rectangle paintArea, DateTime startDate, int line)
		{
			DateTime lineStart = startDate.AddDays (line * 7);
			int offsetSize = paintArea.Width / 9;
			int centerLine = paintArea.Y + LineHeight / 2 + LineHeight * line + ((paintArea.Height % LineHeight) / 2);
			
			Pango.Alignment align;
			string text;
			int dayOffset = 0;
			for (int i = 0; i < 9; i++) {
				if (i == 8) {
					cr.Color = new Cairo.Color (1, 1, 1, .5);
					text = string.Format ("<b>  {0}</b>", lineStart.AddDays (6).ToString ("MMM"));
					align = Pango.Alignment.Left;
				} else if (i == 0) {
					cr.Color = new Cairo.Color (1, 1, 1, .5);
					text = string.Format ("<b>{0}  </b>", lineStart.DayOfYear / 7 + 1);
					align = Pango.Alignment.Right;
				} else {
					DateTime day = lineStart.AddDays (dayOffset);
					align = Pango.Alignment.Center;
					
					if (line == 0) {
						cr.Translate (-15, 0);
						text = string.Format ("<b>{0}</b>", day.DayOfWeek.ToString () [0]);
						cr.Color = new Cairo.Color (1, 1, 1, .5);
						DockServices.DrawingService.TextPathAtPoint (cr,
						                                             text,
						                                             new Gdk.Point (paintArea.X + offsetSize * i, centerLine),
						                                             offsetSize,
						                                             align);
						cr.Translate (15, 0);
						cr.Fill ();
					}
					
					cr.Color = new Cairo.Color (1, 1, 1);
					text = string.Format ("<b>{0:00}</b>", day.Day);
					dayOffset++;
				}
				DockServices.DrawingService.TextPathAtPoint (cr,
				                                             text,
				                                             new Gdk.Point (paintArea.X + offsetSize * i, centerLine),
				                                             offsetSize,
				                                             align);
				cr.Fill ();
				
			}
		}
		
		public void Summon ()
		{
			OnShowRequested ();
		}
		
		void RequestDraw ()
		{
			OnPaintNeeded (new PaintNeededArgs ());
		} 
		
	}
}
