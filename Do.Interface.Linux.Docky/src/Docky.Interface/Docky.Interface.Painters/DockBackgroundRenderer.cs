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
		
		const int ShineWidth = 120;
		const int width = 1500;
		
		public static void RenderDockBackground (Context context, Gdk.Rectangle dockArea, PointD shine)
		{
			if (sr == null || 
			    (DockPreferences.DockIsHorizontal && dockArea.Height != height) ||
			    (!DockPreferences.DockIsHorizontal && dockArea.Width != height)) {
				
				if (sr != null)
					sr.Destroy ();
				
				height = DockPreferences.DockIsHorizontal ? dockArea.Height : dockArea.Width;
				sr = context.Target.CreateSimilar (context.Target.Content, width, dockArea.Height);
				
				using (Context cr = new Context (sr)) {
					cr.SetRoundedRectanglePath (.5, .5, width - 1, height+40, 5); // fall off the bottom
					cr.Color = new Cairo.Color (0.1, 0.1, 0.1, .75);
					cr.FillPreserve ();
			
					// gives the dock a "lifted" look and feel
					cr.Color = new Cairo.Color (0, 0, 0, .6);
					cr.LineWidth = 1;
					cr.Stroke ();
			
					cr.SetRoundedRectanglePath (1.5, 1.5, width - 3, height + 40, 5);
					LinearGradient lg = new LinearGradient (0, 1.5, 0, 10);
					lg.AddColorStop (0, new Cairo.Color (1, 1, 1, .4));
					lg.AddColorStop (1, new Cairo.Color (1, 1, 1, 0));
					cr.Pattern = lg;
					cr.LineWidth = 1;
					cr.Stroke ();
				
					lg.Destroy ();
				}
			}
			
			switch (DockPreferences.Orientation) {
			case DockOrientation.Bottom:
				RenderBottomBackground (context, dockArea, shine);
				break;
			case DockOrientation.Left:
				RenderLeftBackground (context, dockArea, shine);
				break;
			case DockOrientation.Right:
				RenderRightBackground (context, dockArea, shine);
				break;
			case DockOrientation.Top:
				RenderTopBackground (context, dockArea, shine);
				break;
			}
		}

		static void RenderBottomBackground (Context context, Gdk.Rectangle dockArea, PointD shine)
		{
			context.SetSource (sr, dockArea.X, dockArea.Y);
			context.Rectangle (dockArea.X, dockArea.Y, dockArea.Width / 2, dockArea.Height);
			context.Fill ();
			
			context.SetSource (sr, dockArea.X + dockArea.Width - width, dockArea.Y);
			context.Rectangle (dockArea.X + dockArea.Width / 2, dockArea.Y, dockArea.Width - dockArea.Width / 2, dockArea.Height);
			context.Fill ();
			
			if (shine.X == 0 && shine.Y == 0)
				return;
			
			context.Rectangle (dockArea.X + 5, dockArea.Y, dockArea.Width - 10, dockArea.Height);
			context.Clip ();
			
			PointD startShine = new PointD (shine.X - ShineWidth, dockArea.Y + 1.5);
			PointD endShine = new PointD (shine.X + ShineWidth, dockArea.Y + 1.5);
			
			RenderShine (context, startShine, endShine); 
			
			context.ResetClip ();
		}
		
		static void RenderLeftBackground (Context context, Gdk.Rectangle dockArea, PointD shine)
		{
			double rotation = Math.PI * .5;

			context.Translate (height, 0);
			context.Rotate (rotation);
			
			context.SetSource (sr, dockArea.Y, dockArea.X);
			context.Rectangle (dockArea.Y, dockArea.X, dockArea.Height / 2, dockArea.Width);
			context.Fill ();
			
			context.SetSource (sr, dockArea.Y + dockArea.Height - width, dockArea.X);
			context.Rectangle (dockArea.Y + dockArea.Height / 2, dockArea.X, dockArea.Height / 2 + 10, dockArea.Width);
			context.Fill ();

			
			context.Rotate (0 - rotation);
			context.Translate (0 - height, 0);
		}

		static void RenderRightBackground (Context context, Gdk.Rectangle dockArea, PointD shine)
		{
			double rotation = 0 - Math.PI * 0.5;
			double translatex, translatey;
			translatex = 0;
			translatey = width;
			
			context.Translate (translatex, translatey);
			context.Rotate (rotation);
			
			context.SetSource (sr, (width - dockArea.Height) - dockArea.Y, dockArea.X);
			context.Rectangle ((width - dockArea.Height) - dockArea.Y, dockArea.X, dockArea.Height / 2, dockArea.Width);
			context.Fill ();
			
			context.SetSource (sr, 0 - dockArea.Y, dockArea.X);
			context.Rectangle ((width - dockArea.Height) - dockArea.Y + dockArea.Height / 2, dockArea.X, dockArea.Height / 2 + 10, dockArea.Width);
			context.Fill ();

			
			context.Rotate (0 - rotation);
			context.Translate (0 - translatex, 0 - translatey);
		}

		static void RenderTopBackground (Context context, Gdk.Rectangle dockArea, PointD shine)
		{
			context.Scale (1, -1);
			

			context.SetSource (sr, dockArea.X, 0 - dockArea.Y - dockArea.Height);
			context.Rectangle (dockArea.X, 0 - dockArea.Y - dockArea.Height, dockArea.Width / 2, dockArea.Height);
			context.Fill ();

			context.SetSource (sr, dockArea.X + dockArea.Width - width, 0 - dockArea.Y - dockArea.Height);
			context.Rectangle (dockArea.X + dockArea.Width / 2, dockArea.Y - dockArea.Height, dockArea.Width - dockArea.Width / 2, dockArea.Height);
			context.Fill ();

			context.Scale (1, -1);
			
			if (shine.X == 0 && shine.Y == 0)
				return;
			
			context.Rectangle (dockArea.X + 5, dockArea.Y, dockArea.Width - 10, dockArea.Height);
			context.Clip ();
			
			PointD startShine = new PointD (shine.X - ShineWidth, dockArea.Y + dockArea.Height - 1.5);
			PointD endShine = new PointD (shine.X + ShineWidth, dockArea.Y + dockArea.Height - 1.5);
			
			RenderShine (context, startShine, endShine); 
			
			context.ResetClip ();
		}
		
		static void RenderShine (Context context, PointD startShine, PointD endShine)
		{
			context.MoveTo (startShine);
			context.LineTo (endShine);
			
			LinearGradient lg = new LinearGradient (startShine.X, 0, endShine.X, 0);
			lg.AddColorStop (0, new Cairo.Color (1, 1, 1, 0));
			lg.AddColorStop (.35, new Cairo.Color (1, 1, 1, .85));
			lg.AddColorStop (.5, new Cairo.Color (1, 1, 1, .9));
			lg.AddColorStop (.65, new Cairo.Color (1, 1, 1, .85));
			lg.AddColorStop (1, new Cairo.Color (1, 1, 1, 0));
			
			context.Pattern = lg;
			context.LineWidth = 1;
			context.Stroke ();
			
			lg.Destroy ();
		}
	}
}
