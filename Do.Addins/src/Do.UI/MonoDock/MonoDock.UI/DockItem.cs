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
	
	
	public class DockItem : IDockItem, IDoDockItem
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
		Surface sr, icon_surface;
		
		public string Icon { get { return item.Icon; } }
		public string Description { get { return item.Name; } }
		public IObject IObject { get { return item; } }
		
		public DateTime LastClick { get; set; }
		public DateTime DockAddItem { get; set; }
		
		public int Width { get { return IconSize; } }
		public int Height { get { return IconSize; } }
		public bool Scalable { get { return true; } }
		
		Gdk.Pixbuf pixbuf;
		Gdk.Pixbuf Pixbuf {
			get {
				return pixbuf ?? pixbuf = GetPixbuf ();
			}
		}
		
		public DockItem(IObject item)
		{
			LastClick = DateTime.UtcNow - new TimeSpan (0, 10, 0);
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
		
		public Surface GetIconSurface ()
		{
			if (icon_surface == null) {
				icon_surface = new ImageSurface (Cairo.Format.Argb32, (int) (IconSize*IconQuality), (int) (IconSize*IconQuality));
				Context cr = new Context (icon_surface);
				Gdk.CairoHelper.SetSourcePixbuf (cr, Pixbuf, 0, 0);
				cr.Paint ();
				
				(cr as IDisposable).Dispose ();
				pixbuf.Dispose ();
				pixbuf = null;
			}
			return icon_surface;
		}
		
		public Surface GetTextSurface ()
		{
			if (sr == null)
				sr = Util.GetBorderedTextSurface (item.Name, TextWidth);
			return sr;
		}
		
		public void Clicked (uint button)
		{
			
		}
		
		public bool Equals (IDockItem other)
		{
			if (other is DockItem)
				return ((other as DockItem).IObject == IObject);
			return false;
		}

		#region IDisposable implementation 
		
		public void Dispose ()
		{
			if (sr != null)
				sr.Destroy ();
			
			if (icon_surface != null)
				icon_surface.Destroy ();
			
			if (pixbuf != null)
				pixbuf.Dispose ();
		}
		
		#endregion 
		
	}
}
