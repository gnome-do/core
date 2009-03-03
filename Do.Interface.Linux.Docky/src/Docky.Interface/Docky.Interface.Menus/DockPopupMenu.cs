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
		public static readonly Cairo.Color BackgroundColor = new Cairo.Color (0.1, 0.1, 0.1, .9);
		
		const int TailHeight = 25;
		new const int BorderWidth = 2;
		const int Radius = 10;
		const int Width = 170;
		const double Pointiness = 1.5;
		const double Curviness = 1;
		const double Bluntness = 2;
		
		int horizontal_offset;
		int vertical_offset;
		
		Gtk.Alignment align;
		
		public Gtk.VBox Container { get; private set; }

		// we are making a new one here for speed reasons
		public new bool Visible { get; private set; }
		
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
			align.LeftPadding = 4;
			align.RightPadding = 3;
			align.TopPadding = align.BottomPadding = 7;
			
			switch (DockPreferences.Orientation) {
			case DockOrientation.Bottom:
				align.BottomPadding += TailHeight + 3;
				break;
			case DockOrientation.Top:
				align.TopPadding += TailHeight + 3;
				break;
			}
			
			align.Add (Container);
			Add (align);
			align.ShowAll ();
		}

		
		public virtual void PopUp (IEnumerable<AbstractMenuArgs> args, int x, int y)
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
			case DockOrientation.Top:
				postion = new Gdk.Point (x - req.Width / 2, y);
				break;
			default:
				postion = new Gdk.Point (0, 0);
				break;
			}

			if (postion.X < geo.X) {
				horizontal_offset = geo.X - postion.X;
			} else if (postion.X + req.Width > geo.X + geo.Width) {
				horizontal_offset = postion.X + req.Width - geo.X - geo.Width;
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

		protected override void OnShown ()
		{
			Visible = true;
			base.OnShown ();
		}

		protected override void OnHidden ()
		{
			Visible = false;
			base.OnHidden ();
		}

		void DrawBackground (Context cr)
		{
			Gdk.Rectangle rect;
			GetSize (out rect.Width, out rect.Height);
			
			SetBackgroundPath (cr);
			
			cr.Color = BackgroundColor;
			cr.FillPreserve ();
			
			cr.Color = new Cairo.Color (1, 1, 1, .4);
			cr.LineWidth = 1;
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
			case DockOrientation.Top:
				mainArea = new Gdk.Rectangle (BorderWidth, 
				                              BorderWidth + TailHeight, 
				                              size.Width - 2 * BorderWidth, 
				                              size.Height - 2 * BorderWidth - TailHeight);
				break;
			}

			context.Translate (.5, .5);
			mainArea.Width -= 1;
			mainArea.Height -= 1;
			
			PointD topLeftRadialCenter = new PointD (mainArea.X + Radius, mainArea.Y + Radius);
			PointD topRightRadialCenter = new PointD (mainArea.X + mainArea.Width - Radius, mainArea.Y + Radius);
			PointD bottomRightRadialCenter = new PointD (mainArea.X + mainArea.Width - Radius, mainArea.Y + mainArea.Height - Radius);
			PointD bottomLeftRadialCenter = new PointD (mainArea.X + Radius, mainArea.Y + mainArea.Height - Radius);


			context.MoveTo (mainArea.X, mainArea.Y + Radius);
			if (DockPreferences.Orientation == DockOrientation.Top) {
				context.RelCurveTo (0, 0 - TailHeight * Curviness,
				                    mainArea.Width / 2 - 10 * Bluntness, 0 - TailHeight * (1 - Curviness),
				                    mainArea.Width / 2, 0 - TailHeight);
				context.RelCurveTo (10 * Bluntness, TailHeight * Curviness,
				                    mainArea.Width / 2, TailHeight * (1 - Curviness),
				                    mainArea.Width / 2, TailHeight);
			} else {
				context.Arc (topLeftRadialCenter.X, topLeftRadialCenter.Y, Radius, Math.PI, Math.PI * 1.5);
				context.Arc (topRightRadialCenter.X, topRightRadialCenter.Y, Radius, Math.PI * 1.5, Math.PI * 2);
			}
			
			if (DockPreferences.Orientation == DockOrientation.Bottom) {
				context.LineTo (bottomRightRadialCenter.X + Radius, bottomRightRadialCenter.Y);
				context.RelCurveTo (0, TailHeight * Curviness,
				                    0 - mainArea.Width / 2 + 10 * Bluntness, TailHeight * (1 - Curviness),
				                    0 - mainArea.Width / 2, TailHeight);
				
				context.RelCurveTo (0 - 10 * Bluntness, 0 - TailHeight * Curviness,
				                    0 - mainArea.Width / 2, 0 - TailHeight * (1 - Curviness),
				                    0 - mainArea.Width / 2, 0 - TailHeight);
			} else {
				context.Arc (bottomRightRadialCenter.X, bottomRightRadialCenter.Y, Radius, 0, Math.PI * .5);
				context.Arc (bottomLeftRadialCenter.X, bottomLeftRadialCenter.Y, Radius, Math.PI * .5, Math.PI);
			}
			

			context.ClosePath ();

			context.Translate (-.5, -.5);
		}
	}
}
