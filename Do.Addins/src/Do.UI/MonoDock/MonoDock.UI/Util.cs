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

using Cairo;
using Do.Addins.CairoUtils;
using Gdk;

namespace MonoDock.UI
{
	
	
	public static class Util
	{
		
		public static Surface GetBorderedTextSurface (string text, int max_width)
		{
			Surface sr;
			sr = new Cairo.ImageSurface (Cairo.Format.Argb32, max_width, 20);
			
			Context cr = new Context (sr);
			
			Pango.Layout layout = Pango.CairoHelper.CreateLayout (cr);
			layout.Width = Pango.Units.FromPixels (max_width);
			layout.SetMarkup ("<b>" + text + "</b>");
			layout.Alignment = Pango.Alignment.Center;
			layout.Ellipsize = Pango.EllipsizeMode.End;
			
			Pango.Rectangle rect1, rect2;
			layout.GetExtents (out rect1, out rect2);
			
			cr.SetRoundedRectanglePath (Pango.Units.ToPixels (rect2.X) - 10, 0, Pango.Units.ToPixels (rect2.Width) + 16, 20, 10);
			cr.Color = new Cairo.Color (0, 0, 0, .6);
			cr.Fill ();
			
			Pango.CairoHelper.LayoutPath (cr, layout);
			
			cr.Color = new Cairo.Color (0, 0, 0);
			cr.Fill ();
			
			cr.Translate (-2, -1);
			Pango.CairoHelper.LayoutPath (cr, layout);
			cr.Color = new Cairo.Color (1, 1, 1);
			cr.Fill ();
			
			(cr as IDisposable).Dispose ();
			layout.Dispose ();
			return sr;
		}
	}
}
