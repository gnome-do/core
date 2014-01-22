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

using Cairo;
using Gtk;
using Gdk;

// For Mono.Cairo 3.2 compatibility shims
using Do.Interface.CairoUtils;


namespace Do.Interface.Widgets
{
	public class GlossyRoundedFrame : Frame
	{
		protected double glossHeight = .5;
		protected int glossAngle = -25;

		public GlossyRoundedFrame ()
		{
			drawGradient = true;
		}

		public double GlossHeight
		{
			get { return glossHeight; }
			set {
				if (value >= 0 || value <= 1)
					glossHeight = value; 
			}
		}
		
		public int GlossAngle
		{
			get { return glossAngle; }
			set { glossAngle = value; }
		}
		
		protected void GlossOverlay (Context glare)
		{
			Cairo.PointD pt1, pt2, pt3;

			int localGlossHeight = (int) (height * glossHeight);
			
			pt1 = new Cairo.PointD (x,           localGlossHeight);
			pt2 = new Cairo.PointD (x+2*width/3, localGlossHeight+glossAngle);
			pt3 = new Cairo.PointD (x+width/3,   localGlossHeight+glossAngle);
			
			if (radius != 0) {
				glare.MoveTo  (x+radius,       y);
				glare.Arc     (x+width-radius, y+radius, radius, (Math.PI*1.5), (Math.PI*2));
				glare.LineTo  (x+width,        localGlossHeight);
				glare.CurveTo (pt2,            pt3,      pt1);
				glare.Arc     (x+radius,       y+radius, radius, Math.PI, (Math.PI*1.5));
			} else {
				glare.MoveTo    (x,       y);
				glare.LineTo    (x+width, y);
				glare.LineTo    (x+width, localGlossHeight);
				glare.CurveTo   (pt2,     pt3, pt1);
				glare.ClosePath ();
			}
			
		}
		
		protected override LinearGradient CreateGradient ()
		{
			double r, g, b;
			
			var gloss = base.CreateGradient ();
			r = (double) fillColor.Red / ushort.MaxValue;
			g = (double) fillColor.Green / ushort.MaxValue;
			b = (double) fillColor.Blue / ushort.MaxValue;
			
			gloss.AddColorStop (0,   new Cairo.Color (r+.25, g+.25, b+.25, fillAlpha));
			gloss.AddColorStop (.25, new Cairo.Color (r,     g,     b,     fillAlpha));
			gloss.AddColorStop (.75, new Cairo.Color (r-.15, g-.15, b-.15, fillAlpha));
			
			return gloss;
		}

		
		protected override void PaintFill ()
		{
			base.PaintFill ();
			
			using (var fade = new Cairo.LinearGradient (0, 0, 0, height)) {
				fade.AddColorStop (0, new Cairo.Color (1, 1, 1, 0));
				fade.AddColorStop (.75, new Cairo.Color (1, 1, 1, .25));
			
				cairo.Save ();
				cairo.NewPath ();
				GlossOverlay (cairo);
			
				cairo.SetSource (fade);
				cairo.FillPreserve ();
			
				cairo.Restore ();
			}
		}
	}
}
