// BezelTextUtils.cs
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
using Gtk;

using Do.Addins;

namespace Do.UI
{
	
	
	public class BezelTextUtils
	{
		public static int TextHeight = 11;
		
		public static void RenderLayoutText (Context cr, string text, int x, int y, int width, Gtk.Widget w)
		{
			Pango.Color color = new Pango.Color ();
			color.Blue = color.Red = color.Green = ushort.MaxValue;
			RenderLayoutText (cr, text, x, y, width, color, Pango.Alignment.Center, Pango.EllipsizeMode.End, w);
		}
		
		public static Gdk.Rectangle RenderLayoutText (Context cr, string text, int x, int y, int width, 
		                       Pango.Color color, Pango.Alignment align, Pango.EllipsizeMode ellipse, Gtk.Widget w)
		{
			if (string.IsNullOrEmpty (text)) return new Gdk.Rectangle ();
	
			Pango.Layout layout = new Pango.Layout (w.PangoContext);
			layout.Width = Pango.Units.FromPixels (width);
			layout.SetMarkup (text);
			
			layout.Ellipsize = ellipse;
				
			layout.FontDescription = Pango.FontDescription.FromString ("normal bold");
			layout.FontDescription.AbsoluteSize = Pango.Units.FromPixels (TextHeight);
			layout.Alignment = align;
			
			if (ellipse == Pango.EllipsizeMode.None)
				layout.Wrap = Pango.WrapMode.WordChar;
			
			text = string.Format ("<span foreground=\"{0}\">{1}</span>", color, text);
			layout.SetMarkup (text);
			
			cr.Rectangle (x, y, width, 155);
			cr.Clip ();
			cr.MoveTo (x, y);
			Pango.CairoHelper.ShowLayout (cr, layout);
			Pango.Rectangle strong, weak;
			layout.GetCursorPos (layout.Lines [layout.LineCount-1].StartIndex + 
			                     layout.Lines [layout.LineCount-1].Length, 
			                     out strong, out weak);
			cr.ResetClip ();
			layout.FontDescription.Dispose ();
			layout.Dispose ();
			return new Gdk.Rectangle (Pango.Units.ToPixels (weak.X) + x,
			                          Pango.Units.ToPixels (weak.Y) + y,
			                          Pango.Units.ToPixels (weak.Width),
			                          Pango.Units.ToPixels (weak.Height));
		}
	}
}
