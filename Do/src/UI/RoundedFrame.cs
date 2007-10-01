// RoundedFrame.cs created with MonoDevelop
// User: dave at 11:15 AM 8/25/2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using Gtk;
using Gdk;

using Do.Core;

namespace Do.UI
{
	
	public class RoundedFrame : Bin
	{
		
		Rectangle childAlloc;
		double radius;
		
		bool drawFrame;
		Color frameColor;
		double frameAlpha;
		
		bool fill;
		Color fillColor;
		double fillAlpha;
		
		public RoundedFrame () : base ()
		{
			fill = false;
			fillAlpha = 1.0;
			drawFrame = false;
			frameAlpha = 1.0;
			radius = 12.0;
			fillColor = frameColor = new Color (0, 0, 0);
		}
		
		public double Radius {
			get { return radius; }
			set {
				radius = value;
				if (IsDrawable) QueueDraw ();
			}
		}
		
		public bool DrawFrame {
			get { return drawFrame; }
			set {
				drawFrame = value;
				if (IsDrawable) QueueDraw ();
			}
		}
		
		public Color FrameColor {
			get { return frameColor; }
			set {
				fillColor = new Color ((byte)value.Red, (byte)value.Green, (byte)value.Blue);
				if (IsDrawable) QueueDraw ();
			}
		}
		
		public double FrameAlpha {
			get { return frameAlpha; }
			set {
				frameAlpha = value;
				if (IsDrawable) QueueDraw ();
			}
		}
		
		public bool DrawFill {
			get { return fill; }
			set {
				fill = value;
				if (IsDrawable) QueueDraw ();
			}
		}
		
		public Color FillColor {
			get { return fillColor; }
			set {
				fillColor = new Color ((byte)value.Red, (byte)value.Green, (byte)value.Blue);
				if (IsDrawable) QueueDraw ();
			}
		}
		
		public double FillAlpha {
			get { return fillAlpha; }
			set {
				fillAlpha = value;
				if (IsDrawable) QueueDraw ();
			}
		}
		
		protected void cc_rectangle_re (Cairo.Context cairo, double x, double y, double width, double height, double radius)
		{
			double cx, cy;
			
			width -= 2 * radius;
			height -= 2 * radius;
			cairo.MoveTo (x + radius, y);
			cairo.RelLineTo (width, 0.0);
			
			cx = cairo.CurrentPoint.X;
			cy = cairo.CurrentPoint.Y;
			cairo.Arc (cx, cy + radius, radius, 3.0 * (Math.PI / 2.0), 0);
			
			cairo.RelLineTo (0.0, height);

			cx = cairo.CurrentPoint.X;
			cy = cairo.CurrentPoint.Y;
			cairo.Arc (cx - radius, cy, radius, 0, (Math.PI / 2.0));
			cairo.RelLineTo (-width, 0.0);
			
			cx = cairo.CurrentPoint.X;
			cy = cairo.CurrentPoint.Y;
			cairo.Arc (cx, cy - radius, radius, (Math.PI / 2.0), Math.PI);
			cairo.RelLineTo (0.0, -height);

			cx = cairo.CurrentPoint.X;
			cy = cairo.CurrentPoint.Y;
			cairo.Arc (cx + radius, cy, radius, Math.PI, 3.0 * (Math.PI / 2.0));
			
			cairo.ClosePath ();
		}
		
		protected virtual void Paint (Gdk.Rectangle area)
		{
			Cairo.Context cairo;
			int         x, y;
			int         width, height;
			double radius;
			
			if (!IsDrawable) {
				return;
			}
			if (!drawFrame && !fill) {
				/* Nothing to draw. */
				return;
			}
			
			x = childAlloc.X - Style.XThickness;
			y = childAlloc.Y - Style.YThickness;

			width  = childAlloc.Width + 2 * Style.XThickness;
			height = childAlloc.Height + 2 * Style.Ythickness;

			if (this.radius < 0.0) {
				radius = (int) Util.Min (width, height);
				radius = (radius / 100) * 10;
			} else {
				radius = this.radius;
			}
			
			cairo = Gdk.CairoHelper.Create (this.GdkWindow);
			cairo.Rectangle (x, y, width, height);
			cairo.Clip ();

			cc_rectangle_re (cairo, x, y, width, height, radius);
			cairo.Operator = Cairo.Operator.Over;
			
			if (fill) {
				double r, g, b, a;

				r = (double) fillColor.Red / ushort.MaxValue;
				g = (double) fillColor.Green / ushort.MaxValue;
				b = (double) fillColor.Blue / ushort.MaxValue;
				a = fillAlpha / 1.0;
				cairo.Color = new Cairo.Color (r, g, b, a);
				cairo.FillPreserve ();
			}

			if (drawFrame) {
				double r, g, b, a;

				r = (double) frameColor.Red / ushort.MaxValue;
				g = (double) frameColor.Green / ushort.MaxValue;
				b = (double) frameColor.Blue / ushort.MaxValue;
				a = frameAlpha / 1.0;
				cairo.Color = new Cairo.Color (r, g, b, a);
				cairo.Stroke ();
			}
		}
		
		protected override bool OnExposeEvent (EventExpose evnt)
		{
			if (IsDrawable) {
				Paint (evnt.Area);
				base.OnExposeEvent (evnt);
			}
			return false;
		}
		
		protected override void OnSizeRequested (ref Requisition requisition)
		{
			Requisition cr;

			requisition.Height = requisition.Width = 0;
			if (Child != null && Child.Visible) {
				cr = Child.SizeRequest ();
				requisition.Width = (int)Util.Max (requisition.Width, cr.Width);
				requisition.Height += cr.Height;
			}
			requisition.Width += (int)(BorderWidth + Style.XThickness * 2);
			requisition.Height += (int)(BorderWidth + Style.Ythickness * 2);
		}
		
		protected override void OnSizeAllocated (Rectangle allocation)
		{
			Rectangle new_alloc;
			
			new_alloc.X = (int) BorderWidth + Style.XThickness;
			new_alloc.Width = (int) Util.Max (1, allocation.Width - new_alloc.X * 2);
			new_alloc.Y = (int) BorderWidth  + Style.Ythickness;
			new_alloc.Height = (int) Util.Max (1, allocation.Height
			                                        - new_alloc.Y
			                                        - (int) BorderWidth
			                                        - Style.Ythickness);
			new_alloc.X += allocation.X;
			new_alloc.Y += allocation.Y;
			if (IsMapped && new_alloc != childAlloc) {
				GdkWindow.InvalidateRect (new_alloc, false);
			}
			if (Child != null && Child.Visible) {
				Child.SizeAllocate (new_alloc);
			}
			childAlloc = new_alloc;
		}
	}
}
