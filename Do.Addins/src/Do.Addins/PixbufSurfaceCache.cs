// PixbufSurfaceCache.cs
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
using System.Linq;
using System.Collections.Generic;

using Cairo;
using Gdk;
using Gtk;

using Do.Platform;

namespace Do.Addins
{
	
	
	public class PixbufSurfaceCache : IDisposable
	{
		Dictionary <string, Entry> surface_cache;
		int surface_width;
		int surface_height;
		
		public PixbufSurfaceCache(int count, int surface_width, int surface_height, Surface sourceSurface)
		{
			this.surface_width = surface_width;
			this.surface_height = surface_height;
			surface_cache = new Dictionary<string, Entry> ();
			Entry e;
			for (int i=0; i<count; i++) {
				e = new Entry (sourceSurface.CreateSimilar (sourceSurface.Content, surface_width, surface_height), "null"+i);
				surface_cache.Add (e.ID, e);
			}
		}
		
		public PixbufSurfaceCache(int count, int surface_width, int surface_height)
		{
			this.surface_width = surface_width;
			this.surface_height = surface_height;
			surface_cache = new Dictionary<string, Entry> ();
			Entry e;
			for (int i=0; i<count; i++) {
				e = new Entry (new ImageSurface (Format.Argb32, surface_width, surface_height), "null"+i);
				surface_cache.Add (e.ID, e);
			}
		}
		
		~PixbufSurfaceCache ()
		{
			foreach (Entry e in surface_cache.Values)
				e.surface.Destroy ();
		}
		
		public Surface AddPixbufSurface (string id, string icon)
		{
			Surface sr = EvictLRU ();
			Context cr = new Context (sr);

			cr.Operator = Operator.Source;
			cr.Color = new Cairo.Color (0, 0, 0, 0);
			cr.Paint ();
			
			DrawIconOnSurface (sr, icon);
			
			surface_cache.Add (id, new Entry (sr, id));
			(cr as IDisposable).Dispose ();
			return sr;
		}
		
		private void DrawIconOnSurface (Surface sr, string icon)
		{
			Context cr = new Context (sr);
			Gdk.Pixbuf pixbuf;
			pixbuf = IconProvider.PixbufFromIconName (icon, surface_width);
			if (pixbuf.Height != surface_width && pixbuf.Width != surface_width) {
				double scale = (double)surface_width / Math.Max (pixbuf.Width, pixbuf.Height);
				Gdk.Pixbuf temp = pixbuf.ScaleSimple ((int) (pixbuf.Width * scale), (int) (pixbuf.Height * scale), InterpType.Bilinear);
				pixbuf.Dispose ();
				pixbuf = temp;
			}
			
			if (pixbuf == null) {
				(cr as IDisposable).Dispose ();
				return;
			}
			
			Gdk.CairoHelper.SetSourcePixbuf (cr, 
			                                 pixbuf, 
			                                 (int) ((surface_width - pixbuf.Width)/2), 
			                                 (int) ((surface_height - pixbuf.Height)/2));
			cr.Operator = Operator.Over;
			cr.Paint ();
			pixbuf.Dispose ();
			(cr as IDisposable).Dispose ();
		}
		
		public Surface GetSurface (string  id)
		{
			if (!surface_cache.ContainsKey (id))
				return null;
			surface_cache[id].time = DateTime.Now;
			return surface_cache[id].surface;
		}
		
		public bool ContainsKey (string id)
		{
			return surface_cache.ContainsKey (id);
		}
		
		private Surface EvictLRU ()
		{
			Entry lru = surface_cache.Values.Min ();
			surface_cache.Remove (lru.ID);
			return lru.surface;
		}

		#region IDisposable implementation 
		
		public void Dispose ()
		{
			foreach (Entry en in surface_cache.Values)
				en.surface.Destroy ();
		}
		
		#endregion 
		
		
		class Entry : IComparable<Entry> {
			public Surface surface;
			public DateTime time;
			public string ID;
			
			#region IComparable[PixbufSurfaceCache.Entry] implementation 
			public int CompareTo (Entry other)
			{
				return time.CompareTo (other.time);
			}
			#endregion 
			
			public Entry (Surface s, string id)
			{
				ID = id;
				surface = s;
				time = DateTime.Now;
			}
		}
	}
}
