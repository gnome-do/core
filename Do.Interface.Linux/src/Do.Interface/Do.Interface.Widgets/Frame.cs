/* Frame.cs
 *
 * GNOME Do is the legal property of its developers. Please refer to the
 * COPYRIGHT file distributed with this source distribution.
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using Gtk;
using Gdk;
// For Mono.Cairo 3.2 compatibility shims
using Do.Interface.CairoUtils;

namespace Do.Interface.Widgets
{
	public class Frame : Bin
	{
		protected Rectangle childAlloc;
		protected double radius;

		protected bool drawFrame;
		protected Color frameColor;
		protected double frameAlpha;
		protected bool drawGradient;

		protected bool fill;
		protected Color fillColor;
		protected double fillAlpha;
		
		protected Cairo.Context cairo;
		protected int height, width;
		protected int x, y;

		public Frame () : base ()
		{
			fill = false;
			fillAlpha = 1.0;
			drawFrame = false;
			frameAlpha = 1.0;
			radius = 12.0;
			fillColor = frameColor = new Color (0, 0, 0);
		}

		public double Radius
		{
			get { return radius; }
			set {
				radius = value;
				if (IsDrawable) QueueDraw ();
			}
		}

		public bool DrawFrame
		{
			get { return drawFrame; }
			set {
				drawFrame = value;
				if (IsDrawable) QueueDraw ();
			}
		}

		public Color FrameColor
		{
			get { return frameColor; }
			set {
				frameColor = new Color ((byte)value.Red, (byte)value.Green, (byte)value.Blue);
				if (IsDrawable) QueueDraw ();
			}
		}

		public double FrameAlpha
		{
			get { return frameAlpha; }
			set {
				frameAlpha = value;
				if (IsDrawable) QueueDraw ();
			}
		}

		public bool DrawFill
		{
			get { return fill; }
			set {
				fill = value;
				if (IsDrawable) QueueDraw ();
			}
		}

		public Color FillColor
		{
			get { return fillColor; }
			set {
				fillColor = new Color ((byte)value.Red, (byte)value.Green, (byte)value.Blue);
				if (IsDrawable) QueueDraw ();
			}
		}

		public double FillAlpha
		{
			get { return fillAlpha; }
			set {
				fillAlpha = value;
				if (IsDrawable) QueueDraw ();
			}
		}

		protected virtual void GetFrame (Cairo.Context cairo)
		{
			if (radius == 0)
			{
				cairo.MoveTo (x, y);
				cairo.Rectangle (x, y, width, height);
			} else {
				cairo.MoveTo (x+radius, y);
				cairo.Arc (x+width-radius, y+radius, radius, (Math.PI*1.5), (Math.PI*2));
				cairo.Arc (x+width-radius, y+height-radius, radius, 0, (Math.PI*0.5));
				cairo.Arc (x+radius, y+height-radius, radius, (Math.PI*0.5), Math.PI);
				cairo.Arc (x+radius, y+radius, radius, Math.PI, (Math.PI*1.5));
			}
		}

		protected virtual void GetBorderFrame (Cairo.Context cairo)
		{
			/* Override coordinates to align to the cairo grid */
			double X, Y, Width, Height;
			X = x + cairo.LineWidth/2.0;
			Y = y + cairo.LineWidth/2.0;
			Width = width - cairo.LineWidth;
			Height = height - cairo.LineWidth;
	
			if (radius == 0)
			{
				cairo.MoveTo (X, Y);
				cairo.Rectangle (X, Y, Width, Height);
			} else {
				cairo.MoveTo (X+radius, Y);
				cairo.Arc (X+Width-radius, Y+radius, radius, (Math.PI*1.5), (Math.PI*2));
				cairo.Arc (X+Width-radius, Y+Height-radius, radius, 0, (Math.PI*0.5));
				cairo.Arc (X+radius, Y+Height-radius, radius, (Math.PI*0.5), Math.PI);
				cairo.Arc (X+radius, Y+radius, radius, Math.PI, (Math.PI*1.5));
			}
		}

		protected virtual void Paint (Gdk.Rectangle area)
		{
			if (!IsDrawable) {
				return;
			}
			if (!drawFrame && !fill) {
				/* Nothing to draw. */
				return;
			}

			/* You shouldn't change the size of the drawing area
			 * to avoid glitches when switching panes, though
			 * you can enlarge the big frame.
			 * This workaround is enlarging only the frame which has
			 * radius == 0, so when the window is not composited.
			 * Pretty ugly, I should think on something better.
			 *
			 * int offset = radius == 0 ? 1 : 0;
			 *
			 * x = childAlloc.X - offset;
			 * y = childAlloc.Y - offset;
			 * width  = childAlloc.Width + offset * 2;
			 * height = childAlloc.Height + offset * 2;
			 */

			x = childAlloc.X;
			y = childAlloc.Y;
			width  = childAlloc.Width;
			height = childAlloc.Height;

			if (this.radius < 0.0) {
				radius = Math.Min (width, height);
				radius = (radius / 100) * 10;
			}

			using (cairo = Gdk.CairoHelper.Create (GdkWindow)) {
				cairo.Operator = Cairo.Operator.Over;

				if (fill) {
					PaintFill ();
				}
				cairo.NewPath ();

				if (drawFrame) {
					PaintBorder ();
				}
			}
		}

		protected virtual Cairo.LinearGradient CreateGradient ()
		{
			return new Cairo.LinearGradient (x, y, x, y+height);
		}
		
		protected virtual void PaintFill ()
		{
			double r, g, b;
			
			r = (double) fillColor.Red / ushort.MaxValue;
			g = (double) fillColor.Green / ushort.MaxValue;
			b = (double) fillColor.Blue / ushort.MaxValue;
			
			cairo.Save ();
			GetFrame (cairo);
			
			if (!drawGradient) {
				cairo.SetSourceRGBA (r, g, b, fillAlpha);
			} else { 
				using (var grad = CreateGradient()) {
					cairo.SetSource (grad);
				}
			}
			
			cairo.FillPreserve ();
			cairo.Restore ();
		}
		
		protected virtual void PaintBorder ()
		{
			double r, g, b;
			
			r = (double) frameColor.Red / ushort.MaxValue;
			g = (double) frameColor.Green / ushort.MaxValue;
			b = (double) frameColor.Blue / ushort.MaxValue;
			
			cairo.Save ();
			cairo.LineWidth = 2;
			GetBorderFrame (cairo);

			cairo.SetSourceRGBA (r, g, b, frameAlpha);
			cairo.Stroke ();
			cairo.Restore ();
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
				requisition.Width = Math.Max (requisition.Width, cr.Width);
				requisition.Height += cr.Height;
			}
			requisition.Width += (int)(BorderWidth + 2);
			requisition.Height += (int)(BorderWidth + 2);
		}

		protected override void OnSizeAllocated (Rectangle allocation)
		{
			Rectangle new_alloc;

			new_alloc.X = (int) BorderWidth + 1;
			new_alloc.Width = Math.Max (1, allocation.Width - new_alloc.X * 2);
			new_alloc.Y = (int) BorderWidth + 1;
			new_alloc.Height = Math.Max (1, allocation.Height - new_alloc.Y * 2);
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
