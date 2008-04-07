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

using Do.Universe;

namespace Do
{	
	public class AliasItemSource : IItemSource
	{
		static List<IItem> items;
		
		static AliasItemSource ()
		{
			items = new List<IItem> ();
		}
		
		public static void AliasItem (IItem item, string alias)
		{
			ProxyItem proxy;
			
			proxy = new ProxyItem (alias, item);
			items.Add (proxy);
			//Do.UniverseManager.Reload ();
			Do.UniverseManager.AddItem (proxy);
		}
		
		public static void Unalias (ProxyItem proxy)
		{
			items.Remove (proxy);
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
