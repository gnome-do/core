// 
//  TestItemSource.cs
//  
//  Author:
//       Christopher James Halse Rogers <raof@ubuntu.com>
//  
//  Copyright © 2012 Christopher James Halse Rogers <raof@ubuntu.com>
// 
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
// 
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Do.Universe
{
	public class TestItemSource : ItemSource
	{
		List<Item> items = new List<Item> ();
		public List<Item> next_items = new List<Item> ();

		public override IEnumerable<Type> SupportedItemTypes {
			get {
				throw new NotImplementedException ();
			}
		}

		public override IEnumerable<Item> Items {
			get {
				return items;
			}
		}

		public override void UpdateItems ()
		{
			items = next_items;
		}

		public override IEnumerable<Item> ChildrenOfItem (Item item)
		{
			return base.ChildrenOfItem (item);
		}

		public override string Name {
			get {
				return "Test Item Source";
			}
		}

		public override string Description {
			get {
				return "An item source for unit testing";
			}
		}

		public override string Icon {
			get {
				return "unknown";
			}
		}
	}

	[TestFixture()]
	public class TestDynamicItemSourceEmulation
	{
		[Test()]
		public void TestUpdateItemsRaisesItemsAvailable ()
		{
			var source = new TestItemSource ();

			bool itemsAvailableCalled = false;

			source.ItemsAvailable += delegate(object sender, ItemsAvailableEventArgs e) {
				itemsAvailableCalled = true;
			};
			source.ItemsUnavailable += delegate {};

			source.next_items = new List<Item> () { new Common.TextItem ("bar") };

			source.UpdateAndEmit ();
			Assert.IsTrue (itemsAvailableCalled);
		}

		[Test()]
		public void TestUpdateItemsRaisesItemsUnavailable ()
		{
			var source = new TestItemSource ();

			bool itemsUnavailableCalled = false;

			source.ItemsUnavailable += delegate(object sender, ItemsUnavailableEventArgs e) {
				itemsUnavailableCalled = true;
			};
			source.ItemsAvailable += delegate {};

			source.next_items = new List<Item> () { new Common.TextItem ("bar") };

			source.UpdateAndEmit ();

			source.next_items = new List<Item> ();

			source.UpdateAndEmit ();
			Assert.IsTrue (itemsUnavailableCalled);
		}

		[Test()]
		public void TestUpdateItemsAddsCorrectItems ()
		{
			var source = new TestItemSource ();
			var items = new List<Item> ();
			items.Add (new Common.TextItem ("bar"));
			items.Add (new Common.TextItem ("baz"));

			List<Item> items_raised = null;

			source.ItemsAvailable += (object sender, ItemsAvailableEventArgs e) => {
				items_raised = new List<Item> (e.newItems);
			};
			source.ItemsUnavailable += delegate { };

			source.next_items = items;
			source.UpdateAndEmit ();

			CollectionAssert.AreEqual (items, items_raised);
		}

		[Test()]
		public void TestUpdateItemsRemovesCorrectItems ()
		{
			var source = new TestItemSource ();
			var items = new List<Item> ();
			items.Add (new Common.TextItem ("bar"));
			items.Add (new Common.TextItem ("baz"));

			List<Item> items_removed = null;

			source.ItemsAvailable += delegate { };
			source.ItemsUnavailable += delegate(object sender, ItemsUnavailableEventArgs e) {
				items_removed = new List<Item> (e.unavailableItems);
			};

			// Add items to Universe...
			source.next_items = items;
			source.UpdateAndEmit ();

			//...now remove them.
			source.next_items = new List<Item> ();
			source.UpdateAndEmit ();

			CollectionAssert.AreEqual (items, items_removed);
		}

		[Test()]
		public void TestMultipleUpdatesProduceCorrectItems ()
		{
			var source = new TestItemSource ();
			var items = new List<Item> ();
			Item bar = new Common.TextItem ("bar");
			Item baz = new Common.TextItem ("baz");
			items.Add (bar);
			items.Add (baz);

			List<Item> items_removed = null;
			List<Item> items_added = null;

			source.ItemsAvailable += delegate(object sender, ItemsAvailableEventArgs e) {
				items_added = new List<Item> (e.newItems);
			};
			source.ItemsUnavailable += delegate(object sender, ItemsUnavailableEventArgs e) {
				items_removed = new List<Item> (e.unavailableItems);
			};

			// Add items to Universe...
			source.next_items = items;
			source.UpdateAndEmit ();

			CollectionAssert.AreEqual (items, items_added);
			Assert.AreEqual (null, items_removed);

			items_added = null;
			items_removed = null;

			// Nothing added, nothing should change
			source.UpdateAndEmit ();

			Assert.AreEqual (null, items_added);
			Assert.AreEqual (null, items_removed);

			//...now remove something...
			items.Remove (bar);

			source.next_items = items;
			source.UpdateAndEmit ();

			Assert.AreEqual (null, items_added);
			CollectionAssert.AreEqual (new Item[] {bar}, items_removed);

			items_added = null;
			items_removed = null;

			//...finally, add something back.
			items.Add (bar);

			source.next_items = items;
			source.UpdateAndEmit ();

			CollectionAssert.AreEqual (new Item[] {bar}, items_added);
			Assert.AreEqual (null, items_removed);
		}

		[Test]
		public void TestConnectingSignalsFiresUpdates ()
		{
			var source = new TestItemSource ();
			var items = new List<Item> ();
			items.Add (new Common.TextItem ("bar"));
			items.Add (new Common.TextItem ("baz"));

			List<Item> items_added = null;

			// Ensure the source will have the items before UpdateItems is first called
			source.next_items = items;

			source.ItemsAvailable += delegate(object sender, ItemsAvailableEventArgs e) {
				items_added = new List<Item> (e.newItems);
			};
			source.ItemsUnavailable += delegate { };

			CollectionAssert.AreEqual (items, items_added);
		}
	}
}

