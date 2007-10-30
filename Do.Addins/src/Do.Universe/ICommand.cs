/* ICommand.cs
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
	/// An ICommand is the root interface implemented by classes that will be used
	/// as commands.
	/// </summary>
	public interface ICommand : IObject
	{
		/// <value>
		/// An array of IItem sub-interfaces that this command supports.
		/// null is ok---it signifies that NO types are supported.
		/// </value>
		Type[] SupportedItemTypes { get; }
		
		/// <value>
		/// An array of IItem sub-interfaces that this command supports as modifier
		/// items.
		/// null is ok---it signifies that NO types are supported.
		/// </value>
		Type[] SupportedModifierItemTypes { get; }
		
		/// <summary>
		/// Perform this command on the given items with the given modifier items.
		/// </summary>
		/// <param name="items">
		/// A <see cref="IItem[]"/> of primary items.
		/// </param>
		/// <param name="modItems">
		/// A <see cref="IItem[]"/> of modifier items.
		/// </param>
		void Perform (IItem[] items, IItem[] modItems);

		/// <summary>
		/// Indicate whether the command can operate on the given item.  The item is
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
		/// guaranteed to be a subtype of a supported type. If the command class
		/// implementing this interface does not support any modifier item types,
		/// this method will never be called.
		/// </summary>
		/// <param name="items">
		/// A <see cref="IItem[]"/> of IItems that the user is attempting to perform
		/// this command on.
		/// </param>
		/// <param name="modItem">
		/// A <see cref="IItem"/> that will be used to modify the behavior of this
		/// command when performed on the items in the IItem array.
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/> indicating whether this command supports
		/// the particular modifier item for the items in the IItem array.
		/// </returns>
		bool SupportsModifierItemForItems (IItem[] items, IItem modItem);
	}
}
