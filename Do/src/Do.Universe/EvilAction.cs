/* EvilAction.cs
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
using System.Collections.Generic;

namespace Do.Universe
{	

	class EvilAction : Act
	{

		protected void DoEvil ()
		{
			throw new Exception ("Muahahahaha!");
		}

		public override string Name {
			get { DoEvil (); return null; }
		}

		public override string Description {
			get { DoEvil (); return null; }
		}

		public override string Icon {
			get { DoEvil (); return null; }
		}

		public override IEnumerable<Type> SupportedItemTypes {
			get {
				DoEvil ();
				return null;
			}
		}
		
		public override IEnumerable<Type> SupportedModifierItemTypes {
			get {
				DoEvil ();
				return null;
			}
		}
		
		public override bool ModifierItemsOptional {
			get {
				DoEvil ();
				return false;
			}
		}
		
		public override bool SupportsItem (Item item)
		{
			DoEvil ();
			return true;
		}
		
		public override bool SupportsModifierItemForItems (IEnumerable<Item> items, Item modItem)
		{
			DoEvil ();
			return true;
		}

		public override IEnumerable<Item> DynamicModifierItemsForItem (Item item)
		{
			DoEvil ();
			return null;
		}

		public override IEnumerable<Item> Perform (IEnumerable<Item> items, IEnumerable<Item> modItems)
		{
			DoEvil ();
			return null;
		}

	}
}
