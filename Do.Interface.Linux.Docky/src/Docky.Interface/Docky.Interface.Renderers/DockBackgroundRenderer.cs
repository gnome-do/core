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

namespace Docky.Interface.Renderers
{
	
	
	public static class DockBackgroundRenderer
	{
		public static void RenderDockBackground (Context cr, Gdk.Rectangle dockArea)
		{
			cr.SetRoundedRectanglePath (dockArea.X+.5, dockArea.Y+.5, dockArea.Width-1, dockArea.Height+40, 5); //fall off the bottom
			cr.Color = new Cairo.Color (0.1, 0.1, 0.1, .75);
			cr.FillPreserve ();
			
			//gives the dock a "lifted" look and feel
			cr.Color = new Cairo.Color (0, 0, 0, .6);
			cr.LineWidth = 1;
			cr.Stroke ();
			
			cr.SetRoundedRectanglePath (dockArea.X+1.5, dockArea.Y+1.5, dockArea.Width-3, dockArea.Height+40, 5);
			LinearGradient lg = new LinearGradient (0, dockArea.Y+1.5, 0, dockArea.Y+10);
			lg.AddColorStop (0, new Cairo.Color (1, 1, 1, .4));
			lg.AddColorStop (1, new Cairo.Color (1, 1, 1, 0));
			cr.Pattern = lg;
			cr.LineWidth = 1;
			cr.Stroke ();
			
			lg.Destroy ();
		}
	}
}
