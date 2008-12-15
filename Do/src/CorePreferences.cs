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
	
	class CorePreferences  {

		#region Key constants and default values
		const string ThemeKey = "Theme";
		const string QuietStartKey = "QuietStart";
		const string StartAtLoginKey = "StartAtLogin";
		const string SummonKeybindingKey = "SummonKeybinding";
		const string AlwaysShowResultsKey = "AlwaysShowResults";
		const string TextModeKeybindingKey = "TextModeKeybinding";

		const string ThemeDefaultValue = "Classic";
		const bool QuietStartDefaultValue = false;
		const bool StartAtLoginDefaultValue = false;
		const bool AlwaysShowResultsDefaultValue = false;
		const string TextModeKeybindingDefaultValue = "period";
		const string SummonKeybindingDefaultValue = "<Super>space";

		const string DebugOption = "--debug";
		const string LogToFileOption = "--log-to-file";
		#endregion

		IPreferences Preferences { get; set; }
		
		public CorePreferences ()
		{
			Preferences = Services.Preferences.Get<CorePreferences> ();
		}

		bool HasOption (string option)
		{
			return Env.GetCommandLineArgs ().Contains (option);
		}

		public event EventHandler<PreferenceChangedEventArgs> KeybindingChanged;

		public event EventHandler<PreferenceChangedEventArgs> PreferenceChanged {
			add { Preferences.PreferenceChanged += value; }
			remove { Preferences.PreferenceChanged -= value; }
		}
		
		public bool WriteLogToFile {
			get { return HasOption (LogToFileOption); }
		}
		
		public bool Debug {
			get { return HasOption (DebugOption); }
		}

		public string SummonKeybindingPath {
			get { return Preferences.AbsolutePathForKey (SummonKeybindingKey); }
		}
		public string SummonKeybinding {
			get { return Preferences.Get (SummonKeybindingKey, SummonKeybindingDefaultValue); }
			set { Preferences.Set (SummonKeybindingKey, value); }
		}
		
		public string TextModeKeybinding {
			get { return Preferences.Get (TextModeKeybindingKey, TextModeKeybindingDefaultValue); }
			set { Preferences.Set (TextModeKeybindingKey, value); }
		}

		public string Theme {
			get { return Preferences.Get (ThemeKey, ThemeDefaultValue); }
			set { Preferences.Set (ThemeKey, value); }
		}

		public bool QuietStart {
			get { return Preferences.Get (QuietStartKey, QuietStartDefaultValue); }
			set { Preferences.Set (QuietStartKey, value); }
		}

		public bool StartAtLogin {
			get { return Preferences.Get(StartAtLoginKey, StartAtLoginDefaultValue); }
			set { Preferences.Set (StartAtLoginKey, value); }
		}
		
		public bool AlwaysShowResults {
			get { return Preferences.Get (AlwaysShowResultsKey, AlwaysShowResultsDefaultValue); }
			set { Preferences.Set (AlwaysShowResultsKey, value); }
		}
	}
}
