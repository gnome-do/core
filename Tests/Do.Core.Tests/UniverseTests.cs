// UniverseTests.cs
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

using Mono.Unix;

using Do;

namespace Do.Core
{
	
	[TestFixture ()]
	public class UniverseTests
	{
		
		[Test ()]
		public void UniverseCreationTest ()
		{
			IUniverseManager universe = new SimpleUniverseManager ();
			Assert.IsNotNull (universe, "Universe creation has failed");
			return;
		}
		
//		[Test ()]
		public void UniverseInitializationTest ()
		{
			IUniverseManager universe = new SimpleUniverseManager ();
			universe.Initialize ();
			
			Assert.IsTrue (universe.Search ("e", new Type[] {}).Length > 0);
		}
		
		[TestFixtureSetUp]
		public void SetUp ()
		{
			Catalog.Init ("gnome-do", "/usr/local/share/locale");
			Gtk.Application.Init ();
			Gdk.Threads.Init ();
//			PluginManager.Initialize (); //fixme?
		}
	}
}
