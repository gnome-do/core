// DockPopupMenu.cs
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

using Do.Interface;
using Do.Interface.CairoUtils;
using Do.Platform;

using Docky.Utilities;

namespace Docky.Interface.Menus
{
	
	
	public class DockPopupMenu : Gtk.Window
	{
		const int TailHeight = 20;
		new const int BorderWidth = 2;
		const int Radius = 10;
		const int Width = 230;
		const double Pointiness = 1.5;
		
		int horizontal_offset;
		int vertical_offset;
		
		Gtk.Alignment align;
		
		public Gtk.VBox Container { get; private set; } 
		
		public DockPopupMenu() : base (Gtk.WindowType.Popup)
		{
			Decorated = false;
			KeepAbove = true;
			AppPaintable = true;
			SkipPagerHint = true;
			SkipTaskbarHint = true;
			Resizable = false;
			Modal = true;
			TypeHint = WindowTypeHint.PopupMenu;
			
			WidthRequest = Width;
			
			this.SetCompositeColormap ();
			
			AddEvents ((int) EventMask.PointerMotionMask | 
			           (int) EventMask.LeaveNotifyMask |
			           (int) EventMask.ButtonPressMask | 
			           (int) EventMask.ButtonReleaseMask |
			           (int) EventMask.FocusChangeMask);
			
			Container = new Gtk.VBox ();
			Build ();
		}
		
		protected virtual void Build ()
		{
			align = new Gtk.Alignment (0.5f, 0.5f, 1, 1);
			
			switch (DockPreferences.Orientation) {
			case DockOrientation.Bottom:
				align.BottomPadding = TailHeight;
				break;
			case DockOrientation.Left:
				align.LeftPadding = TailHeight;
				break;
			case DockOrientation.Right:
				align.RightPadding = TailHeight;
				break;
			case DockOrientation.Top:
				align.TopPadding = TailHeight;
				break;
			}
			
			align.Add (Container);
			Container.BorderWidth = 5;
			Add (align);
			align.ShowAll ();
		}

		
		public virtual void PopUp (IEnumerable<AbstractMenuButtonArgs> args, int x, int y)
		{
			vertical_offset = horizontal_offset = 0;
			ShowAll ();
			Gdk.Rectangle geo = LayoutUtils.MonitorGemonetry ();
			Gtk.Requisition req = SizeRequest ();
			
			Gdk.Point postion;
			switch (DockPreferences.Orientation) {
			case DockOrientation.Bottom:
				postion = new Gdk.Point (x - req.Width / 2, y - req.Height);
				break;
			case DockOrientation.Left:
				postion = new Gdk.Point (x, y - req.Height / 2);
				break;
			case DockOrientation.Right:
				postion = new Gdk.Point (x - req.Width, y - req.Height / 2);
				break;
			case DockOrientation.Top:
				postion = new Gdk.Point (x - req.Width / 2, y);
				break;
			default:
				postion = new Gdk.Point (0, 0);
				break;
			}

			if (DockPreferences.DockIsHorizontal) {
				if (postion.X < geo.X) {
					horizontal_offset = geo.X - postion.X;
				} else if (postion.X + req.Width > geo.X + geo.Width) {
					horizontal_offset = postion.X + req.Width - geo.X - geo.Width;
				}
			} else {
				if (postion.Y < geo.Y) {
					vertical_offset = geo.Y - postion.Y;
				} else if (postion.Y + req.Height > geo.Y + geo.Height) {
					vertical_offset = postion.Y + req.Height - geo.Y - geo.Height;
				}
			}
			postion.X += horizontal_offset;
			postion.Y += vertical_offset;
			Move (postion.X, postion.Y);
			
			Do.Interface.Windowing.PresentWindow (this);
		}
		
		protected override bool OnExposeEvent (Gdk.EventExpose evnt)
		{
			using (Context cr = Gdk.CairoHelper.Create (GdkWindow)) {
				cr.AlphaFill ();
				DrawBackground (cr);
			}
			
			return base.OnExposeEvent (evnt);
		}
		
		protected override bool OnButtonReleaseEvent (Gdk.EventButton evnt)
		{
			Gdk.Rectangle rect;
			GetSize (out rect.Width, out rect.Height);
			
			if (evnt.X < 0 || evnt.Y < 0 || evnt.X > rect.Width || evnt.Y > rect.Height) {
				Do.Interface.Windowing.UnpresentWindow (this);
				Hide ();
			}
			return base.OnButtonReleaseEvent (evnt);
		}
		
		protected override bool OnKeyReleaseEvent (Gdk.EventKey evnt)
		{
			if (evnt.Key == Gdk.Key.Escape) {
				Do.Interface.Windowing.UnpresentWindow (this);
				Hide ();
			}
			return base.OnKeyReleaseEvent (evnt);
		}

