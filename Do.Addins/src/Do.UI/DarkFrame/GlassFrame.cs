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
			double up, down;
			
			up   =  .2;
			down = -.2;
			
			LinearGradient gloss = base.GetGradient ();
			r = (double) fillColor.Red / ushort.MaxValue;
			g = (double) fillColor.Green / ushort.MaxValue;
			b = (double) fillColor.Blue / ushort.MaxValue;
			
			gloss.AddColorStop (0, new Cairo.Color (r+up,   g+up,   b+up,   1));
			gloss.AddColorStop (1, new Cairo.Color (r+down, g+down, b+down, 1));
			
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
	}
}
