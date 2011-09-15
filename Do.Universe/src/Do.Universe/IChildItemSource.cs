// 
//  IChildItemSource.cs
//  
//  Author:
//       Christopher James Halse Rogers <raof@ubuntu.com>
// 
//  Copyright © 2011 Christopher James Halse Rogers <raof@ubuntu.com>
// 
//  This library is free software; you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as
//  published by the Free Software Foundation; either version 2.1 of the
//  License, or (at your option) any later version.
// 
//  This library is distributed in the hope that it will be useful, but
//  WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//  Lesser General Public License for more details.
// 
//  You should have received a copy of the GNU Lesser General Public
//  License along with this library; if not, write to the Free Software
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA

using System;
using System.Collections.Generic;

namespace Do.Universe
{
	/// <summary>
	/// An interface for providing child items.
	/// <remarks>
	/// An <see cref="ItemSource"/> or <see cref="DynamicItemSource"/> will implement
	/// this to associate the source with the Items it provides, but this can also
	/// be used to associate Items that are not provided by the same source.
	/// For example, the Banshee plugin uses this to associate the albums and artists
	/// it provides with the <see cref="ApplicationItem"/> corresponding to Banshee, provided by
	/// the <see cref="ApplicationItemSource"/>.
	/// </remarks>
	/// </summary>
	public interface IChildItemSource
	{
		/// <summary>
		/// Provides a collection of children of an item. Item is guaranteed to be a
		/// subtype of a type in SupportedItemTypes.
		/// An empty collection is ok---it signifies that no children are
		/// provided for the Item argument.
		/// </summary>
		IEnumerable<Item> ChildrenOfItem (Item item);

		/// <value>
		/// Item sub-types provided/supported by this source. These include any
		/// types of items provided by the Items property, and the types of items
		/// that this source will provide children for.  Please provide types as
		/// close as possible in ancestry to the static types of items this source
		/// provides/supports (e.g.  FirefoxBookmarkItem instead of Item or
		/// BookmarkItem).
		/// </value>
		IEnumerable<Type> SupportedItemTypes { get; }
	}
}

