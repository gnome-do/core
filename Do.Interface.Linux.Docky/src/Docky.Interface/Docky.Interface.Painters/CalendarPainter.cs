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
			string cal = "";
			DateTime date = DateTime.Now.Date;
			int daysInMonth = DateTime.DaysInMonth (date.Year, date.Month);
			for (int i = 1; i <= daysInMonth; i++) {
				string tmp = i.ToString ();
				DateTime local_date = new DateTime (date.Year, date.Month, i);
				if (local_date == date)
					tmp = "<span size=\"xx-large\" underline=\"single\">" + tmp + "</span>";
				else
					tmp = "<span size=\"large\">" + tmp + "</span>";
				
				if (local_date.DayOfWeek == DayOfWeek.Saturday || local_date.DayOfWeek == DayOfWeek.Sunday)
					tmp = "<b>" + tmp + "</b>";
				
				cal += tmp + "   ";
			}
			Gdk.Point drawing_point = new Gdk.Point (dockArea.X + 10, dockArea.Y + 2 * dockArea.Height / 3);
			DockServices.DrawingService.RenderTextAtPoint (cr, cal, drawing_point, 
			                                               dockArea.Width, Pango.Alignment.Center);
			
			Gdk.Point month_point = new Gdk.Point (drawing_point.X, drawing_point.Y - 25);
			DockServices.DrawingService.RenderTextAtPoint (cr, "<span size=\"large\"><b>" + date.ToLongDateString () + "</b></span>", 
			                                               month_point, dockArea.Width, Pango.Alignment.Center);
			
		}
		
		public void Clicked (Gdk.Rectangle dockArea, Gdk.Point cursor)
		{
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
