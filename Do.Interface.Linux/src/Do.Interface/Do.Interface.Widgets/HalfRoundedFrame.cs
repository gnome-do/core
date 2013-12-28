/* HalfRoundedFrame.cs
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

namespace Do.Interface.Widgets
{
	public class HalfRoundedFrame : Frame
	{
		protected Cairo.Context outline;
		
		public HalfRoundedFrame ()
		{
		}
		
		protected override void GetFrame (Cairo.Context cairo)
		{
			if (radius == 0) {
				cairo.MoveTo (x, y);
				cairo.Rectangle (x, y, width, height);
			} else {
				cairo.MoveTo (x, y);
				cairo.LineTo (x+width, y);
				cairo.Arc (x+width-radius, y+height-radius, radius, 0, (Math.PI*0.5));
				cairo.Arc (x+radius, y+height-radius, radius, (Math.PI*0.5), Math.PI);
				cairo.ClosePath ();
			}
		}
		
		protected void SemiOutline (Cairo.Context outline)
		{
			/* Override coordinates to align to the cairo grid */
			double X, Y, Width, Height;
			X = x + cairo.LineWidth/2.0;
			Y = y + cairo.LineWidth/2.0;
			Width = width - cairo.LineWidth;
			Height = height - cairo.LineWidth;

			if (radius == 0) {
				outline.MoveTo (X+Width, Y);
				outline.LineTo (X+Width, Y+Height);
				outline.LineTo (X, Y+Height);
				outline.LineTo (X, Y);
			} else {
				outline.MoveTo (X+width, y);
				outline.Arc (X+Width-radius, Y+Height-radius, radius, 0, (Math.PI*0.5));
				outline.Arc (x+radius, Y+Height-radius, radius, (Math.PI*0.5), Math.PI);
				outline.LineTo (X, Y);
			}
		}
		
		protected override void PaintFill ()
		{
			double r, g, b, offset;
			offset = -.15;
			
			r = (double) fillColor.Red / ushort.MaxValue;
			g = (double) fillColor.Green / ushort.MaxValue;
			b = (double) fillColor.Blue / ushort.MaxValue;
			
			cairo.Save ();
			GetFrame (cairo);
			
			cairo.SetSourceRGBA (r+offset, g+offset, b+offset, fillAlpha);
			cairo.FillPreserve ();
			cairo.Restore ();
		}

		protected override void PaintBorder ()
		{
			double r, g, b, brighten = .2;
			
			r = (double) fillColor.Red / ushort.MaxValue;
			g = (double) fillColor.Green / ushort.MaxValue;
			b = (double) fillColor.Blue / ushort.MaxValue;
			
			cairo.Save ();
			cairo.LineWidth = 2;
			SemiOutline (cairo);
			
			cairo.SetSourceRGBA (r+brighten, g+brighten, b+brighten, frameAlpha);
			cairo.Stroke ();
			
			cairo.Restore ();
		}

	}
}
