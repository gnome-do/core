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
		
		public static void RenderDockBackground (Context context, Gdk.Rectangle dockArea)
		{
			if (sr == null || dockArea.Height != height) {
				
				if (sr != null)
					sr.Destroy ();
				
				height = dockArea.Height;
				sr = context.Target.CreateSimilar (context.Target.Content, width, dockArea.Height);
				
				using (Context cr = new Context (sr)) {
					cr.SetRoundedRectanglePath (.5, .5, width - 1, height + 40, 5); // fall off the bottom
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
				RenderBackground (context, dockArea);
				break;
			case DockOrientation.Top:
				context.Scale (1, -1);
				context.Translate (0, 0 - (dockArea.Height + dockArea.Y));
				
				RenderBackground (context, dockArea);
				
				context.Translate (0, dockArea.Height + dockArea.Y);
				context.Scale (1, -1);
				break;
			}
		}

		static void RenderBackground (Context context, Gdk.Rectangle dockArea)
		{
			
			context.SetSource (sr, dockArea.X, dockArea.Y);
			context.Rectangle (dockArea.X, dockArea.Y, dockArea.Width / 2, dockArea.Height);
			context.Fill ();
			
			context.SetSource (sr, dockArea.X + dockArea.Width - width, dockArea.Y);
			context.Rectangle (dockArea.X + dockArea.Width / 2, dockArea.Y, dockArea.Width - dockArea.Width / 2, dockArea.Height);
			context.Fill ();
		}
	}
}
