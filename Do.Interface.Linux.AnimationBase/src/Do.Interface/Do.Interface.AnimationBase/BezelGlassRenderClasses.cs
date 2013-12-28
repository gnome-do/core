// BezelGlassRenderClasses.cs
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

using Gdk;
using Gtk;
using Cairo;

using Do.Interface;
using Do.Interface.CairoUtils;
using Do.Universe;
using Do.Platform.Linux;

namespace Do.Interface.AnimationBase
{
	
	public interface IBezelResultItemRenderer
	{
		int Height { get; }
		void RenderItem (Context cr, Gdk.Point renderAnchor, int width, Do.Universe.Item item, bool drawArrow);
	}
	
	public class BezelFullResultItemRenderer : IBezelResultItemRenderer
	{
		BezelGlassResults parent;
		
		public int Height { get { return 36; } }
		
		public int IconSize { get { return 32; } }

		public BezelFullResultItemRenderer (BezelGlassResults parent)
		{
			this.parent = parent;
		}
		
		public void RenderItem (Context cr, Gdk.Point renderAnchor, int width, Do.Universe.Item item, bool drawArrow)
		{
			cr.Rectangle (renderAnchor.X, renderAnchor.Y, width, Height);
			cr.SetSourceRGBA (0, 0, 0, 0);
			cr.Operator = Operator.Source;
			cr.Fill ();
			cr.Operator = Operator.Over;
			
			Gdk.Pixbuf pixbuf = IconProvider.PixbufFromIconName (item.Icon, IconSize);
			Gdk.CairoHelper.SetSourcePixbuf (cr, pixbuf, 2, 2);
			cr.Paint ();
			
			pixbuf.Dispose ();
			
			foreach (int i in parent.Secondary) {
				if (parent.Results[i] == item) {
					pixbuf = IconProvider.PixbufFromIconName ("gtk-add", IconSize);
					Gdk.CairoHelper.SetSourcePixbuf (cr, pixbuf, 2, 2);
					cr.PaintWithAlpha (.7);
					pixbuf.Dispose ();
				}
			}
				
			Pango.Layout layout = new Pango.Layout (parent.PangoContext);
			layout.Width = Pango.Units.FromPixels (width - IconSize - 25);
			layout.Ellipsize = Pango.EllipsizeMode.End;
			layout.SetMarkup ("<span foreground=\"#" + parent.ItemTextColor + "\">"+GLib.Markup.EscapeText (item.Name)+"</span>");
			layout.FontDescription = Pango.FontDescription.FromString ("normal bold");
			layout.FontDescription.AbsoluteSize = Pango.Units.FromPixels (10);
				
			cr.MoveTo (IconSize + 6, 4);
			Pango.CairoHelper.ShowLayout (cr, layout);
			
			layout.SetMarkup ("<span foreground=\"#" + parent.ItemTextColor + "\">"+GLib.Markup.EscapeText (item.Description)+"</span>");
			layout.FontDescription.Dispose ();
			layout.FontDescription = Pango.FontDescription.FromString ("normal");
			layout.FontDescription.AbsoluteSize = Pango.Units.FromPixels (10);
			cr.MoveTo (IconSize + 8, 19);
			Pango.CairoHelper.ShowLayout (cr, layout);
			
			if (drawArrow) {
				cr.MoveTo (width - 15, 13);
				cr.LineTo (width - 15+7, 17);
				cr.LineTo (width - 15, 21);
				Gdk.Color gc = new Gdk.Color ();
				Gdk.Color.Parse ("#" + parent.ItemTextColor, ref gc);
				cr.SetSourceRGB(gc);
				cr.Fill ();
			}
			
			
			layout.FontDescription.Dispose ();
			layout.Dispose ();
			(cr as IDisposable).Dispose ();
		}
	}
	
	public class BezelHalfResultItemRenderer : IBezelResultItemRenderer
	{
		BezelGlassResults parent;
		
		public int Height { get { return 20; } }
		
		public int IconSize { get { return 16; } }

		public BezelHalfResultItemRenderer (BezelGlassResults parent)
		{
			this.parent = parent;
		}
		
		public void RenderItem (Context cr, Gdk.Point renderAnchor, int width, Do.Universe.Item item, bool drawArrow)
		{
			cr.Rectangle (renderAnchor.X, renderAnchor.Y, width, Height);
			cr.SetSourceRGBA (0, 0, 0, 0);
			cr.Operator = Operator.Source;
			cr.Fill ();
			cr.Operator = Operator.Over;
			
			Gdk.Pixbuf pixbuf = IconProvider.PixbufFromIconName (item.Icon, IconSize);
			Gdk.CairoHelper.SetSourcePixbuf (cr, pixbuf, 2, 2);
			cr.Paint ();
			
			pixbuf.Dispose ();
			
			foreach (int i in parent.Secondary) {
				if (parent.Results[i] == item) {
					pixbuf = IconProvider.PixbufFromIconName ("gtk-add", IconSize);
					Gdk.CairoHelper.SetSourcePixbuf (cr, pixbuf, 2, 2);
					cr.PaintWithAlpha (.7);
					pixbuf.Dispose ();
				}
			}
				
			Pango.Layout layout = new Pango.Layout (parent.PangoContext);
			layout.Width = Pango.Units.FromPixels (width - IconSize - 25);
			layout.Ellipsize = Pango.EllipsizeMode.End;
			layout.SetMarkup ("<span foreground=\"#" + parent.ItemTextColor + "\">"+GLib.Markup.EscapeText (item.Name)+"</span>");
			layout.FontDescription = Pango.FontDescription.FromString ("normal bold");
			layout.FontDescription.AbsoluteSize = Pango.Units.FromPixels (10);
				
			cr.MoveTo (IconSize + 6, 4);
			Pango.CairoHelper.ShowLayout (cr, layout);
			
			if (drawArrow) {
				cr.MoveTo (width - IconSize+6, 5);
				cr.LineTo (width - IconSize+10, 10);
				cr.LineTo (width - IconSize+6, 15);
				
				cr.MoveTo (width - IconSize+2, 5);
				cr.LineTo (width - IconSize+6, 10);
				cr.LineTo (width - IconSize+2, 15);
				Gdk.Color gc = new Gdk.Color ();
				Gdk.Color.Parse ("#" + parent.ItemTextColor, ref gc);
				cr.SetSourceRGB(gc);
				cr.Stroke ();
			}
			
			layout.FontDescription.Dispose ();
			layout.Dispose ();
			(cr as IDisposable).Dispose ();
		}
	}
}
