/* ItemSource.cs
 *
 * GNOME Do is the legal property of its developers. Please refer to the
 * COPYRIGHT file distributed with this
 * source distribution.
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
using System.Collections.Generic;

using Do.Universe;

namespace Do.Core
{
	
	public class ItemSource : GCObject 
	{
	
		public static readonly string DefaultItemSourceName = "Unnamed Item Source";
		public static readonly string DefaultItemSourceDescription = "No description.";
		public static readonly string DefaultItemSourceIcon = "empty";
		
		private bool enabled;
		protected IItemSource source;
		protected List<Item> items;
		
		public ItemSource (IItemSource source)
		{
			if (source == null) {
				throw new ArgumentNullException ();
			}
			this.source = source;
			items = new List<Item> ();
			foreach (IItem item in source.Items) {
				items.Add (new Item (item));
			}
			enabled = true;
		}
		
		public override string Name {
			get { return (source.Name == null ? DefaultItemSourceName : source.Name); }
		}
		
		public override string Description {
			get { return (source.Description == null ? DefaultItemSourceDescription : source.Description); }
		}
		
		public override string Icon {
			get { return (source.Icon == null ? DefaultItemSourceIcon : source.Icon); }
		}
		
		public void UpdateItems () {
			source.UpdateItems ();
			items.Clear ();
			items = new List<Item> ();
			foreach (IItem item in source.Items) {
				items.Add (new Item (item));
			}
		}
		
		public ICollection<Item> Items {
			get { return items; }
		}
		
		public ICollection<IItem> ChildrenOfItem (IItem item) {
			return new List<IItem> ();
		}
		
		public bool Enabled {
			get { return enabled; }
			set { enabled = value; }
		}
		
		public override string ToString ()
		{
			string items_str = GetType().ToString() + " {";
			foreach (Item item in items) {
				items_str = String.Format ("{0}\t{1}\n", items_str, item);
			}
			items_str += "}";
			return items_str;
		}
		
	}
}
