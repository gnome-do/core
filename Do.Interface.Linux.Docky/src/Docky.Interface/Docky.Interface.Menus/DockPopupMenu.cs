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
		public static readonly Cairo.Color BackgroundColor = new Cairo.Color (0.13, 0.13, 0.13, .95);
		
		const int TailHeight = 20;
		const int TailWidth = 25;
		new const int BorderWidth = 2;
		const int HeaderSize = 20;
		const int Radius = 6;
		const int Width = 180;
		const double Curviness = .05;
		const string FormatString = "<b>{0}</b>";
		
		int horizontal_offset;
		int vertical_offset;
		
		string header;
		
		Gtk.Alignment align;
		
		public Gtk.VBox Container { get; private set; }

		// we are making a new one here for speed reasons
		public new bool Visible { get; private set; }
		
		int HeaderTextOffset {
			get {
				if (DockPreferences.Orientation == DockOrientation.Bottom)
					return (HeaderSize + 10) / 2;
				else
					return (TailHeight + 3) + (HeaderSize + 10) / 2;
			}
		}
		
		public DockPopupMenu() : base (Gtk.WindowType.Popup)
		{
			AcceptFocus = false;
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
			
			DockPreferences.OrientationChanged += SetAlignment;
		}
		
		protected virtual void Build ()
		{
			align = new Gtk.Alignment (0.5f, 0.5f, 1, 1);
			SetAlignment ();
			
			align.Add (Container);
			Add (align);
			align.ShowAll ();
		}

		void SetAlignment ()
		{
			align.LeftPadding = 4;
			align.RightPadding = 3;
			align.TopPadding = align.BottomPadding = 7;
			align.TopPadding += HeaderSize;
			
			switch (DockPreferences.Orientation) {
			case DockOrientation.Bottom:
				align.BottomPadding += TailHeight + 3;
				break;
			case DockOrientation.Top:
				align.TopPadding += TailHeight + 3;
				break;
			}
		}
		
		public virtual void PopUp (string description, IEnumerable<AbstractMenuArgs> args, int x, int y)
		{
			header = description;
			vertical_offset = horizontal_offset = 0;
			ShowAll ();
			Gdk.Rectangle geo = LayoutUtils.MonitorGeometry ();
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
				horizontal_offset = (geo.X + geo.Width) - (postion.X + req.Width);
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
			
			LinearGradient lg = (DockPreferences.Orientation == DockOrientation.Bottom) ? 
				new LinearGradient (0, rect.Height - TailHeight, 0, rect.Height) : new LinearGradient (0, TailHeight, 0, 0);
			lg.AddColorStop (0, BackgroundColor);
			lg.AddColorStop (1, new Cairo.Color (BackgroundColor.R, BackgroundColor.G, BackgroundColor.B, 0));
			cr.Pattern = lg;
			cr.LineWidth = 3;
			cr.StrokePreserve ();
			
			lg.Destroy ();
			lg = (DockPreferences.Orientation == DockOrientation.Bottom) ? 
				new LinearGradient (0, rect.Height - TailHeight, 0, rect.Height) : new LinearGradient (0, TailHeight, 0, 0);
			lg.AddColorStop (0, new Cairo.Color (1, 1, 1, .25));
			lg.AddColorStop (1, new Cairo.Color (1, 1, 1, 0));
			cr.Pattern = lg;
			cr.LineWidth = 1;
			cr.Stroke ();
			
			lg.Destroy ();
			
			TextRenderContext context = new TextRenderContext (cr, string.Format (FormatString, header), Width - 16);
			context.LeftCenteredPoint = new Gdk.Point (8, HeaderTextOffset);
			context.Alignment = Pango.Alignment.Center;
			
			Core.DockServices.DrawingService.TextPathAtPoint (context);
			cr.Color = new Cairo.Color (1, 1, 1);
			cr.Fill ();
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
			
			context.Arc (topLeftRadialCenter.X, topLeftRadialCenter.Y, Radius, Math.PI, Math.PI * 1.5);
			if (DockPreferences.Orientation == DockOrientation.Top) {
				Gdk.Point vertex = new Gdk.Point ();
				vertex.X = mainArea.X + mainArea.Width / 2;
				vertex.Y = mainArea.Y - TailHeight;
				vertex.X -= horizontal_offset;
				
				Gdk.Point top = new Gdk.Point (vertex.X, vertex.Y + TailHeight);
				
				context.LineTo (top.X - TailWidth, top.Y);
				context.CurveTo (top.X - TailWidth * Curviness, top.Y, vertex.X, vertex.Y, vertex.X, vertex.Y);
				context.CurveTo (vertex.X, vertex.Y, top.X + TailWidth * Curviness, top.Y, top.X + TailWidth, top.Y);
			}
			context.Arc (topRightRadialCenter.X, topRightRadialCenter.Y, Radius, Math.PI * 1.5, Math.PI * 2);
			context.Arc (bottomRightRadialCenter.X, bottomRightRadialCenter.Y, Radius, 0, Math.PI * .5);
			if (DockPreferences.Orientation == DockOrientation.Bottom) {
				Gdk.Point vertex = new Gdk.Point ();
				vertex.X = mainArea.X + mainArea.Width / 2;
				vertex.Y = mainArea.Y + mainArea.Height + TailHeight;
				vertex.X -= horizontal_offset;
				
				Gdk.Point top = new Gdk.Point (vertex.X, vertex.Y - TailHeight);
				
				context.LineTo (top.X + TailWidth, top.Y);
				context.CurveTo (top.X + TailWidth * Curviness, top.Y, vertex.X, vertex.Y, vertex.X, vertex.Y);
				context.CurveTo (vertex.X, vertex.Y, top.X - TailWidth * Curviness, top.Y, top.X - TailWidth, top.Y);
			}
			context.Arc (bottomLeftRadialCenter.X, bottomLeftRadialCenter.Y, Radius, Math.PI * .5, Math.PI);
			

			context.ClosePath ();

			context.Translate (-.5, -.5);
		}
		
		public override void Dispose ()
		{
			DockPreferences.OrientationChanged -= SetAlignment;
			base.Dispose ();
		}
	}
}
