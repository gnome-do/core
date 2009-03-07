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
using System.Globalization;

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
		const int LineHeight = 16;
		const double lowlight = .35;
		const string BoldFormatString = "<b>{0}</b>";
		
		ClockDockItem Clock { get; set; }
		
		DateTime CalendarStartDate {
			get {
				return DateTime.Today.AddDays (0 - (int) DateTime.Today.DayOfWeek + 
				                               ((int) DateTimeFormatInfo.CurrentInfo.FirstDayOfWeek));
			}
		}
		
		public CalendarPainter (ClockDockItem clock) : base (clock as AbstractDockItem)
		{
			Clock = clock;
		}
		
		#region IDockPainter implementation 
		
		protected override int Width {
			get {
				return 670;
			}
		}

		
		protected override void PaintArea (Cairo.Context cr, Gdk.Rectangle paintArea)
		{
			int height = paintArea.Height / LineHeight;
			RenderHeader (cr, paintArea);
			for (int i = 1; i < height; i++)
				RenderLine (cr, paintArea, i);
		}
		
		protected override void ReceiveClick (Gdk.Rectangle paintArea, Gdk.Point cursor)
		{
			OnHideRequested ();
		}
		
		#endregion 
		
		void RenderHeader (Context cr, Gdk.Rectangle paintArea)
		{
			int centerLine = paintArea.Y + LineHeight / 2 + ((paintArea.Height % LineHeight) / 2);
			int offsetSize = paintArea.Width / 9;
			
			DateTime day = CalendarStartDate;
			TextRenderContext textContext = new TextRenderContext (cr, string.Empty, offsetSize);
			textContext.Alignment = Pango.Alignment.Center;
			
			cr.Color = new Cairo.Color (1, 1, 1, .5);
			for (int i = 1; i < 8; i++) {
				textContext.Text = string.Format (BoldFormatString, day.ToString ("ddd").ToUpper ());
				textContext.LeftCenteredPoint = new Gdk.Point (paintArea.X + offsetSize * i, centerLine);
				DockServices.DrawingService.TextPathAtPoint (textContext);
				cr.Fill ();
				day = day.AddDays (1);
			}
		}
		
		void RenderLine (Context cr, Gdk.Rectangle paintArea, int line)
		{
			DateTime lineStart = CalendarStartDate.AddDays ((line - 1) * 7);
			int offsetSize = paintArea.Width / 9;
			int centerLine = paintArea.Y + LineHeight / 2 + LineHeight * line + ((paintArea.Height % LineHeight) / 2);
			
			int dayOffset = 0;
			TextRenderContext textContext = new TextRenderContext (cr, string.Empty, offsetSize);
			for (int i = 0; i < 9; i++) {
				if (i == 8) {
					cr.Color = new Cairo.Color (1, 1, 1, lowlight);
					textContext.Text = string.Format (BoldFormatString, lineStart.AddDays (6).ToString ("MMM").ToUpper ());
					textContext.Alignment = Pango.Alignment.Left;
				} else if (i == 0) {
					cr.Color = new Cairo.Color (1, 1, 1, lowlight);
					int woy = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear (lineStart, 
					                                                             DateTimeFormatInfo.CurrentInfo.CalendarWeekRule, 
					                                                             DateTimeFormatInfo.CurrentInfo.FirstDayOfWeek);
					textContext.Text = string.Format ("<b>W{0:00}</b>", woy);
					textContext.Alignment = Pango.Alignment.Right;
				} else {
					DateTime day = lineStart.AddDays (dayOffset);
					textContext.Alignment = Pango.Alignment.Center;
					
					if (day.Month == CalendarStartDate.Month)
						cr.Color = new Cairo.Color (1, 1, 1);
					else
						cr.Color = new Cairo.Color (1, 1, 1, .8);
					
					textContext.Text = string.Format ("{0:00}", day.Day);
					if (day.Date == DateTime.Today)
						textContext.Text = string.Format (BoldFormatString, textContext.Text);
					dayOffset++;
				}
				textContext.LeftCenteredPoint = new Gdk.Point (paintArea.X + offsetSize * i, centerLine);
				DockServices.DrawingService.TextPathAtPoint (textContext);
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
