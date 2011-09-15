/* ItemSource.cs
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
using System.Linq;
using System.Collections.Generic;

using Do.Universe.Safe;

namespace Do.Universe
{
	/// <summary>
	/// A source of Items.
	/// Example: A "EpiphanyBookmarkItemSource" could provide Items representing
	/// Epiphany web browser bookmarks.
	/// </summary>
	public abstract class ItemSource : Item, IChildItemSource
	{

		static SafeItemSource safe_item_source = new SafeItemSource ();

		/// <value>
		/// Quick access to a safe equivalent of the reciever.
		/// </value>
		/// <remarks>
		/// The caller DOES NOT have exclusive access to the value
		/// returned; DO NOT put the value in a collection, linq statement,
		/// or otherwise retain the value returned. The following is the
		/// sole legitimate use:
		/// <code>
		/// source.Safe.UpdateItems ();
		/// </code>
		/// In words: access the property, but do not retain it.
		/// </value>
		/// </remarks>
		public new SafeItemSource Safe {
			get {
				safe_item_source.ItemSource = this;
				return safe_item_source;
			}
		}

		/// <summary>
		/// Returns a safe equivalent of the reciever. Unlike Safe,
		/// this returns a new safe wrapper instance that the caller has
		/// exclusive access to. You may want to call this in a multi-threaded
		/// context, or if you need a collection of safe instances.
		/// </summary>
		/// <returns>
		/// A <see cref="SafeAct"/>
		/// </returns>
		public new SafeItemSource RetainSafe ()
		{
			return new SafeItemSource (this);
		}

		/// <value>
		/// Item sub-types provided/supported by this source. These include any
		/// types of items provided by the Items property, and the types of items
		/// that this source will provide children for.  Please provide types as
		/// close as possible in ancestry to the static types of items this source
		/// provides/supports (e.g.  FirefoxBookmarkItem instead of Item or
		/// BookmarkItem).
		/// </value>
		public abstract IEnumerable<Type> SupportedItemTypes { get; }

		/// <value>
		/// The Items provided by this source.
		/// null is ok---it signifies that no items are provided.
		/// </value>
		public virtual IEnumerable<Item> Items {
			get { yield break; }
		}
		
		public virtual IEnumerable<Item> ChildrenOfItem (Item item)
		{
			yield break;
		}
		
		/// <summary>
		/// When called, the source should make sure the collection of Items
		/// returned on subsequent accesses of the Items property is up to date.
		/// Example: Re-read bookmarks from the filesystem or check for new email,
		/// etc.
		/// </summary>
		public virtual void UpdateItems ()
		{
		}
	}

}
