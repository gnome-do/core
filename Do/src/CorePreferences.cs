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
using System.Collections;
using System.Collections.Generic;
using Env = System.Environment;

using Do.Platform;

namespace Do
{
	class CorePreferences  {

		#region Key constants and default values
		const string ThemeKey = "Theme";
		const string QuietStartKey = "QuietStart";
		const string StartAtLoginKey = "StartAtLogin";
		const string AlwaysShowResultsKey = "AlwaysShowResults";
		const string ForceClassicWindowKey = "ForceClassicWindow";

		const string ThemeDefaultValue = "Classic";
		const bool QuietStartDefaultValue = false;
		const bool StartAtLoginDefaultValue = false;
		const bool AlwaysShowResultsDefaultValue = false;
		const bool ForceClassicWindowDefaultValue = false;
		const string TextModeKeybindingDefaultValue = "period";
		const string SummonKeybindingDefaultValue = "<Super>space";

		const string DebugOption = "--debug";
		const string LogToFileOption = "--log-to-file";
                

		#endregion

		public event EventHandler<PreferencesChangedEventArgs> ThemeChanged;
		
		IPreferences Preferences { get; set; }
		
		public CorePreferences ()
		{
			Preferences = Services.Preferences.Get<CorePreferences> ();
			Preferences.PreferencesChanged += PreferencesChanged;
		}
		
		public bool WriteLogToFile {
			get { return HasOption (LogToFileOption); }
		}
		
		public static bool PeekDebug {
			get { return HasOption (DebugOption); }
		}

		public bool Debug {
			get { return CorePreferences.PeekDebug; }
		}

		public string Theme {
			get { return Preferences.Get (ThemeKey, ThemeDefaultValue); }
			set { if (Theme != value) Preferences.Set (ThemeKey, value); }
		}

		public bool QuietStart {
			get { return Preferences.Get (QuietStartKey, QuietStartDefaultValue); }
			set { if (QuietStart != value) Preferences.Set (QuietStartKey, value); }
		}

		public bool StartAtLogin {
			get { return Preferences.Get(StartAtLoginKey, StartAtLoginDefaultValue); }
			set { if (StartAtLogin != value) Preferences.Set (StartAtLoginKey, value); }
		}
		
		public bool AlwaysShowResults {
			get { return Preferences.Get (AlwaysShowResultsKey, AlwaysShowResultsDefaultValue); }
			set { if (AlwaysShowResults != value) Preferences.Set (AlwaysShowResultsKey, value); }
		}
		
		public bool ForceClassicWindow {
			get { return Preferences.Get (ForceClassicWindowKey, ForceClassicWindowDefaultValue); }
			set { if (ForceClassicWindow != value) Preferences.Set (ForceClassicWindowKey, value); }
		}
		
		static bool HasOption (string option)
		{
			return Env.GetCommandLineArgs ().Contains (option);
		}


		void PreferencesChanged (object sender, PreferencesChangedEventArgs e)
		{
			switch (e.Key) {
			case ThemeKey:
				if (ThemeChanged != null)
					ThemeChanged (this, e);
				break;
			}
		}
	}
}
