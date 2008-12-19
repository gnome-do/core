/* DictionaryPreferencesServiceTests.cs
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
using NUnit.Framework;

namespace Do.Platform.Common
{
	
	
	[TestFixture()]
	public class DictionaryPreferencesServiceTests
	{

		DictionaryPreferencesService Service { get; set; }

		[SetUp]
		public void SetUp ()
		{
			Service = new DictionaryPreferencesService ();
		}

		[TearDown]
		public void TearDown ()
		{
		}

		[Test()]
		public void Set_bool ()
		{
			string key = "testbool";
			bool val_in = true;
			
			Assert.IsTrue (Service.Set (key, val_in));
		}

		[Test()]
		public void TryGet_bool ()
		{
			string key = "testbool";
			bool val_in = true, val_out;
			
			Service.Set (key, val_in);
			Assert.IsTrue (Service.TryGet (key, out val_out));
			Assert.AreEqual (val_in, val_out);
		}
		
		[Test()]
		public void Set_int ()
		{
			string key = "testint";
			int val_in = 42;
			
			Assert.IsTrue (Service.Set (key, val_in));
		}

		[Test()]
		public void TryGet_int ()
		{
			string key = "testint";
			int val_in = 42, val_out;
			
			Service.Set (key, val_in);
			Assert.IsTrue (Service.TryGet (key, out val_out));
			Assert.AreEqual (val_in, val_out);
		}

		[Test()]
		public void Set_string ()
		{
			string key = "teststring";
			string val_in = "hello";
			
			Assert.IsTrue (Service.Set (key, val_in));
		}

		[Test()]
		public void TryGet_string ()
		{
			string key = "teststring";
			string val_in = "hello", val_out;
			
			Service.Set (key, val_in);
			Assert.IsTrue (Service.TryGet (key, out val_out));
			Assert.AreEqual (val_in, val_out);
		}

		[Test()]
		public void TryGetDoesNotExist ()
		{
			string key = "xcnd45w3d8bvweqmgf7rbx";
			string val = "";

			Assert.IsFalse (Service.TryGet (key, out val));
		}
	}
}
