// DoItemTests.cs
// 
// Copyright (C) 2008 GNOME Do
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using System;
using NUnit.Framework;

using Do.Universe;

namespace Do.Core
{

	
	[TestFixture()]
	public class DoItemTests
	{

		class SimpleItem : IItem
		{
			public string Name { get; set; }
			public string Description { get; set; }
			public string Icon { get; set; }
		}
		
		[Test()]
		public void EnsureIItem_Identity ()
		{
			IItem item = new SimpleItem ();
			Assert.AreSame (item, DoItem.EnsureIItem (item));
		}

		[Test()]
		public void EnsureIItem_Basic ()
		{
			IItem item = new SimpleItem ();
			IItem doItem = new DoItem (item);
			Assert.AreSame (item, DoItem.EnsureIItem (doItem));
		}

		[Test()]
		public void EnsureIItem_Recursive ()
		{
			IItem item = new SimpleItem ();
			IItem doItem = new DoItem (new DoItem (item));
			Assert.AreSame (item, DoItem.EnsureIItem (doItem));
		}

		[Test()]
		public void EnsureDoItem_Identity ()
		{
			IItem doItem = new DoItem (new SimpleItem ());
			Assert.AreSame (doItem, DoItem.EnsureDoItem (doItem));
		}

		[Test()]
		public void EnsureDoItem_Basic ()
		{
			IItem item = new SimpleItem ();
			Assert.AreEqual (typeof (DoItem), DoItem.EnsureDoItem (item).GetType ());
		}
	}
}
