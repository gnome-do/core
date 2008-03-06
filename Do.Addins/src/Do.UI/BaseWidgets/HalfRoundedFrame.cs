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

namespace Do.UI
{
	
	
	public class HalfRoundedFrame : Frame
	{
		protected Cairo.Context outline;
		
		public HalfRoundedFrame ()
		{
		}
		
		protected override void GetFrame (Cairo.Context cairo)
		{
			cairo.MoveTo (x, y);
			if (radius == 0)
			{
				cairo.Rectangle (x, y, width, height);
			} else {
				cairo.LineTo (x+width, y);
				cairo.Arc (x+width-radius, y+height-radius, radius, 0, (Math.PI*0.5));
				cairo.Arc (x+radius, y+height-radius, radius, (Math.PI*0.5), Math.PI);
				cairo.ClosePath ();
			}
		}
		
		protected void SemiOutline (Cairo.Context outline)
		{
			if (radius == 0)
			{
				outline.MoveTo (x+width, y);
				outline.LineTo (x+width, y+height);
				outline.LineTo (x, y+height);
				outline.LineTo (x, y);
			} else {
				outline.MoveTo (x+width, y);
				outline.Arc (x+width-radius, y+height-radius, radius, 0, (Math.PI*0.5));
				outline.Arc (x+radius, y+height-radius, radius, (Math.PI*0.5), Math.PI);
				outline.LineTo (x, y);
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
			
			cairo.Color = new Cairo.Color (r+offset, g+offset, b+offset, fillAlpha);
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
			SemiOutline (cairo);
			
			cairo.Color = new Cairo.Color (r+brighten, g+brighten, b+brighten, frameAlpha);
			cairo.LineWidth = 2;
			cairo.Stroke ();
			
			cairo.Restore ();
		}

	}
}
