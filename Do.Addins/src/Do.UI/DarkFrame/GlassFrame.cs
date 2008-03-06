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
		
		protected override void PaintFill ()
		{
			cairo.Save ();
			
			GetFrame (cairo);
			cairo.Color = new Cairo.Color (.7, .7, .7, .55);
			
			cairo.FillPreserve ();
			cairo.Restore ();
			
			cairo.Save ();
			cairo.NewPath ();
			
			int tempx = x, tempy = y, temph = height, tempw = width;
			double tempr = radius;
			x = x + offset;
			y = y + offset;
			width = width - 2 * offset;
			height = height - 2 * offset;
			
			radius = radius / 1.4;
			
			GetFrame (cairo);
			cairo.Pattern = GetGradient ();
			cairo.FillPreserve ();
			
			cairo.Restore ();
			
			radius = tempr;
			x = tempx;
			y = tempy;
			width = tempw;
			height = temph;
		}
	}
}
