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
		static int IndicatorSize = 9;
		static int UrgentIndicatorSize = 12;
		static int Height = 26;
		static Surface indicator, urgent_indicator;
		
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
			sr = similar.CreateSimilar (similar.Content, max_width, Height);
			
			Context cr = new Context (sr);

			Pango.Layout layout = Pango.CairoHelper.CreateLayout (cr);
			layout.FontDescription = Pango.FontDescription.FromString ("sans-serif 11");
			layout.FontDescription.Weight = Pango.Weight.Semibold;
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
			
			cr.SetRoundedRectanglePath (Pango.Units.ToPixels (rect2.X), 1, Pango.Units.ToPixels (rect2.Width) + 18, Height - 2, 5);
			cr.Color = new Cairo.Color (0.1, 0.1, 0.1, .75);
			cr.FillPreserve ();

			cr.Color = new Cairo.Color (1, 1, 1, .8);
			cr.Stroke ();

			Pango.Layout shadow = layout.Copy();
			shadow.Indent = 1;

			cr.Translate (10, (int) ((Height - 18) / 2));
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
			if (urgent) {
				cr.SetSource (GetUrgentIndicator (cr.Target), location.X - UrgentIndicatorSize / 2.0, location.Y - UrgentIndicatorSize / 2.0);
			} else {
				cr.SetSource (GetIndicator (cr.Target), location.X - IndicatorSize / 2.0, location.Y - IndicatorSize / 2.0);
			}

			cr.Paint ();
		}

		static Surface GetIndicator (Surface similar)
		{
			if (indicator == null) {
				indicator = similar.CreateSimilar (similar.Content, IndicatorSize, IndicatorSize);
				Context cr = new Context (indicator);

				double x = IndicatorSize / 2.0;
				double y = x;
				
				cr.MoveTo (x, y);
				cr.Arc (x, y, IndicatorSize, 0, Math.PI * 2);
				
				RadialGradient rg = new RadialGradient (x, y, 0, x, y, IndicatorSize);
				rg.AddColorStop (0, new Cairo.Color (1, 1, 1, 1));
				rg.AddColorStop (.10, new Cairo.Color (.5, .6, 1, 1.0));
				rg.AddColorStop (.20, new Cairo.Color (.5, .6, 1, .60));
				rg.AddColorStop (.25, new Cairo.Color (.5, .6, 1, .25));
				rg.AddColorStop (.50, new Cairo.Color (.5, .6, 1, .15));
				rg.AddColorStop (1.0, new Cairo.Color (.5, .6, 1, 0.0));
				
				cr.Pattern = rg;
				cr.Fill ();
				rg.Destroy ();

				(cr as IDisposable).Dispose ();
			}
			return indicator;
		}

		static Surface GetUrgentIndicator (Surface similar)
		{
			if (urgent_indicator == null) {
				urgent_indicator = similar.CreateSimilar (similar.Content, UrgentIndicatorSize, UrgentIndicatorSize);
				Context cr = new Context (urgent_indicator);

				double x = UrgentIndicatorSize / 2.0;
				double y = x;
				
				cr.MoveTo (x, y);
				cr.Arc (x, y, UrgentIndicatorSize, 0, Math.PI * 2);
				
				RadialGradient rg = new RadialGradient (x, y, 0, x, y, UrgentIndicatorSize);
				rg.AddColorStop (0, new Cairo.Color (1, 1, 1, 1));
				rg.AddColorStop (.10, new Cairo.Color (1, .8, .8, 1.0));
				rg.AddColorStop (.20, new Cairo.Color (1, .6, .6, .60));
				rg.AddColorStop (.35, new Cairo.Color (1, .3, .3, .35));
				rg.AddColorStop (.50, new Cairo.Color (1, .3, .3, .25));
				rg.AddColorStop (1.0, new Cairo.Color (1, .3, .3, 0.0));
				
				cr.Pattern = rg;
				cr.Fill ();
				rg.Destroy ();

				(cr as IDisposable).Dispose ();
			}
			return urgent_indicator;
		}
	}
}
