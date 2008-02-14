/* GlossyRoundedFrame.cs
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

namespace Do.UI
{
	public class GlossyRoundedFrame : RoundedFrame
	{
		private int glossHeight;
		private int glossAngle;

		public GlossyRoundedFrame ()
		{
		}

		public int GlossHeight
		{
			get { return glossHeight; }
			set { glossHeight = value; }
		}
		
		public int GlossAngle
		{
			get { return glossAngle; }
			set { glossAngle = value; }
		}
		
		protected void GlossOverlay (Cairo.Context cairo, int x, int y, int width, int height, double radius)
		{
			Cairo.PointD pt1, pt2, pt3;

			glossHeight = height / 2;
			glossAngle = -25;
			pt1 = new Cairo.PointD (x,           glossHeight);
			pt2 = new Cairo.PointD (x+2*width/3, glossHeight+glossAngle);
			pt3 = new Cairo.PointD (x+width/3,   glossHeight+glossAngle);
			
			cairo.MoveTo (x+radius, y);
			cairo.Arc (x+width-radius, y+radius, radius, (Math.PI*1.5), (Math.PI*2));
			cairo.LineTo (x+width, glossHeight);
			cairo.CurveTo (pt2, pt3, pt1);
			cairo.Arc (x+radius, y+radius, radius, Math.PI, (Math.PI*1.5));
		}
		
		protected override void Paint (Gdk.Rectangle area)
		{
			int x, y;
			double radius;
			int width, height;
			Cairo.Context cairo, glare;

			if (!IsDrawable) return;
			if (!drawFrame && !fill) return;

			x = childAlloc.X + Style.XThickness;
			y = childAlloc.Y + Style.YThickness;

			width  = childAlloc.Width - 2 * Style.XThickness;
			height = childAlloc.Height - 2 * Style.Ythickness;

			if (this.radius < 0.0) {
				radius = Math.Min (width, height);
				radius = (radius / 100) * 10;
			} else {
				radius = this.radius;
			}

			using (cairo = Gdk.CairoHelper.Create (GdkWindow))
			using (glare = Gdk.CairoHelper.Create (GdkWindow)) {
				RoundedRectangle (cairo, x, y, width, height, radius);
				cairo.Operator = Cairo.Operator.Over;
				
				GlossOverlay (glare, x, y, width, height, radius);
				glare.Operator = Cairo.Operator.Over;

				if (fill) {
					double r, g, b;
					Cairo.Gradient gloss, fade;
					
					r = (double) fillColor.Red / ushort.MaxValue;
					g = (double) fillColor.Green / ushort.MaxValue;
					b = (double) fillColor.Blue / ushort.MaxValue;

					gloss = new Cairo.LinearGradient (0, 0, 0, height);
					gloss.AddColorStop (0,   new Cairo.Color (r+.25, g+.25, b+.25, fillAlpha));
					gloss.AddColorStop (.25, new Cairo.Color (r,     g,     b,     fillAlpha));
					gloss.AddColorStop (.75, new Cairo.Color (r-.15, g-.15, b-.15, fillAlpha));
					
					fade = new Cairo.LinearGradient (0, 0, 0, height);
					fade.AddColorStop (0,   new Cairo.Color (1, 1, 1, 0));
					fade.AddColorStop (.75, new Cairo.Color (1, 1, 1, .25));
					
					glare.Save ();
					cairo.Save ();
					
					cairo.Pattern = gloss;
					cairo.FillPreserve ();
					
					glare.Pattern = fade;
					glare.FillPreserve ();
					
					cairo.Restore ();
					glare.Restore ();
					
					cairo.Color = new Cairo.Color (r, g, b, fillAlpha);
					cairo.LineWidth = 2;
					cairo.Stroke ();
				}

				if (drawFrame) {
					double r, g, b;
					r = (double) frameColor.Red / ushort.MaxValue;
					g = (double) frameColor.Green / ushort.MaxValue;
					b = (double) frameColor.Blue / ushort.MaxValue;
					cairo.Color = new Cairo.Color (r, g, b, frameAlpha);
					cairo.Stroke ();
				}
			}
		}
	}
}
