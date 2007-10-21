/* IItemSource.cs
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

	/// <summary>
	/// A source of IItems.
	/// Example: A "EpiphanyBookmarkItemSource" could provide IItems
	/// representing Epiphany web browser bookmarks.
	/// </summary>
	public interface IItemSource : IObject
	{

		/// <value>
		/// IItem sub-types provided/supported by
		/// this source. These include any types
		/// of items provided by the Items property, and the
		/// types of items that this source will provide children for.
		/// Please provide types as close as possible in ancestry to the
		/// static types of items this source provides/supports (e.g.
		/// FirefoxBookmarkItem instead of IItem or BookmarkItem).
		/// </value>
		Type[] SupportedItemTypes { get; }
		
		/// <value>
		/// The IItems provided by this source.
		/// null is ok---it signifies that no items are provided.
		/// </value>
		ICollection<IItem> Items { get; }
		
		/// <summary>
		/// Provides a collection of children of an item. Item is guaranteed
		/// to be a subtype of a type in SupportedItemTypes.
		/// null is ok---it signifies that no children are provided
		/// by this source.
		/// </summary>
		ICollection<IItem> ChildrenOfItem (IItem item);
		
		/// <summary>
		/// When called, the source should make sure the collection
		/// of IItems returned on subsequent accesses of the Items
		/// property is up to date.
		/// Example: Re-read bookmarks from the filesystem or check
		/// for new email, etc.
		/// </summary>
		void UpdateItems ();
	}
}
