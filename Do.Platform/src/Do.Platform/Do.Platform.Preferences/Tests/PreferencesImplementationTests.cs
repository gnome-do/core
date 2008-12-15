// PreferencesImplementationTests.cs
//
// GNOME Do is the legal property of its developers. Please refer to the
// COPYRIGHT file distributed with this
// source distribution.
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

namespace Do.Platform.Preferences
{
	
	[TestFixture()]
	public class PreferencesImplementationTests
	{

		const string RootPath = "/test";
		PreferencesImplementation Prefs { get; set; }
		bool PrefChanged { get; set; }
		PreferenceChangedEventArgs PrefChangedArgs { get; set; }

		[SetUp]
		public void SetUp ()
		{
			IPreferencesService service = new Common.DictionaryPreferencesService ();
			//FIXME
			Prefs = new PreferencesImplementation (service, service, RootPath);
			PrefChanged = false;
			Prefs.PreferenceChanged += PrefsChanged;
		}

		void PrefsChanged (object sender, PreferenceChangedEventArgs args)
		{
			PrefChangedArgs = args;
			PrefChanged = true;
		}

		[TearDown]
		public void TearDown ()
		{
			Prefs.PreferenceChanged -= PrefsChanged;
			Prefs = null;
			PrefChangedArgs = null;
			PrefChanged = false;
		}

		[Test()]
		public void AbsolutePathForKey_AbsoluteKey ()
		{
			string absKey = Prefs.Service.AbsolutePathForKey ("/hello");
			
			Assert.AreEqual (Prefs.AbsolutePathForKey (absKey), absKey);
		}

		[Test()]
		public void AbsolutePathForKey_RelativeKey ()
		{
			string relKey = "hello";
			string absKey = RootPath + Prefs.Service.AbsolutePathForKey (relKey);
			
			Assert.AreEqual (Prefs.AbsolutePathForKey (relKey), absKey);
		}

		[Test()]
		public void Indexer ()
		{
			string key = "hello", val = "world";
			
			Prefs [key] = val;
			Assert.AreEqual (Prefs [key], val);
		}

		[Test()]
		public void Get_bool ()
		{
			string key = "hello";
			bool val = true;
			
			Prefs.Set (key, val);
			Assert.AreEqual (Prefs.Get (key, false), val);
		}

		[Test()]
		public void Set_bool ()
		{
			string key = "hello";
			bool val = true;
			
			Assert.IsTrue (Prefs.Set (key, val));
			Assert.AreEqual (Prefs.Get (key, false), val);
		}

		[Test()]
		public void Get_int ()
		{
			string key = "hello";
			int val = 1;
			
			Prefs.Set (key, val);
			Assert.AreEqual (Prefs.Get (key, 0), val);
		}

		[Test()]
		public void Set_int ()
		{
			string key = "hello";
			int val = 1;
			
			Assert.IsTrue (Prefs.Set (key, val));
			Assert.AreEqual (Prefs.Get (key, 0), val);
		}

		[Test()]
		public void Get_string ()
		{
			string key = "hello";
			string val = "yes";
			
			Prefs.Set (key, val);
			Assert.AreEqual (Prefs.Get (key, "no"), val);
		}

		[Test()]
		public void Set_string ()
		{
			string key = "hello";
			string val = "yes";
			
			Assert.IsTrue (Prefs.Set (key, val));
			Assert.AreEqual (Prefs.Get (key, "no"), val);
		}

		[Test()]
		public void PreferenceChanged ()
		{
			string key = "hello";
			string [] vals = new [] { "one", "two", "three" };

			for (int i = 0; i < vals.Length; ++i) {
				PrefChanged = false;
				
				Prefs.Set (key, vals [i]);
				
				Assert.IsTrue (PrefChanged);
				Assert.AreEqual (PrefChangedArgs.Key, key);
				Assert.AreEqual (PrefChangedArgs.NewValue, vals [i]);
				if (0 < i) {
					string lastVal = PrefChangedArgs.OldValue as string;
					Assert.IsNotNull (lastVal);
					Assert.AreEqual (lastVal, vals [i-1]);
				}
			}
		}
		
	}
}
