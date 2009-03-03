// WindowMenuButtonArgs.cs
// 
// Copyright (C) 2008 GNOME Do
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

using Cairo;
using Gdk;
using Gtk;

using Wnck;

using Docky.Core;
using Docky.Utilities;

using Do.Interface.CairoUtils;

namespace Docky.Interface.Menus
{
	
	
	public class WindowMenuButtonArgs : AbstractMenuArgs
	{
		const string FormatString = "<b>{0}</b>";
		const int WidthBuffer = 4;
		
		Wnck.Window window;
		DrawingArea button;
		bool hovered;
		
		public override Widget Widget {
			get {
				return button;
			}
		}
		
		public WindowMenuButtonArgs (Wnck.Window window) : base ()
		{
			this.window = window;
			button = new DrawingArea ();
			
			button.ExposeEvent += HandleExposeEvent;
			button.EnterNotifyEvent += HandleEnterNotifyEvent;
			button.LeaveNotifyEvent += HandleLeaveNotifyEvent; 
			button.ButtonReleaseEvent += HandleButtonReleaseEvent; 
			
			button.AddEvents ((int) (EventMask.EnterNotifyMask | EventMask.LeaveNotifyMask | EventMask.ButtonReleaseMask));
			button.HeightRequest = 24;

			button.SetCompositeColormap ();
		}

		void HandleButtonReleaseEvent(object o, ButtonReleaseEventArgs args)
		{
			window.CenterAndFocusWindow ();
			base.OnActivated ();
		}

		void HandleLeaveNotifyEvent(object o, LeaveNotifyEventArgs args)
		{
			button.QueueDraw ();
			hovered = false;
		}

		void HandleEnterNotifyEvent(object o, EnterNotifyEventArgs args)
		{
			button.QueueDraw ();
			hovered = true;
		}

		void HandleExposeEvent(object o, ExposeEventArgs args)
		{
			using (Context cr = CairoHelper.Create (args.Event.Window)) {
				Gdk.Rectangle area = args.Event.Area;
				cr.AlphaFill ();
				LinearGradient lg = new LinearGradient (area.X, area.Y, area.X, area.Y + area.Height);
				if (hovered) {
					Cairo.Color high = DockPopupMenu.BackgroundColor
						    .ConvertToGdk ()
							.SetMinimumValue (25)
							.ConvertToCairo (DockPopupMenu.BackgroundColor.A);
					
					lg.AddColorStop (0, high);
					lg.AddColorStop (1, DockPopupMenu.BackgroundColor);
				} else {
					lg.AddColorStop (0, DockPopupMenu.BackgroundColor);
					lg.AddColorStop (1, DockPopupMenu.BackgroundColor);
				}
				cr.Pattern = lg;
				cr.Paint ();
				
				cr.MoveTo (area.X, area.Y + .5);
				cr.LineTo (area.X + area.Width, area.Y + .5);
				cr.Color = new Cairo.Color (1, 1, 1, .15);
				cr.LineWidth = 1;
				cr.Stroke ();
				
				DockServices.DrawingService.TextPathAtPoint (cr, 
				                                             string.Format (FormatString, window.Name), 
				                                             new Gdk.Point (area.X + WidthBuffer, area.Y + area.Height / 2),
				                                             area.Width - WidthBuffer * 2,
				                                             Pango.Alignment.Left);
				cr.Color = new Cairo.Color (1, 1, 1);
				cr.Fill ();
				
				lg.Destroy ();
			}
		}
		
		public override void Dispose ()
		{
			button.Destroy ();
			base.Dispose ();
		}
	}
}
