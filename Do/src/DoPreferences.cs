/* DoPreferences.cs
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

namespace Do
{
	public class DoPreferences : Preferences {
	
		public DoPreferences ()
			: base ("core-preferences")
		{
		}
		
		public string SummonKeyBinding {
			get { return Get<string> ("SummonKeyBinding", "<Super>space"); }
			set { Set<string> ("SummonKeyBinding", value); }
		}

		public string Theme {
			get { return Get<string> ("Theme", "Classic"); }
			set { Set<string> ("Theme", value); }
		}

		public bool QuietStart {
			get { return Get<bool> ("QuietStart", false); }
			set { Set<bool> ("QuietStart", value); }
		}

		public bool StartAtLogin {
			get { return Get<bool> ("StartAtLogin", false); }
			set { Set<bool> ("StartAtLogin", value); }
		}
	}
}
