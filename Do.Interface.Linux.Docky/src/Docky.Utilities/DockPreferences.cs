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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using Do.Interface;
using Do.Universe;
using Do.Platform;

namespace Docky.Utilities
{
	
	public class DockPreferences
	{
		static IPreferences prefs = Services.Preferences.Get<DockPreferences> ();
		
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
		
		static double zoom_percent = prefs.Get<double> ("ZoomPercent", 2);
		public static double ZoomPercent {
			get { return zoom_percent; }
			set {
				prefs.Set<double> ("ZoomPercent", value);
				zoom_percent = value;
			}
		}
		
		public static int FullIconSize {
			get {
				return (int) (IconSize*IconQuality);
			}
		}
		
		static int icon_size = prefs.Get<int> ("IconSize", 64);
		public static int IconSize {
			get { return icon_size; }
			set { 
				if (value < 24 || value > 128)
					return;
				if (Math.Abs (value - 64) < 4)
					value = 64;
				if (Math.Abs (value - 32) < 4)
					value = 32;
				
				if (value == icon_size)
					return;
				
				prefs.Set<int> ("IconSize", value); 
				icon_size = value;
				IconSizeChanged ();
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
		
		static bool autohide = prefs.Get<bool> ("AutoHide", false);
		public static bool AutoHide {
			get { return autohide; }
			set {
				prefs.Set<bool> ("AutoHide", value);
				autohide = value;
			}
		}
		
		static int summon_time = prefs.Get<int> ("SummonTime", 100);
		public static int SummonTime {
			get { return summon_time; }
			set {
				prefs.Set<int> ("SummonTime", value);
				summon_time = value;
			}
		}
		
		static int automatic_icons = prefs.Get<int> ("AutomaticIcons", 10);
		public static int AutomaticIcons {
			get { return automatic_icons; }
			set {
				prefs.Set<int> ("AutomaticIcons", value);
				automatic_icons = value;
			}
		}
		
		#region blacklists
		static List<string> item_blacklist = DeserializeBlacklist ();
		public static IEnumerable<string> ItemBlacklist {
			get {
				return item_blacklist;
			}
		}
		
		public static void AddBlacklistItem (string item) 
		{
			item_blacklist.Add (item);
			SerializeBlacklist ();
		}
		
		public static void RemoveBlacklistItem (string item)
		{
			item_blacklist.Remove (item);
			SerializeBlacklist ();
		}

		static string BlacklistFile {
			get {
				return Path.Combine (Services.Paths.UserDataDirectory, "dock_blacklist");
			}
		}
		
		static List<string> DeserializeBlacklist ()
		{
			string file = BlacklistFile;
			if (!File.Exists (file))
				return new List<string> ();
			
			try {
				Stream s = File.OpenRead (file);
				BinaryFormatter bf = new BinaryFormatter ();
				List<string> out_list = bf.Deserialize (s) as List<string>;
				s.Close ();
				s.Dispose ();
				return out_list;
			} catch { return new List<string> (); }
		}
		
		static void SerializeBlacklist ()
		{
			string file = BlacklistFile;
			
			try {
				Stream s = File.Open (file, FileMode.OpenOrCreate);
				BinaryFormatter bf = new BinaryFormatter ();
				bf.Serialize (s, item_blacklist);
				s.Close ();
				s.Dispose ();
			} catch { }
		}
		#endregion
		public static event NullEventHandler IconSizeChanged;
	}
}
