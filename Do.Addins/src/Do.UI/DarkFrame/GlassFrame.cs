// GlassFrame.cs
//
//GNOME Do is the legal property of its developers. Please refer to the
//COPYRIGHT file distributed with this
//source distribution.
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
//
//

using System;

using Cairo;

namespace Do.UI
{
	
	
	public class GlassFrame : Frame
	{
		int offset;
		bool hoverArrow;
		
		public bool HoverArrow {
			get { return hoverArrow; }
			set {
				hoverArrow = value;
				QueueDraw ();
			}
		}
		
		public GlassFrame(int offset)
		{
			this.offset = offset;
		}
		
		protected override LinearGradient GetGradient ()
		{
			double r, g, b;
			
			LinearGradient gloss = base.GetGradient ();
			r = (double) fillColor.Red / ushort.MaxValue;
			g = (double) fillColor.Green / ushort.MaxValue;
			b = (double) fillColor.Blue / ushort.MaxValue;
			
			gloss.AddColorStop (0, new Cairo.Color (r, g, b, 1));
			gloss.AddColorStop (1, new Cairo.Color (0, 0, 0, 1));
			
			return gloss;
		}
		
		protected virtual void GetMenuButton (Cairo.Context cairo)
		{
			int triWidth, triHeight, start_x, start_y;
			if (HoverArrow)
			{
				triWidth = 13;
				triHeight = 7;
				start_x = x+width-19;
			} else {
				triWidth = 11;
				triHeight = 6;
				start_x = x+width-20;
			}
			start_y = y+offset+3;
			
			cairo.MoveTo (start_x,              start_y);
			cairo.LineTo (start_x-triWidth,     start_y);
			cairo.LineTo (start_x-(triWidth/2), start_y+triHeight);
			cairo.ClosePath ();
		}
		
		protected override void PaintFill ()
		{
			cairo.Save ();
			//get big frame
			GetFrame (cairo);
			cairo.Color = new Cairo.Color (.7, .7, .7, .55);
			
			cairo.FillPreserve ();
			cairo.Restore ();
			
			cairo.Save ();
			cairo.NewPath ();
			//save old variables
			int tempx = x, tempy = y, temph = height, tempw = width;
			double tempr = radius;
			//set new variable to constrain box
			x += offset;
			y += offset;
			width -= 2 * offset;
			height -= 2 * offset;
			radius /= 1.4;
			//get reduced frame
			GetFrame (cairo);
			cairo.Pattern = GetGradient ();
			cairo.FillPreserve ();
			cairo.Restore ();
			//restore variables from temps
			radius = tempr;
			x = tempx;
			y = tempy;
			width = tempw;
			height = temph;
			
			cairo.Save ();
			cairo.NewPath ();
			GetMenuButton (cairo);
			cairo.Color = new Color (1, 1, 1, .9);
			cairo.FillPreserve ();
			cairo.Restore ();
		}
		
		protected override void PaintBorder ()
		{
			double r, g, b;
			
			r = (double) frameColor.Red / ushort.MaxValue;
			g = (double) frameColor.Green / ushort.MaxValue;
			b = (double) frameColor.Blue / ushort.MaxValue;
			
			cairo.Save ();
			cairo.LineWidth = 1;
			GetBorderFrame (cairo);

			cairo.Color = new Cairo.Color (r, g, b, frameAlpha);
			cairo.Stroke ();
			
			cairo.Restore ();
		}
	}
}
