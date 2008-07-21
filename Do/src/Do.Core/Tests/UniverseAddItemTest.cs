// UniverseAddItemTest.cs
// 
// Copyright (C) 2008 GNOME-Do
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

using Do.Core;
using Do.Addins;
using Do.Universe;

namespace Do
{
	
	
	[TestFixture()]
	public class UniverseAddItemTest
	{
		
		[Test()]
		public void TestCase()
		{
			IUniverseManager universe = new SimpleUniverseManager ();
			IItem[] items = new IItem[1];
			items[0] = new FileItem ("/etc");
			
			universe.AddItems (items);
			
			IObject outItem;
			universe.TryGetObjectForUID (new DoItem(items[0]).UID, out outItem);
			Assert.IsNotNull (outItem, "Failed to retrieve item by UID");
			Assert.IsTrue (outItem.Name.Equals (items[0].Name));
		}
	}
}
