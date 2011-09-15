// 
//  DynamicItemSource.cs
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
using System.Linq;
using System.Collections.Generic;

namespace Do.Universe
{
	public class ItemsAvailableEventArgs : EventArgs
	{
		public IEnumerable<Item> newItems;
	}

	public class ItemsUnavailableEventArgs : EventArgs
	{
		public IEnumerable<Item> unavailableItems;
	}

	/// <summary>
	/// An item source with items which may appear or disappear at any time.
	/// Unlike the standard ItemSource, this does not get periodically polled.
	/// </summary>
	public abstract class DynamicItemSource : Item, IChildItemSource
	{
		private object event_lock = new object ();
		private bool available_connected = false;
		private bool unavailable_connected = false;

		/// <summary>
		/// This function is called when a listener has connected to the ItemsAvailable and ItemsUnavailable event.
		/// The <typeparamref>DynamicItemSource</typeparamref> MUST NOT raise either event until this has been
		/// called.
		/// </summary>
		protected abstract void Enable ();

		/// <summary>
		/// This function is called when a listener has disconnected to the ItemsAvailable and ItemsUnavailable event.
		/// The <typeparamref>DynamicItemSource</typeparamref> MUST NOT raise either event after this has been
		/// called until a subsequente Enable () call is made.
		/// </summary>
		protected abstract void Disable ();

		protected bool Connected {
			get { return available_connected && unavailable_connected; }
		}

		private EventHandler<ItemsAvailableEventArgs> itemsAvailable;
		/// <summary>
		/// The <typeparamref>DynamicItemSource</typeparamref> raises this event when
		/// new Items are available and should be added to the Universe.
		/// </summary>
		public event EventHandler<ItemsAvailableEventArgs> ItemsAvailable {
			add {
				lock (event_lock) {
					if (available_connected) {
						throw new InvalidOperationException ("Attempt to subscribe to ItemsAvailable while a subscriber already exists");
					}
					itemsAvailable += value;
					available_connected = true;
					if (Connected) {
						Enable ();
					}
				}
			}
			remove {
				lock (event_lock) {
					if (!available_connected) {
						return;
					}
					itemsAvailable -= value;
					if (Connected) {
						Disable ();
					}
					available_connected = false;
				}
			}
		}

		protected void RaiseItemsAvailable (ItemsAvailableEventArgs args)
		{
			EventHandler<ItemsAvailableEventArgs> handler;
			lock (event_lock) {
				if (!Connected) {
					// FIXME: This should really be a Log message rather than an exception
					throw new InvalidOperationException ("Attempted to raise ItemsAvailable without a subscriber connected.");
				}
				handler = itemsAvailable;
			}
			handler (this, args);
		}

		private EventHandler<ItemsUnavailableEventArgs> itemsUnavailable;
		/// <summary>
		/// The <typeparamref>DynamicItemSource</typeparamref> raises this event when
		/// one or more items are no longer available and should be removed from the
		/// Universe.
		/// </summary>
		public event EventHandler<ItemsUnavailableEventArgs> ItemsUnavailable {
			add {
				lock (event_lock) {
					if (unavailable_connected) {
						throw new InvalidOperationException ("Attempt to subscribe to ItemsUnavailable while a subscriber already exists");
					}
					itemsUnavailable += value;
					unavailable_connected = true;
					if (Connected) {
						Enable ();
					}
				}
			}
			remove {
				lock (event_lock) {
					if (!unavailable_connected) {
						return;
					}
					itemsUnavailable -= value;
					if (Connected) {
						Disable ();
					}
					unavailable_connected = false;
				}
			}
		}

		protected void RaiseItemsUnavailable (ItemsUnavailableEventArgs args)
		{
			EventHandler<ItemsUnavailableEventArgs> handler;
			lock (event_lock) {
				if (!Connected) {
					// FIXME: This should really be a Log message rather than an exception
					throw new InvalidOperationException ("Attempted to raise ItemsUnavailable without a subscriber connected.");
				}
				handler = itemsUnavailable;
			}
			handler (this, args);
		}

		/// <value>
		/// Item sub-types provided/supported by this source. These include any
		/// types of items provided through ItemsAvailable, and the types of items
		/// that this source will provide children for.  Please provide types as
		/// close as possible in ancestry to the static types of items this source
		/// provides/supports (e.g.  FirefoxBookmarkItem instead of Item or
		/// BookmarkItem).
		/// </value>
		public abstract IEnumerable<Type> SupportedItemTypes { get; }

		/// <summary>
		/// Provides a collection of children of an item. Item is guaranteed to be a
		/// subtype of a type in SupportedItemTypes.
		/// An empty enumerable is ok---it signifies that no children are provided for
		/// the Item argument.
		/// </summary>
		public virtual IEnumerable<Item> ChildrenOfItem (Item item)
		{
			yield break;
		}

	}
}

