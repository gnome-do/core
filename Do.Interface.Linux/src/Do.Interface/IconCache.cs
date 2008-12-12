/* IconCache.cs
 *
 * GNOME Do is the legal property of its developers. Please refer to the
 * COPYRIGHT file distributed with this source distribution.
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
 
using System;
using System.Collections;
using System.Collections.Generic;

namespace Do.Interface
{
	public class IconCache
	{
		const int MaxSize = 50;
		
		protected Dictionary<string, Entry> cache;
		
		public IconCache ()
		{			
			cache = new Dictionary<string, Entry> ();
		}
		
		public bool TryGetValue (string key, out Gdk.Pixbuf p)
		{
			Entry e = null;
			
			p = null;
			if (cache.TryGetValue (key, out e))
				p = e.Icon;
			return null != e;
		}
		
		public void Clear ()
		{
			cache.Clear ();
		}
		
		public Gdk.Pixbuf this [string key] {
			get {
				Entry e;
				
				if (cache.TryGetValue (key, out e)) {
					e.Time = DateTime.Now;
					return e.Icon;
				} else {
					return null;
				}
			}
			set {
				if (cache.ContainsKey (key)) {
					cache [key].Time = DateTime.Now;
				} else {	
					if (cache.Count > MaxSize) EvictLRU ();
					cache [key] = new Entry (value);
				}
			}
		}
		
		protected void EvictLRU ()
		{
			string name = null;
			Entry least = null;
			
			foreach (KeyValuePair<string, Entry> e in cache) {
				if (least == null || e.Value.Time < least.Time) {
					name = e.Key;
					least = e.Value;
				}
			}
			if (null != name) {
				cache.Remove (name);
			}
		}
		
		protected class Entry
		{
			public Gdk.Pixbuf Icon;
			public DateTime Time;
			
			
			public Entry (Gdk.Pixbuf p)
			{
				Icon = p;
				Time = DateTime.Now;
			}
		}
	}
}
