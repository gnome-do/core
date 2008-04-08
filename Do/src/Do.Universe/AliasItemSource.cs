/* AliasItemSource.cs
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

namespace Do.Universe
{	
	class AliasItem : ProxyItem
	{
		public AliasItem (string alias, IItem item) :
			base (alias, item)
		{
		}
	}
	
	public class AliasItemSource : IItemSource
	{
		static List<IItem> items;
		
		static AliasItemSource ()
		{
			items = new List<IItem> ();
		}
		
		public static void Alias (IItem item, string alias)
		{
			AliasItem aliasItem;
			
			aliasItem = new AliasItem (alias, item);
			items.Add (aliasItem);
			
			Do.UniverseManager.AddItems (new IItem [] { aliasItem });
		}
		
		public static void Unalias (IItem item)
		{
			int i = IndexOfAlias (item);
			if (i != -1)
				items.RemoveAt (i);
		}
		
		public static bool ItemHasAlias (IItem item)
		{
			return IndexOfAlias (item) != -1;
		}
		
		static int IndexOfAlias (IItem item)
		{
			int i = 0;
			foreach (AliasItem alias in items) {
				if (alias.Inner.Equals (item))
					return i;
				i++;
			}
			return -1;
		}
		
		public string Name {
			get {
				return "Alias items";
			}
		}

		public string Description {
			get {
				return "Aliased items from Do's universe.";
			}
		}

		public string Icon {
			get {
				return "emblem-symbolic-link";
			}
		}

		public Type [] SupportedItemTypes {
			get {
				return new Type [] {
					typeof (ProxyItem),
				};
			}
		}

		public ICollection<IItem> Items {
			get {
				return items;
			}
		}
		
		public ICollection<IItem> ChildrenOfItem (IItem item)
		{
			return null;
		}

		public void UpdateItems ()
		{
		}
	}
}
