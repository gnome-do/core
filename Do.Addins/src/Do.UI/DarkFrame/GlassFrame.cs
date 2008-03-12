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
		bool drawArrow;
		
		public bool DrawArrow {
			get { return drawArrow; }
			set {
				drawArrow = value;
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
			//gloss.AddColorStop (1, new Cairo.Color (r+down, g+down, b+down, 1));
			
			return gloss;
		}
		
		protected virtual void GetMenuButton (Cairo.Context cairo)
		{
			int triWidth = 11;
			int triHeight = 6;
			
			cairo.MoveTo (x+width-15, y+2);
			cairo.LineTo (x+width-15-triWidth, y+2);
			cairo.LineTo (x+width-15-(triWidth/2), y+2+triHeight);
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
			
			if (drawArrow) {
				cairo.Save ();
				cairo.NewPath ();
				GetMenuButton (cairo);
				cairo.Color = new Color (0, 0, 0, .9);
				cairo.FillPreserve ();
				cairo.Restore ();
			}
		}
		
		protected virtual void GetBorderFrame (Cairo.Context cairo)
		{
			double X,Y,Width,Height;
			X = x+.5;
			Y = y+.5;
			Width = width - 1;
			Height = height - 1;
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
		
		protected override void PaintBorder ()
		{
			double r, g, b;
			
			r = (double) frameColor.Red / ushort.MaxValue;
			g = (double) frameColor.Green / ushort.MaxValue;
			b = (double) frameColor.Blue / ushort.MaxValue;
			
			cairo.Save ();
			GetBorderFrame (cairo);
			
			cairo.LineWidth = 1;
			cairo.Color = new Cairo.Color (r, g, b, frameAlpha);
			cairo.Stroke ();
			
			cairo.Restore ();
		}
	}
}
