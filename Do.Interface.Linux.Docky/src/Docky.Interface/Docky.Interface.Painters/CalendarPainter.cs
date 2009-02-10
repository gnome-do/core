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

using Docky.Core;
using Docky.Interface;

namespace Docky.Interface.Painters
{
	
	
	public class CalendarPainter : IDockPainter
	{
		#region IDockPainter implementation 
		
		public event EventHandler<PaintNeededArgs> PaintNeeded;
		
		public event EventHandler ShowRequested;
		
		public event EventHandler HideRequested;
		
		public void Paint (Cairo.Context cr, Gdk.Rectangle dockArea, Gdk.Point cursor)
		{
			int centerLine = dockArea.Y + dockArea.Height / 2 + 5;
			int topTextLine = centerLine - 15;
			int bottomTextLine = centerLine + 15;
			
			cr.MoveTo (dockArea.X + 15, centerLine);
			cr.LineTo (dockArea.X + dockArea.Width - 15, centerLine);
			cr.Color = new Cairo.Color (1, 1, 1);
			cr.Stroke ();
			
			DateTime date = DateTime.Now.Date;
			int daysInMonth = DateTime.DaysInMonth (date.Year, date.Month);
			
			for (int i = 1; i <= daysInMonth; i++) {
				int y = 0;
				string tmp = "<b>" + i.ToString ().PadLeft (2, '0') + "</b>";
				DateTime local_date = new DateTime (date.Year, date.Month, i);
				if (local_date == date)
					tmp = "<span underline=\"single\">" + tmp + "</span>";
				
				if (local_date.DayOfWeek == DayOfWeek.Saturday || local_date.DayOfWeek == DayOfWeek.Sunday)
					y = topTextLine;
				else
					y = bottomTextLine;
			
				Gdk.Point drawing_point = new Gdk.Point (dockArea.X + i * 25, y);
				DockServices.DrawingService.RenderTextAtPoint (cr, tmp, drawing_point, dockArea.Width,
				                                               Pango.Alignment.Left);
			}
			Gdk.Point month_point = new Gdk.Point (dockArea.X + 15, dockArea.Y + 10);
			
			DockServices.DrawingService.RenderTextAtPoint (cr, "<b>" + date.ToString ("MMMM ") + date.Year + "</b>", month_point, 
			                                               dockArea.Width, Pango.Alignment.Center);
		}
		
		public void Clicked (Gdk.Rectangle dockArea, Gdk.Point cursor)
		{
			if (HideRequested != null)
				HideRequested (this, EventArgs.Empty);
		}
		
		public void Interupt ()
		{
		}
		
		public bool DoubleBuffer {
			get {
				return false;
			}
		}
		
		public bool Interuptable {
			get {
				return true;
			}
		}
		
		#endregion 
		

		
		public CalendarPainter()
		{
		}
		
		public void Summon ()
		{
			if (ShowRequested != null)
				ShowRequested (this, EventArgs.Empty);
		}

		#region IDisposable implementation 
		
		public void Dispose ()
		{
			throw new System.NotImplementedException();
		}
		
		#endregion 
		
	}
}
