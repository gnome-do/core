// Preferences.cs
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

using Do.Addins;
using Do.Universe;

namespace MonoDock.Util
{
	
	
	public static class Preferences
	{
		static IPreferences prefs = Do.Addins.Util.GetPreferences ("Bezel");
		
		// we can not store these in gconf all the time since we query these a LOT
		// so we have to use a half and half solution
		static int text_width = prefs.Get<int> ("TextWidth", 350);
		public static int TextWidth {
			get { return text_width; }
			set { 
				prefs.Set<int> ("TextWidth", value); 
				text_width = value;
			}
		}
		
		static int zoom_size = prefs.Get<int> ("ZoomSize", 300);
		public static int ZoomSize {
			get { return zoom_size; }
			set { 
				prefs.Set<int> ("ZoomSize", value); 
				zoom_size = value;
			}
		}
		
		static int icon_size = prefs.Get<int> ("IconSize", 64);
		public static int IconSize {
			get { return icon_size; }
			set { 
				prefs.Set<int> ("IconSize", value); 
				icon_size = value;
			}
		}
		
		static double icon_quality = prefs.Get<double> ("IconQuality", 2);
		public static double IconQuality {
			get { return icon_quality; }
			set { 
				prefs.Set<double> ("IconQuality", value); 
				icon_quality = value;
			}
		}
	}
}
