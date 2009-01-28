// DockBackgroundRenderer.cs
// 
// Copyright (C) 2008 GNOME Do
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using System;
using System.Collections.Generic;

using Cairo;
using Gdk;

using Do.Interface.CairoUtils;

using Docky.Utilities;

namespace Docky.Interface.Painters
{
	
	
	public static class DockBackgroundRenderer
	{
		static Surface sr;
		static int height;
		
		public static void RenderDockBackground (Context context, Gdk.Rectangle dockArea)
		{
			if (sr == null || 
			    (DockPreferences.DockIsHorizontal && dockArea.Height != height) ||
			    (!DockPreferences.DockIsHorizontal && dockArea.Width != height)) {
				
				if (sr != null)
					sr.Destroy ();
				
				height = DockPreferences.DockIsHorizontal ? dockArea.Height : dockArea.Width;
				sr = context.Target.CreateSimilar (context.Target.Content, 1000, dockArea.Height);
				
				using (Context cr = new Context (sr)) {
					cr.SetRoundedRectanglePath (.5, .5, 1000 - 1, height+40, 5); // fall off the bottom
					cr.Color = new Cairo.Color (0.1, 0.1, 0.1, .75);
					cr.FillPreserve ();
			
					// gives the dock a "lifted" look and feel
					cr.Color = new Cairo.Color (0, 0, 0, .6);
					cr.LineWidth = 1;
					cr.Stroke ();
			
					cr.SetRoundedRectanglePath (1.5, 1.5, 1000 - 3, height + 40, 5);
					LinearGradient lg = new LinearGradient (0, 1.5, 0, 10);
					lg.AddColorStop (0, new Cairo.Color (1, 1, 1, .4));
					lg.AddColorStop (1, new Cairo.Color (1, 1, 1, 0));
					cr.Pattern = lg;
					cr.LineWidth = 1;
					cr.Stroke ();
				
					lg.Destroy ();
				}
			}
			
//			context.SetSource (sr, dockArea.X, dockArea.Y);
//			context.Rectangle (dockArea.X, dockArea.Y, dockArea.Width / 2, dockArea.Height);
//			context.Fill ();
//			
//			context.SetSource (sr, dockArea.X + dockArea.Width - 1000, dockArea.Y);
//			context.Rectangle (dockArea.X + dockArea.Width / 2, dockArea.Y, dockArea.Width - dockArea.Width / 2, dockArea.Height);
//			context.Fill ();
			switch (DockPreferences.Orientation) {
			case DockOrientation.Bottom:
				RenderBottomBackground (context, dockArea);
				break;
			case DockOrientation.Left:
				RenderLeftBackground (context, dockArea);
				break;
			case DockOrientation.Right:
				RenderRightBackground (context, dockArea);
				break;
			case DockOrientation.Top:
				RenderTopBackground (context, dockArea);
				break;
			}
		}

		static void RenderBottomBackground (Context context, Gdk.Rectangle dockArea)
		{
			context.SetSource (sr, dockArea.X, dockArea.Y);
			context.Rectangle (dockArea.X, dockArea.Y, dockArea.Width / 2, dockArea.Height);
			context.Fill ();
			
			context.SetSource (sr, dockArea.X + dockArea.Width - 1000, dockArea.Y);
			context.Rectangle (dockArea.X + dockArea.Width / 2, dockArea.Y, dockArea.Width - dockArea.Width / 2, dockArea.Height);
			context.Fill ();
		}
		
		static void RenderLeftBackground (Context context, Gdk.Rectangle dockArea)
		{
			double rotation = Math.PI * .5;

			context.Translate (height, 0);
			context.Rotate (rotation);
			
			context.SetSource (sr, dockArea.Y, dockArea.X);
			context.Rectangle (dockArea.Y, dockArea.X, dockArea.Height / 2, dockArea.Width);
			context.Fill ();
			
			context.SetSource (sr, dockArea.Y + dockArea.Height - 1000, dockArea.X);
			context.Rectangle (dockArea.Y + dockArea.Height / 2, dockArea.X, dockArea.Height / 2 + 10, dockArea.Width);
			context.Fill ();

			
			context.Rotate (0 - rotation);
			context.Translate (0 - height, 0);
		}

		static void RenderRightBackground (Context context, Gdk.Rectangle dockArea)
		{
			double rotation = 0 - Math.PI * 0.5;
			double translatex, translatey;
			translatex = 0;
			translatey = 1000;
			
			context.Translate (translatex, translatey);
			context.Rotate (rotation);
			
			context.SetSource (sr, (1000 - dockArea.Height) - dockArea.Y, dockArea.X);
			context.Rectangle ((1000 - dockArea.Height) - dockArea.Y, dockArea.X, dockArea.Height / 2, dockArea.Width);
			context.Fill ();
			
			context.SetSource (sr, 0 - dockArea.Y, dockArea.X);
			context.Rectangle ((1000 - dockArea.Height) - dockArea.Y + dockArea.Height / 2, dockArea.X, dockArea.Height / 2 + 10, dockArea.Width);
			context.Fill ();

			
			context.Rotate (0 - rotation);
			context.Translate (0 - translatex, 0 - translatey);
		}

		static void RenderTopBackground (Context context, Gdk.Rectangle dockArea)
		{
			context.Scale (1, -1);
			

			context.SetSource (sr, dockArea.X, 0 - dockArea.Y - dockArea.Height);
			context.Rectangle (dockArea.X, 0 - dockArea.Y - dockArea.Height, dockArea.Width / 2, dockArea.Height);
			context.Fill ();

			context.SetSource (sr, dockArea.X + dockArea.Width - 1000, 0 - dockArea.Y - dockArea.Height);
			context.Rectangle (dockArea.X + dockArea.Width / 2, dockArea.Y - dockArea.Height, dockArea.Width - dockArea.Width / 2, dockArea.Height);
			context.Fill ();

			context.Scale (1, -1);
		}
	}
}
