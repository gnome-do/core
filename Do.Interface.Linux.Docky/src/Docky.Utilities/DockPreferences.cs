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
		public static event Action AutohideChanged;
		public static event Action IconSizeChanged;
		public static event Action AutomaticIconsChanged;
		public static event Action MonitorChanged;
		public static event Action AllowOverlapChanged;
		public static event Action OrientationChanged;
	
		public const int IconBorderWidth = 2;
		public const string HighlightFormat = "<span foreground=\"#5599ff\">{0}</span>";
		
		static IPreferences prefs = Services.Preferences.Get<DockPreferences> ();
		const int DefaultIconSize = 64;
		
		static DockPreferences ()
		{
			if (System.IO.Directory.Exists ("/sys/module/nvidia")) {
				// new nvidia drivers have the nasty habbit of migrating out pixmaps out of video memory after 10
				// minutes or so, so if we recreate them more frequently than that, it doesn't do that to us.
				GLib.Timeout.Add (5 * 60 * 1000, delegate {
					if (IconSizeChanged != null)
						IconSizeChanged ();
					return true;
				});
			}
			prefs.PreferencesChanged += HandlePreferencesChanged;
		}
		
		static void HandlePreferencesChanged (object o, PreferencesChangedEventArgs e) {
			if (e.Key == "IndicateMultipleWindows")
				SetIndicateMultipleWindows ((bool)e.Value);
			if (e.Key == "ZoomPercent")
				SetZoomPercent ((double)e.Value);
			if (e.Key == "EnableZoom")
				SetZoomEnabled ((bool)e.Value);
			if (e.Key == "IconSize")
				SetIconSize ((int)e.Value);
			if (e.Key == "SummonTime")
				SetSummonTime (new TimeSpan (0, 0, 0, 0, (int)e.Value));
			if (e.Key == "AutomaticIcons")
				SetAutomaticIcons ((int)e.Value);
			if (e.Key == "Monitor")
				SetMonitor (Math.Max (0, (int)e.Value));
			if (e.Key == "Orientation")
				SetOrientation ((DockOrientation) Enum.Parse (typeof (DockOrientation), (string)e.Value));
			if (e.Key == "AutohideType")
				SetAutohideType ((AutohideType) Enum.Parse (typeof (AutohideType), (string)e.Value));
		}
		
		public static int TextWidth {
			get { return 350; }
		}
		
		public static int ZoomSize {
			get { return (int) (330 * (IconSize / (double) DefaultIconSize)); }
		}

		static bool indicate_multiple_windows = prefs.Get ("IndicateMultipleWindows", false);
		public static bool IndicateMultipleWindows {
			get { return indicate_multiple_windows; }
			set { 
				if (SetIndicateMultipleWindows (value))
					prefs.Set ("IndicateMultipleWindows", value); 
			}
		}
		static bool SetIndicateMultipleWindows (bool val)
		{
			if (indicate_multiple_windows == val) return false;
			indicate_multiple_windows = val;
			return true;
		}
		
		static double zoom_percent = Math.Round (prefs.Get ("ZoomPercent", 2.0), 1);
		public static double ZoomPercent {
			get { return ZoomEnabled ? zoom_percent : 1; }
			set {
				if (SetZoomPercent (value))
					prefs.Set ("ZoomPercent", value);
			}
		}
		static bool SetZoomPercent (double val)
		{
			if (val < 1)
				val = 1;
			if (zoom_percent == val) return false;
			zoom_percent = val;
			if (IconSizeChanged != null)
				IconSizeChanged ();
			return true;
		}
		
		static bool enable_zoom = prefs.Get ("EnableZoom", true);
		public static bool ZoomEnabled {
			get { return enable_zoom; }
			set {
				if (SetZoomEnabled (value))
					prefs.Set ("EnableZoom", value);
			}
		}
		static bool SetZoomEnabled (bool val)
		{
			if (enable_zoom == val) return false;
			enable_zoom = val;
			if (IconSizeChanged != null)
				IconSizeChanged ();
			return true;
		}
		
		public static int FullIconSize {
			get {
				return (int) (IconSize*IconQuality);
			}
		}
		
		static int max_icon_size = 128;
		public static int MaxIconSize {
			get { return max_icon_size; }
			set {
				int tmp = IconSize;
				max_icon_size = value;
				if (tmp != IconSize && IconSizeChanged != null)
					IconSizeChanged ();
			}
		}
		
		static int icon_size = prefs.Get ("IconSize", DefaultIconSize);
		public static int IconSize {
			get { return Math.Min (icon_size, MaxIconSize); }
			set {
				if (SetIconSize (value))
					prefs.Set ("IconSize", value); 
			}
		}
		static bool SetIconSize (int val)
		{
			if (val < 24)
				val = 24;
			if (val > 128)
				val = 128;
			
			if (val == icon_size)
				return false;
			
			icon_size = val;
			if (IconSizeChanged != null)
				IconSizeChanged ();
			return true;
		}
		
		/// <summary>
		/// Currently returns ZoomPercent.  This is useful in the future case where we wish to optimize for best
		/// looking icons by picking "good" sizes.  This is not implemented yet however.
		/// </summary>
		public static double IconQuality {
			get { return ZoomPercent; }
		}
		
		public static bool AutoHide {
			get { return AutohideType != AutohideType.None; }
		}
		
		static TimeSpan summon_time = new TimeSpan (0, 0, 0, 0, prefs.Get ("SummonTime", 100));
		public static TimeSpan SummonTime {
			get { return summon_time; }
			set {
				if (SetSummonTime (value))
					prefs.Set ("SummonTime", value.TotalMilliseconds);
			}
		}
		static bool SetSummonTime (TimeSpan val)
		{
			if (summon_time == val) return false;
			summon_time = val;
			return true;
		}
		
		static int automatic_icons = prefs.Get ("AutomaticIcons", 5);
		public static int AutomaticIcons {
			get { return automatic_icons; }
			set {
				if (SetAutomaticIcons (value))
					prefs.Set ("AutomaticIcons", value);
			}
		}
		static bool SetAutomaticIcons (int val)
		{
			if (val < 0)
				val = 0;
			if (automatic_icons == val) return false;
			automatic_icons = val;
			
			if (AutomaticIconsChanged != null)
				AutomaticIconsChanged ();
			return true;
		}

		static int monitor = Math.Max (0, prefs.Get ("Monitor", 0));
		public static int Monitor {
			get {
				monitor = Math.Max (0, Math.Min (monitor, Gdk.Screen.Default.NMonitors - 1));
				return monitor;
			}
			set {
				if (SetMonitor (value))
					prefs.Set ("Monitor", value);
			}
		}
		static bool SetMonitor (int val)
		{
			if (val >= Gdk.Screen.Default.NMonitors || val < 0)
				val = 0;

			if (monitor == val)
				return false;
			
			monitor = val;

			Interface.LayoutUtils.Recalculate ();
			if (MonitorChanged != null)
				MonitorChanged ();
			return true;
		}

		static DockOrientation orientation = (DockOrientation) Enum.Parse (typeof (DockOrientation), prefs.Get ("Orientation", DockOrientation.Bottom.ToString ()));
		public static DockOrientation Orientation {
			get {
				if (orientation != DockOrientation.Top && orientation != DockOrientation.Bottom)
					orientation = DockOrientation.Bottom;
				
				return orientation;
			}
			set {
				if (SetOrientation (value))
					prefs.Set ("Orientation", value.ToString ());
			}
		}
		static bool SetOrientation (DockOrientation val)
		{
			if (orientation == val)
				return false;
			orientation = val;
			if (OrientationChanged != null)
				OrientationChanged ();
			return true;
		}
		
		static AutohideType hide = (AutohideType) Enum.Parse (typeof (AutohideType), prefs.Get ("AutohideType", AutohideType.None.ToString ()));
		public static AutohideType AutohideType {
			get { return hide; }
			set {
				if (SetAutohideType (value))
					prefs.Set ("AutohideType", value.ToString ());
			}
		}
		static bool SetAutohideType (AutohideType val)
		{
			if (hide == val)
				return false;
			hide = val;
			if (AutohideChanged != null)
				AutohideChanged ();
			return true;
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
		
		public static void ClearBlacklist ()
		{
			item_blacklist.Clear ();
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
	}
}
