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
using System.Collections.Generic;

using Do.Universe;

namespace Do
{	
	class AliasAction : IAction
	{
		
		public string Name {
			get {
				return "Alias...";
			}
		}

		public string Description {
			get {
				return "Assign an item an alternative name.";
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
					typeof (IItem),
				};
			}
		}

		public Type[] SupportedModifierItemTypes {
			get {
				return new Type [] {
					typeof (ITextItem),
				};
			}
		}

		public bool ModifierItemsOptional {
			get {
				return false;
			}
		}

		public IItem[] Perform (IItem[] items, IItem[] modItems)
		{
			string alias;
			
			alias = (modItems [0] as ITextItem).Text;
			AliasItemSource.AliasItem (items [0], alias);
			return null;
		}

		public bool SupportsItem (IItem item)
		{
			return true;
		}

		public bool SupportsModifierItemForItems (IItem[] items, IItem modItem)
		{
			return true;
		}

		public IItem[] DynamicModifierItemsForItem (IItem item)
		{
			return null;
		}
	}
}
