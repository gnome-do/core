// Util.cs
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
using System.Diagnostics;

using Do.Universe;
using Do.Interface.CairoUtils;

using Cairo;
using Gdk;

using Docky.Utilities;

namespace Docky.Interface
{
	public enum ClickAction {
		Focus,
		Minimize,
		Restore,
		None,
	}
	
	public static class Util
	{
		public static Surface GetBorderedTextSurface (string text, int max_width, Surface similar) 
		{
			return GetBorderedTextSurface (text, max_width, similar, DockOrientation.Bottom);
		}
		
		/// <summary>
		/// Gets a surface containing a transparent black rounded rectangle with the provided text on top.
		/// </summary>
		/// <param name="text">
		/// A <see cref="System.String"/>
		/// </param>
		/// <param name="max_width">
		/// A <see cref="System.Int32"/>
		/// </param>
		/// <param name="similar">
		/// A <see cref="Surface"/>
		/// </param>
		/// <returns>
		/// A <see cref="Surface"/>
		/// </returns>
		public static Surface GetBorderedTextSurface (string text, int max_width, Surface similar, DockOrientation orientation)
		{
			Surface sr;
			sr = similar.CreateSimilar (similar.Content, max_width, 22);
			
			Context cr = new Context (sr);

			Pango.Layout layout = Pango.CairoHelper.CreateLayout (cr);
			layout.FontDescription = Pango.FontDescription.FromString ("sans-serif 11");
			layout.Width = Pango.Units.FromPixels (max_width - 18);
			layout.SetMarkup (text);
			switch (orientation) {
			case DockOrientation.Left:
				layout.Alignment = Pango.Alignment.Left;
				break;
			case DockOrientation.Right:
				layout.Alignment = Pango.Alignment.Right;
				break;
			default:
				layout.Alignment = Pango.Alignment.Center;
				break;
			}
			layout.Ellipsize = Pango.EllipsizeMode.End;
			
			Pango.Rectangle rect1, rect2;
			layout.GetExtents (out rect1, out rect2);
			
			cr.SetRoundedRectanglePath (Pango.Units.ToPixels (rect2.X) + .5, .5, Pango.Units.ToPixels (rect2.Width) + 17, 21, 5);
			cr.Color = new Cairo.Color (0.1, 0.1, 0.1, .75);
			cr.FillPreserve ();

			cr.Color = new Cairo.Color (1, 1, 1, .55);
			cr.LineWidth = 1;
			cr.Stroke ();

			Pango.Layout shadow = layout.Copy();
			shadow.Indent = 1;

			cr.Translate (10, 2);
			cr.Translate(1,1);
			Pango.CairoHelper.LayoutPath (cr, shadow);
			cr.Color = new Cairo.Color (0, 0, 0, 0.6);
			cr.Fill ();
			cr.Translate(-1,-1);

			Pango.CairoHelper.LayoutPath (cr, layout);
			cr.Color = new Cairo.Color (1, 1, 1);
			cr.Fill ();

			(cr as IDisposable).Dispose ();
			shadow.FontDescription.Dispose ();
			shadow.Dispose ();
			layout.FontDescription.Dispose ();
			layout.Dispose ();
			return sr;
		}
		
		public static void DrawGlowIndicator (Context cr, Gdk.Point location, bool urgent, int numberOfWindows)
		{
			if (DockPreferences.IndicateMultipleWindows && 1 < numberOfWindows) {
				DrawSingleIndicator (cr, location.RelativeMovePoint (3, RelativeMove.RelativeLeft), urgent);
				DrawSingleIndicator (cr, location.RelativeMovePoint (3, RelativeMove.RelativeRight), urgent);
			} else if (0 < numberOfWindows) {
				DrawSingleIndicator (cr, location, urgent);
			}
		}
		
		static void DrawSingleIndicator (Context cr, Gdk.Point location, bool urgent)
		{
			int size = urgent ? 12 : 9;
			
			cr.MoveTo (location.X, location.Y);
			cr.Arc (location.X, location.Y, size, 0, Math.PI * 2);
			
			RadialGradient rg = new RadialGradient (location.X, location.Y, 0, location.X, location.Y, size);
			rg.AddColorStop (0, new Cairo.Color (1, 1, 1, 1));
			if (urgent) {
				rg.AddColorStop (.10, new Cairo.Color (1, .8, .8, 1.0));
				rg.AddColorStop (.20, new Cairo.Color (1, .6, .6, .60));
				rg.AddColorStop (.35, new Cairo.Color (1, .3, .3, .35));
				rg.AddColorStop (.50, new Cairo.Color (1, .3, .3, .25));
				rg.AddColorStop (1.0, new Cairo.Color (1, .3, .3, 0.0));
			} else {
				rg.AddColorStop (.10, new Cairo.Color (.5, .6, 1, 1.0));
				rg.AddColorStop (.20, new Cairo.Color (.5, .6, 1, .60));
				rg.AddColorStop (.25, new Cairo.Color (.5, .6, 1, .25));
				rg.AddColorStop (.50, new Cairo.Color (.5, .6, 1, .15));
				rg.AddColorStop (1.0, new Cairo.Color (.5, .6, 1, 0.0));
			}
			
			cr.Pattern = rg;
			cr.Fill ();
			rg.Destroy ();
		}
	}
}
