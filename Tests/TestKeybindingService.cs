// 
//  TestKeybindingService.cs
//  
//  Author:
//       Christopher James Halse Rogers <raof@ubuntu.com>
// 
//  Copyright © 2012 Christopher James Halse Rogers <raof@ubuntu.com>
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
using System.Reflection;
using NUnit.Framework;

using Mono.Addins;
using Mono.Unix;

using Do.Platform;
using Do.Platform.Common;

namespace Do
{
	class MockPreferencesService : IPreferencesService
	{
		public List<string> accessed_members = new List<string> ();
		public List<Tuple<string, object>> set_members = new List<Tuple<string, object>> ();

		#region IPreferencesService implementation
		public event EventHandler<PreferencesChangedEventArgs> PreferencesChanged;

		public bool Set<T> (string key, T val)
		{
			set_members.Add (new Tuple<string, object> (key, val));
			return true;
		}

		public bool TryGet<T> (string key, out T val)
		{
			accessed_members.Add (key);
			val = default (T);
			return false;
		}
		#endregion

		public void Reset ()
		{
			accessed_members = new List<string> ();
			set_members = new List<Tuple<string, object>> ();
		}
	}

	class MockKeybindingService : AbstractKeyBindingService
	{
		#region implemented abstract members of Do.Platform.Common.AbstractKeyBindingService
		public override bool RegisterOSKey (string keyString, EventCallback cb)
		{
			// No-op
			return true;
		}

		public override bool UnRegisterOSKey (string keyString)
		{
			// No-op
			return true;
		}
		#endregion
	}

	[TestFixture()]
	public class TestKeybindingService
	{
		MockKeybindingService keybinder;
		MockPreferencesService preferences;
		string initial_lang;

		[SetUp()]
		public void SetUp ()
		{
			Gtk.Application.Init ();
			Gdk.Threads.Init ();
			Core.PluginManager.Initialize ();
			AddinManager.Registry.Update ();
			preferences = AddinManager.GetExtensionObjects ("/Do/Service", true).OfType<MockPreferencesService> ().First ();
			keybinder = AddinManager.GetExtensionObjects ("/Do/Service", true).OfType<MockKeybindingService> ().First ();
			initial_lang = System.Environment.GetEnvironmentVariable ("LANGUAGE");
			preferences.Reset ();
		}

		[TearDown]
		public void TearDown ()
		{
			if (String.IsNullOrEmpty (initial_lang)) {
				System.Environment.SetEnvironmentVariable ("LANGUAGE", "");
			} else {
				System.Environment.SetEnvironmentVariable ("LANGUAGE", initial_lang);
			}
		}

		bool CollectionContainsSubstring (IEnumerable<string> collection, string search)
		{
			return collection.Aggregate<string, bool> (false, ((bool found, string str) => {
				return found || str.Contains (search);
			}));
		}

		[Test()]
		public void TestKeybindingRequestsCorrectPreferencesKey ()
		{
			Services.Keybinder.RegisterKeyBinding (new KeyBinding ("Summon_Do",
					Catalog.GetString ("Summon Do"), "<Super>space", delegate {}, true));

			Assert.True (CollectionContainsSubstring (preferences.accessed_members, "Summon_Do"));
		}


		[Test()]
		public void TestKeybindingUsesUntranslatedKey ()
		{
			System.Environment.SetEnvironmentVariable ("LANGUAGE", "de");
			Catalog.Init ("gnome-do", ".");

			if ("Nächstes Element" != Catalog.GetString ("Next Item")) {
				Assert.Inconclusive ("Translations are not properly set up, test cannot run.");
			}
			Services.Keybinder.RegisterKeyBinding (new KeyBinding ("Summon_Do",
					Catalog.GetString ("Summon Do"), "<Super>space", delegate {}, true));

			Assert.True (CollectionContainsSubstring (preferences.accessed_members, "Summon_Do"));
		}

		bool IsAllAscii (string text)
		{
			return text.All (c => c >= ' ' && c <= '~');
		}

		[Test]
		public void TestSetupKeybindingsUsesUntranslatedKeys ()
		{
			System.Environment.SetEnvironmentVariable ("LANGUAGE", "de");
			Catalog.Init ("gnome-do", ".");

			if ("Nächstes Element" != Catalog.GetString ("Next Item")) {
				Assert.Inconclusive ("Translations are not properly set up, test cannot run.");
			}
			Core.Controller controller = new Core.Controller ();
			var foo = controller.GetType ().GetMethod ("SetupKeybindings", System.Reflection.BindingFlags.NonPublic |
			                                           System.Reflection.BindingFlags.Instance);
			foo.Invoke (controller, new object [] { });

			foreach (var key in preferences.accessed_members.Concat (preferences.set_members.Select (pref => pref.Item1))) {
				Assert.That (IsAllAscii (key), String.Format ("Key “{0}” contains non-ASCII character", key));
			}
		}

		[Test]
		public void TestSetKeyStringUsesUntranslatedKey ()
		{
			System.Environment.SetEnvironmentVariable ("LANGUAGE", "de");
			Catalog.Init ("gnome-do", ".");

			if ("Nächstes Element" != Catalog.GetString ("Next Item")) {
				Assert.Inconclusive ("Translations are not properly set up, test cannot run.");
			}

			var binding = new KeyBinding ("Next_Item", Catalog.GetString ("Next Item"), "Down", delegate {}, true);
			keybinder.RegisterKeyBinding (binding);

			keybinder.SetKeyString (binding, "Up");

			foreach (var key in preferences.accessed_members.Concat (preferences.set_members.Select (pref => pref.Item1))) {
				Assert.That (IsAllAscii (key), String.Format ("Key “{0}” contains non-ASCII character", key));
			}
		}
	}
}

