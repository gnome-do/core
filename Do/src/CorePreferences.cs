/* CorePreferences.cs
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
using System.Linq;
using Env = System.Environment;

using Do.Platform;

namespace Do
{
	
	public static class CorePreferences  {

		const string RootKey = "core";
		const string SummonKeyBindingName = "SummonKeyBinding";
		public static readonly string SummonKeyBindingPath =
			"/apps/gnome-do/preferences/" + RootKey + "/" + SummonKeyBindingName;

		static IPreferences prefs = Services.Preferences.Get (RootKey);

		public static event EventHandler<PreferenceChangedEventArgs> PreferenceChanged {
			add { prefs.PreferenceChanged += value; }
			remove { prefs.PreferenceChanged -= value; }
		}
		
		public static bool WriteLogToFile {
			get { return Env.GetCommandLineArgs ().Contains ("--log-to-file"); }
		}
		
		public static bool Debug {
			get { return Env.GetCommandLineArgs ().Contains ("--debug"); }
		}

		public static string SummonKeyBinding {
			get { return prefs.Get<string> (SummonKeyBindingName, "<Super>space"); }
			set { prefs.Set<string> (SummonKeyBindingName, value); }
		}
		
		public static string TextModeKeyBinding {
			get { return prefs.Get<string> ("TextModeKeyBinding", "period"); }
			set { prefs.Set<string> ("TextModeKeyBinding", value); }
		}
		
		public static Gdk.Key TextModeKeyVal {
			get {
				return (Gdk.Key) Enum.Parse (typeof (Gdk.Key), TextModeKeyBinding);
			}
		}

		public static string Theme {
			get { return prefs.Get<string> ("Theme", "Classic"); }
			set { prefs.Set<string> ("Theme", value); }
		}

		public static bool QuietStart {
			get { return prefs.Get<bool> ("QuietStart", false); }
			set { prefs.Set<bool> ("QuietStart", value); }
		}

		public static bool StartAtLogin {
			get { return prefs.Get<bool> ("StartAtLogin", false); }
			set { prefs.Set<bool> ("StartAtLogin", value); }
		}
		
		public static bool AlwaysShowResults {
			get { return prefs.Get<bool> ("AlwaysShowResults", false); }
			set { prefs.Set<bool> ("AlwaysShowResults", value); }
		}
	}
}