		void DrawBackground (Context cr)
		{
			Gdk.Rectangle rect;
			GetSize (out rect.Width, out rect.Height);
			
			SetBackgroundPath (cr);
			
			cr.Color = new Cairo.Color (0.1, 0.1, 0.1, .9);
			cr.FillPreserve ();
			
			cr.Color = new Cairo.Color (1, 1, 1, .8);
			cr.Stroke ();
		}

		void SetBackgroundPath (Context context)
		{
			Gdk.Rectangle size;
			GetSize (out size.Width, out size.Height);

			Gdk.Rectangle mainArea = new Gdk.Rectangle (0, 0, 0, 0);
			switch (DockPreferences.Orientation) {
			case DockOrientation.Bottom:
				mainArea = new Gdk.Rectangle (BorderWidth, 
				                              BorderWidth, 
				                              size.Width - 2 * BorderWidth, 
				                              size.Height - 2 * BorderWidth - TailHeight);
				break;
			case DockOrientation.Left:
				mainArea = new Gdk.Rectangle (BorderWidth + TailHeight, 
				                              BorderWidth, 
				                              size.Width - 2 * BorderWidth - TailHeight, 
				                              size.Height - 2 * BorderWidth);
				break;
			case DockOrientation.Right:
				mainArea = new Gdk.Rectangle (BorderWidth, 
				                              BorderWidth, 
				                              size.Width - 2 * BorderWidth - TailHeight, 
				                              size.Height - 2 * BorderWidth);
				break;
			case DockOrientation.Top:
				mainArea = new Gdk.Rectangle (BorderWidth, 
				                              BorderWidth + TailHeight, 
				                              size.Width - 2 * BorderWidth, 
				                              size.Height - 2 * BorderWidth - TailHeight);
				break;
			}

			PointD topLeftRadialCenter = new PointD (mainArea.X + Radius, mainArea.Y + Radius);
			PointD topRightRadialCenter = new PointD (mainArea.X + mainArea.Width - Radius, mainArea.Y + Radius);
			PointD bottomRightRadialCenter = new PointD (mainArea.X + mainArea.Width - Radius, mainArea.Y + mainArea.Height - Radius);
			PointD bottomLeftRadialCenter = new PointD (mainArea.X + Radius, mainArea.Y + mainArea.Height - Radius);

			// draw top line
			context.MoveTo (mainArea.X, mainArea.Y + Radius);
			context.Arc (topLeftRadialCenter.X, topLeftRadialCenter.Y, Radius, Math.PI, Math.PI * 1.5);
			if (DockPreferences.Orientation == DockOrientation.Top) {
				PointD apex = new PointD (mainArea.X + mainArea.Width / 2 - horizontal_offset, BorderWidth);

				context.LineTo (apex.X - 10 * Pointiness, mainArea.Y);
				context.LineTo (apex);
				context.LineTo (apex.X + 10 * Pointiness, mainArea.Y);
			}

			context.Arc (topRightRadialCenter.X, topRightRadialCenter.Y, Radius, Math.PI * 1.5, Math.PI * 2);
			if (DockPreferences.Orientation == DockOrientation.Right) {
				PointD apex = new PointD (mainArea.X + mainArea.Width + TailHeight, mainArea.Y + mainArea.Height / 2 - vertical_offset);

				context.LineTo (mainArea.X + mainArea.Width, apex.Y - 10 * Pointiness);
				context.LineTo (apex);
				context.LineTo (mainArea.X + mainArea.Width, apex.Y + 10 * Pointiness);
			}

			context.Arc (bottomRightRadialCenter.X, bottomRightRadialCenter.Y, Radius, 0, Math.PI * .5);
			if (DockPreferences.Orientation == DockOrientation.Bottom) {
				PointD apex = new PointD (mainArea.X + mainArea.Width / 2 - horizontal_offset, mainArea.Y + mainArea.Height + TailHeight);

				context.LineTo (apex.X + 10 * Pointiness, mainArea.Y + mainArea.Height);
				context.LineTo (apex);
				context.LineTo (apex.X - 10 * Pointiness, mainArea.Y + mainArea.Height);
			}

			context.Arc (bottomLeftRadialCenter.X, bottomLeftRadialCenter.Y, Radius, Math.PI * .5, Math.PI);
			if (DockPreferences.Orientation == DockOrientation.Left) {
				PointD apex = new PointD (BorderWidth, mainArea.Y + mainArea.Height / 2 - vertical_offset);

				context.LineTo (mainArea.X, apex.Y + 10 * Pointiness);
				context.LineTo (apex);
				context.LineTo (mainArea.X, apex.Y - 10 * Pointiness);
			}

			context.ClosePath ();
		}
	}
}
