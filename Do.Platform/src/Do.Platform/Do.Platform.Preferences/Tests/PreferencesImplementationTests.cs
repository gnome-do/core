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

		string RootPath { get; set; }
		bool PreferencesDidChange { get; set; }
		IPreferences Preferences { get; set; }
		IPreferencesService Service { get; set; }
		ISecurePreferencesService SecureService { get; set; }
		PreferencesChangedEventArgs PreferencesChangedArgs { get; set; }

		[SetUp]
		public void SetUp ()
		{
			Service = new Common.DictionaryPreferencesService ();
			SecureService = null;
			Preferences = new PreferencesImplementation<PreferencesImplementationTests> (Service, SecureService);
			RootPath = "/" + typeof (PreferencesImplementationTests).Name;
			PreferencesDidChange = false;
			Preferences.PreferencesChanged += PreferencesChanged;
		}

		void PreferencesChanged (object sender, PreferencesChangedEventArgs args)
		{
			PreferencesChangedArgs = args;
			PreferencesDidChange = true;
		}

		[TearDown]
		public void TearDown ()
		{
			Preferences.PreferencesChanged -= PreferencesChanged;
			Preferences = null;
			PreferencesChangedArgs = null;
			PreferencesDidChange = false;
		}

		[Test()]
		public void Get_bool ()
		{
			string key = "hello";
			bool val = true;
			
			Preferences.Set (key, val);
			Assert.AreEqual (Preferences.Get (key, false), val);
		}

		[Test()]
		public void Set_bool ()
		{
			string key = "hello";
			bool val = true;
			
			Assert.IsTrue (Preferences.Set (key, val));
			Assert.AreEqual (Preferences.Get (key, false), val);
		}

		[Test()]
		public void Get_int ()
		{
			string key = "hello";
			int val = 1;
			
			Preferences.Set (key, val);
			Assert.AreEqual (Preferences.Get (key, 0), val);
		}

		[Test()]
		public void Set_int ()
		{
			string key = "hello";
			int val = 1;
			
			Assert.IsTrue (Preferences.Set (key, val));
			Assert.AreEqual (Preferences.Get (key, 0), val);
		}

		[Test()]
		public void Get_string ()
		{
			string key = "hello";
			string val = "yes";
			
			Preferences.Set (key, val);
			Assert.AreEqual (Preferences.Get (key, "no"), val);
		}

		[Test()]
		public void Set_string ()
		{
			string key = "hello";
			string val = "yes";
			
			Assert.IsTrue (Preferences.Set (key, val));
			Assert.AreEqual (Preferences.Get (key, "no"), val);
		}

		[Test()]
		public void PreferencesChanged ()
		{
			string key = "hello", val = "world", oldVal = "mars";
			
			Preferences.Set (key, oldVal);
			PreferencesChangedArgs = null;
			PreferencesDidChange = false;
			
			Preferences.Set (key, val);
			Assert.IsTrue (PreferencesDidChange);
			Assert.AreEqual (PreferencesChangedArgs.Key, key);
			Assert.AreEqual (PreferencesChangedArgs.Value as string, val);
		}
		
	}
}
