/* IAction.cs
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

namespace Do.Universe
{	
	/// <summary>
	/// An IAction is the root interface implemented by classes that will be used
	/// as actions.
	/// </summary>
	public interface IAction : IObject
	{
		/// <value>
		/// An array of IItem sub-interfaces that this action supports.
		/// null is ok--it signifies that NO types are supported.
		/// </value>
		Type[] SupportedItemTypes { get; }
		
		/// <value>
		/// An array of IItem sub-interfaces that this action supports as modifier
		/// items. If your action uses modifier items, either optionally or necessarily,
		/// either from the item universe or provided dynamically with DynamicModifierItemsForItem,
		/// this must return a non-empty array; if you return null or an empty array
		/// here, your action will not work with modifier items.
		/// null is ok--it signifies that NO types are supported.
		/// </value>
		Type[] SupportedModifierItemTypes { get; }
		
		/// <value>
		/// Whether modifier items are optional (if you indicate that modifier
		/// items are /not/ optional, this means that they are required).
		/// </value>
		bool ModifierItemsOptional { get; }
		
		/// <summary>
		/// Perform this action on the given items with the given modifier items.
		/// </summary>
		/// <param name="items">
		/// A <see cref="IItem[]"/> of primary items.
		/// </param>
		/// <param name="modItems">
		/// A <see cref="IItem[]"/> of modifier items.
		/// </param>
		/// <returns>
		/// A <see cref="IItem[]"/> of result items to present to the user.
		/// You may return null--this is the same as returning an empty array.
		/// </returns>
		IItem[] Perform (IItem[] items, IItem[] modItems);

		/// <summary>
		/// Indicate whether the action can operate on the given item.  The item is
		/// guaranteed to be a subtype of some supported type.
		/// </summary>
		/// <param name="item">
		/// A <see cref="IItem"/> to test for supportability.
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/> indicating whether or not to accept the
		/// item.
		/// </returns>
		bool SupportsItem (IItem item);
		
		/// <summary>
		/// Similar to SupportsItem, but uses an array of IItems as a context for
		/// determining whether or not a modifier item is supported. IItems will
		/// have already been accepted by SupportsItem. The modifier IItem is
		/// guaranteed to be a subtype of a supported type. If the action class
		/// implementing this interface does not support any modifier item types,
		/// this method will never be called.
		/// </summary>
		/// <param name="items">
		/// A <see cref="IItem[]"/> of IItems that the user is attempting to perform
		/// this action on.
		/// </param>
		/// <param name="modItem">
		/// A <see cref="IItem"/> that will be used to modify the behavior of this
		/// action when performed on the items in the IItem array.
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/> indicating whether this action supports
		/// the particular modifier item for the items in the IItem array.
		/// </returns>
		bool SupportsModifierItemForItems (IItem[] items, IItem modItem);
		
		/// <summary>
		/// If you would like to supply modifier items dynamically, do
		/// so here.
		/// </summary>
		/// <param name="item">
		/// An <see cref="IItem[]"/> for which you will or will not provide
		/// dynamic modifier items.
		/// </param>
		/// <returns>
		/// An <see cref="IItem[]"/> containing items to use as modifier items.
		/// null is ok--it signifies that no modifier items are provided.
		/// </returns>
		IItem[] DynamicModifierItemsForItem (IItem item);
	}
	
	/// <summary>
	/// This class is for your convenience. It stubs some less-frequently
	/// implemented IAction members with default values.
	/// </summary>
	public abstract class AbstractAction : Pluggable, IAction
	{
		
		public abstract string Name { get; }
		public abstract string Icon { get; }
		public abstract string Description { get; }
		
		public abstract Type[] SupportedItemTypes { get; }
		
		public virtual Type[] SupportedModifierItemTypes
		{
			get { return null; }
		}
		
		public virtual bool ModifierItemsOptional
		{
			get { return false; }
		}

		public virtual bool SupportsItem (IItem item)
		{
			return true;
		}

		public virtual bool SupportsModifierItemForItems (IItem[] items, IItem modItem)
		{
			return true;
		}
		
		public virtual IItem[] DynamicModifierItemsForItem (IItem item)
		{
			return null;
		}
		
		public abstract IItem[] Perform (IItem[] items, IItem[] modItems);
		

	}
}
