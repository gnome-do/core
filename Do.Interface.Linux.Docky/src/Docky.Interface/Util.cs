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

using Do.Universe;
using Do.Interface.CairoUtils;

using Cairo;
using Gdk;

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
		public static Surface GetBorderedTextSurface (string text, int max_width, Surface similar)
		{
			Surface sr;
			sr = similar.CreateSimilar (similar.Content, max_width, 20);
			
			Context cr = new Context (sr);
			
			Pango.Layout layout = Pango.CairoHelper.CreateLayout (cr);
			layout.FontDescription = Pango.FontDescription.FromString ("sans-serif bold");
			layout.Width = Pango.Units.FromPixels (max_width);
			layout.SetMarkup ("<b>" + text + "</b>");
			layout.Alignment = Pango.Alignment.Center;
			layout.Ellipsize = Pango.EllipsizeMode.End;
			
			Pango.Rectangle rect1, rect2;
			layout.GetExtents (out rect1, out rect2);
			
			cr.SetRoundedRectanglePath (Pango.Units.ToPixels (rect2.X) - 10, 0, Pango.Units.ToPixels (rect2.Width) + 20, 20, 10);
			cr.Color = new Cairo.Color (0, 0, 0, .6);
			cr.Fill ();
			
			Pango.CairoHelper.LayoutPath (cr, layout);
			cr.Color = new Cairo.Color (1, 1, 1);
			cr.Fill ();
			
			(cr as IDisposable).Dispose ();
			layout.FontDescription.Dispose ();
			layout.Dispose ();
			return sr;
		}
		
		public static void DrawGlowIndicator (Context cr, int x, int y, bool urgent)
		{
			int size = urgent ? 12 : 9;
			cr.MoveTo (x, y);
			cr.Arc (x, y, size, 0, Math.PI * 2);
			
			RadialGradient rg = new RadialGradient (x, y, 0, x, y, size);
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
