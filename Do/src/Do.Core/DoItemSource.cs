/* DoItemSource.cs
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
	public class DoItemSource : DoObject, IItem
	{
		private bool enabled;
		
		public DoItemSource (IItemSource source):
			base (source)
		{
			enabled = true;
		}
		
		public void UpdateItems ()
		{
			try {
				(Inner as IItemSource).UpdateItems ();
			} catch (Exception e) {
				Log.Error ("Item source \"{0}\" encountered an error while " +
						"updating items: {1}", Inner.Name, e.Message);
			}
		}
		
		public ICollection<IItem> Items
		{
			get {
				IItemSource source = Inner as IItemSource;
				List<IItem> items;
				
				items = new List<IItem> ();
				if (source.Items != null) {
					foreach (IItem item in source.Items) {
						if (item is DoItem)
							items.Add (item);
						else
							items.Add (new DoItem (item));
					}
				}
				return items;
			}
		}
		
		public ICollection<IItem> ChildrenOfItem (IItem item)
		{
			IItemSource source = Inner as IItemSource;
			ICollection<IItem> children = null;
			List<IItem> doChildren;
			
			doChildren = new List<IItem> ();
			item = EnsureIItem (item);
			try {
				children = source.ChildrenOfItem (item);
			} catch {
				children = null;
			} finally {
				children = children ?? new IItem[0];
			}
			
			foreach (IItem child in children) {
				if (child is DoItem)
					doChildren.Add (child);
				else
					doChildren.Add (new DoItem (child));
			}
			return doChildren;
		}
		
		public bool Enabled
		{
			get { return enabled; }
			set { enabled = value; }
		}
	}
}
