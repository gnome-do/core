/* AliasAction.cs
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
using System.Linq;
using System.Collections.Generic;
using Mono.Unix;

namespace Do.Universe {
	
	class AliasAction : IAction {
		
		public string Name {
			get {
				return Catalog.GetString ("Assign Alias...");
			}
		}

		public string Description {
			get {
				return Catalog.GetString ("Give an item an alternate name.");
			}
		}

		public string Icon {
			get {
				return "emblem-symbolic-link";
			}
		}

		public IEnumerable<Type> SupportedItemTypes {
			get {
				yield return typeof (IItem);
			}
		}

		public IEnumerable<Type> SupportedModifierItemTypes {
			get {
				yield return typeof (ITextItem);
			}
		}

		public bool ModifierItemsOptional {
			get {
				return false;
			}
		}

		public IEnumerable<IItem> Perform (IEnumerable<IItem> items, IEnumerable<IItem> modItems)
		{
			string alias;
			
			alias = (modItems.First () as ITextItem).Text;
			yield return AliasItemSource.Alias (items.First (), alias);;
		}

		public bool SupportsItem (IItem item)
		{
			return !(item is IProxyItem);
		}

		public bool SupportsModifierItemForItems (IEnumerable<IItem> items, IItem modItem)
		{
			return true;
		}

		public IEnumerable<IItem> DynamicModifierItemsForItem (IItem item)
		{
			return null;
		}
		

	}
}
