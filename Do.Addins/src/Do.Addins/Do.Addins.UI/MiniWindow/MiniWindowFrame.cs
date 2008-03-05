// MiniWindowFrame.cs
//
//  Copyright (C) 2008 Jason Smith
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
using Gdk;
using Gtk;

namespace Do.Addins.UI
{
	
	
	public class MiniWindowFrame : GlossyRoundedFrame
	{
		public MiniWindowFrame ()
		{
			GlossHeight = .18;
			GlossAngle = 5;
		}
		
		protected override void Paint (Gdk.Rectangle area)
		{
			if (!IsDrawable) {
				return;
			}
			if (!drawFrame && !fill) {
				/* Nothing to draw. */
				return;
			}

			// Why thickness?? Isn't it useless?
			x = childAlloc.X + Style.XThickness;
			y = childAlloc.Y + Style.YThickness;

			
			//FIXME
			width  = childAlloc.Width - 2 * Style.XThickness;
			height = (childAlloc.Height - 2 * Style.Ythickness);

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
		
		protected override void PaintFill ()
		{
			base.PaintFill ();
			
			cairo.Save ();
			cairo.NewPath ();
			GlossOverlay (cairo);
			
			cairo.Color = new Cairo.Color (1, 1, 1, .2);
			cairo.FillPreserve ();
			
			cairo.Restore ();
		}
	}
}
