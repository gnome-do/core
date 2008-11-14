// DockItem.cs
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

using Gdk;
using Cairo;

using Do.UI;
using Do.Universe;
using Do.Addins.CairoUtils;

namespace MonoDock.UI
{
	
	
	public class DockItem
	{
#region Static Area
		static int icon_size = 64;
		static double icon_quality = 2;
		
		public static int IconSize {
			get { return icon_size; }
			set { icon_size = value; }
		}
		
		public static double IconQuality {
			get { return icon_quality; }
			set { icon_quality = value; }
		}
		
		public static int TextWidth {
			get { return 350; }
		}
#endregion
		
		IObject item;
		Surface sr;
		
		public string Icon { get { return item.Icon; } }
		public string Description { get { return item.Name; } }
		public IObject IObject { get { return item; } }
		
		public DateTime LastClick { get; set; }
		
		Gdk.Pixbuf pixbuf;
		public Gdk.Pixbuf Pixbuf {
			get {
				return pixbuf ?? pixbuf = GetPixbuf ();
			}
		}
		
		public DockItem(IObject item)
		{
			LastClick = DateTime.Now - new TimeSpan (0, 10, 0);
			this.item = item;
		}
		
		Gdk.Pixbuf GetPixbuf ()
		{
			Gdk.Pixbuf pbuf = IconProvider.PixbufFromIconName (Icon, (int) (IconSize*IconQuality));
			
			if (pbuf.Height != IconSize*IconQuality && pbuf.Width != IconSize*IconQuality) {
				double scale = (double)IconSize*IconQuality / Math.Max (pbuf.Width, pbuf.Height);
				Gdk.Pixbuf temp = pbuf.ScaleSimple ((int) (pbuf.Width * scale), (int) (pbuf.Height * scale), InterpType.Bilinear);
				pbuf.Dispose ();
				pbuf = temp;
			}
			
			return pbuf;
		}
		
		public Surface GetTextSurface ()
		{
			if (sr == null) {
				sr = new Cairo.ImageSurface (Cairo.Format.Argb32, TextWidth, 20);
				
				Context cr = new Context (sr);
				
				Pango.Layout layout = Pango.CairoHelper.CreateLayout (cr);
				layout.Width = Pango.Units.FromPixels (TextWidth);
				layout.SetMarkup ("<b>" + item.Name + "</b>");
				layout.Alignment = Pango.Alignment.Center;
				
				Pango.Rectangle rect1, rect2;
				layout.GetExtents (out rect1, out rect2);
				
				cr.SetRoundedRectanglePath (Pango.Units.ToPixels (rect2.X) - 10, 0, Pango.Units.ToPixels (rect2.Width) + 18, 20, 6);
				cr.Color = new Cairo.Color (0, 0, 0, .7);
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
			}
			return sr;
		}
	}
}
