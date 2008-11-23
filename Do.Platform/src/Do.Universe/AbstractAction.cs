/* AbstractAction.cs
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
	/// <summary>
	/// This class is for your convenience. It stubs some less-frequently
	/// implemented IAction members with default values.
	/// </summary>
	public abstract class AbstractAction : IAction
	{
		
		public abstract string Name { get; }
		public abstract string Icon { get; }
		public abstract string Description { get; }
		
		public abstract IEnumerable<Type> SupportedItemTypes { get; }
		
		public virtual IEnumerable<Type> SupportedModifierItemTypes {
			get { return Type.EmptyTypes; }
		}
		
		public virtual bool ModifierItemsOptional
		{
			get { return false; }
		}

		public virtual bool SupportsItem (IItem item)
		{
			return true;
		}

		public virtual bool SupportsModifierItemForItems (IEnumerable<IItem> items, IItem modItem)
		{
			return true;
		}
		
		public virtual IEnumerable<IItem> DynamicModifierItemsForItem (IItem item)
		{
			return null;
		}
		
		public abstract IEnumerable<IItem> Perform (IEnumerable<IItem> items, IEnumerable<IItem> modItems);
		

	}
}
