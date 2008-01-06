/* Util.cs
 *
 * GNOME Do is the legal property of its developers. Please refer to the
 * COPYRIGHT file distributed with this source distribution.
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
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Gtk;
using Gdk;

using Mono.Unix;
using Do.UI;

namespace Do
{
	public class Util
	{
		static Util ()
		{
		}

		public static void Initialize ()
		{
			// Misc
			Addins.Util.FormatCommonSubstrings = FormatCommonSubstrings;

			// Environment utilities
			Addins.Util.Environment.Open = Environment.Open;

			// Appearance utilities
			Addins.Util.Appearance.PixbufFromIconName = Appearance.PixbufFromIconName;
			Addins.Util.Appearance.MarkupSafeString = Appearance.MarkupSafeString;
			Addins.Util.Appearance.PresentWindow = Appearance.PresentWindow;
			//
			Addins.Util.Appearance.PopupMainMenuAtPosition = MainMenu.Instance.PopupAtPosition;
		}

		public class Environment
		{
			public static void Open (string open_item)
			{
				Process start_proc;

				if (open_item == null) return;

				start_proc = new Process ();
				// start_proc.StartInfo.FileName = open_item;
				// start_proc.StartInfo.UseShellExecute = true;
				start_proc.StartInfo.FileName = "xdg-open";
				start_proc.StartInfo.Arguments = open_item;
				try {
					Log.Info ("Opening \"{0}\"...", open_item);
					start_proc.Start ();
				} catch (Exception e) {
					Log.Error ("Failed to open {0}: {1}", open_item, e.Message);
				}
			}
		}

		public class Appearance
		{
			public static readonly Pixbuf UnknownPixbuf;
			public const int DefaultIconSize = 80;

			// TODO: Implement a separate Pixbuf cache class
			static Dictionary<string, Pixbuf> pixbufCache;

			static Appearance ()
			{
				pixbufCache = new Dictionary<string,Gdk.Pixbuf> ();
				UnknownPixbuf = new Pixbuf (Colorspace.Rgb, true, 8, 1, 1);
				UnknownPixbuf.Fill (0x00000000);

				Gtk.IconTheme.Default.Changed += OnDefaultIconThemeChanged;
				Do.Controller.Vanished += OnMainWindowVanished;
			}

			private static void OnMainWindowVanished (object sender, EventArgs args)
			{
				pixbufCache.Clear ();
			}
			
			private static void OnDefaultIconThemeChanged (object sender, EventArgs args)
			{
				pixbufCache.Clear ();
			}

			public static string MarkupSafeString (string s)
			{
				if (s == null) return "";
				s = s.Replace ("&", "&amp;");
				return s;
			}

			public static Pixbuf PixbufFromIconName (string name, int size)
			{
				IconTheme iconTheme;
				Pixbuf pixbuf;
				string icon_description, name_noext;

				if (name == null || name.Length == 0) {
					return null;
				}

				pixbuf = null;
				icon_description = name + size;
				if (pixbufCache.ContainsKey (icon_description)) {
					return pixbufCache[icon_description];
				}

				// TODO: Use a GNOME ThumbnailFactory
				if (name.StartsWith ("/")) {
					try {
						pixbuf = new Pixbuf (name, size, size);
					} catch {
						return UnknownPixbuf;
					}
				}

				iconTheme = Gtk.IconTheme.Default;
				if (name.Contains (".")) name_noext = name.Remove (name.LastIndexOf ("."));
				else name_noext = name;
				
				if (iconTheme.HasIcon (name)) {
					pixbuf = iconTheme.LoadIcon (name, size, 0);
				} else if (iconTheme.HasIcon (name_noext)) {
					pixbuf = iconTheme.LoadIcon (name_noext, size, 0);
				} else if (name == "gnome-mime-text-plain" && iconTheme.HasIcon ("gnome-mime-text")) {
					pixbuf = iconTheme.LoadIcon ("gnome-mime-text", size, 0);
				}
				
				if (pixbuf == null) {
					// If the icon couldn't be found in the default icon theme, search Tango.
					IconTheme tango;

					tango = new IconTheme ();
					tango.CustomTheme = "Tango";
					if (tango.HasIcon (name)) {
						pixbuf = tango.LoadIcon (name, size, 0);
					} else if (tango.HasIcon (name_noext)) {
						pixbuf = tango.LoadIcon (name_noext, size, 0);
					} else if (name == "gnome-mime-text-plain" && tango.HasIcon ("gnome-mime-text")) {
						pixbuf = tango.LoadIcon ("gnome-mime-text", size, 0);
					}
				}	
				
				if (pixbuf == null && iconTheme.HasIcon ("empty")) {
					pixbuf = iconTheme.LoadIcon ("empty", size, 0);
				}
				if (pixbuf == null) {
					pixbuf = UnknownPixbuf;
				}
				// Cache icon pixbuf.
				if (pixbuf != null && pixbuf != UnknownPixbuf) {
					pixbufCache[icon_description] = pixbuf;
				}
				return pixbuf;
			}

			public static void PresentWindow (Gtk.Window window)
			{
				window.Present ();
				window.GdkWindow.Raise ();

				for (int i = 0; i < 100; i++) {
					if (TryGrabWindow (window)) {
						break;
					}
					Thread.Sleep (100);
				}
			}

			private static bool TryGrabWindow (Gtk.Window window)
			{
				uint time;

				time = Gtk.Global.CurrentEventTime;
				if (Pointer.Grab (window.GdkWindow,
				                  true,
				                  EventMask.ButtonPressMask |
				                  EventMask.ButtonReleaseMask |
				                  EventMask.PointerMotionMask,
				                  null,
				                  null,
				                  time) == GrabStatus.Success)
				{
					if (Keyboard.Grab (window.GdkWindow, true, time) == GrabStatus.Success) {
						return true;
					} else {
						Pointer.Ungrab (time);
						return false;
					}
				}
				return false;
			}
		}

		// Quicksilver algorithm.
		// http://docs.blacktree.com/quicksilver/development/string_ranking?DokuWiki=10df5a965790f5b8cc9ef63be6614516
		public static float StringScoreForAbbreviation (string s, string ab) {
			return StringScoreForAbbreviationInRanges (s, ab,
			                                           new int[] {0, s.Length},
			                                           new int[] {0, ab.Length});
		}

		private static float StringScoreForAbbreviationInRanges (string s, string ab, int[] s_range, int[] ab_range)
		{
			float score, remainingScore;
			int i, j;
			int[] remainingSearchRange = {0, 0};

			if (ab_range[1] == 0) return 0.9F;
			if (ab_range[1] > s_range[1]) return 0.0F;
			for (i = ab_range[1]; i > 0; i--) {
				// Search for steadily smaller portions of the abbreviation.
				// TODO Turn this into a dynamic algorithm.
				string ab_substring = ab.Substring (ab_range[0], i);
				string s_substring = s.Substring (s_range[0], s_range[1]);
				int loc = s_substring.IndexOf (ab_substring, StringComparison.CurrentCultureIgnoreCase);
				if (loc < 0) continue;
				remainingSearchRange[0] = loc + i;
				remainingSearchRange[1] = s_range[1] - remainingSearchRange[0];
				remainingScore = StringScoreForAbbreviationInRanges (s, ab,
				                                                     remainingSearchRange,
				                                                     new int[] {ab_range[0]+i, ab_range[1]-i});
				if (remainingScore != 0) {
					score = remainingSearchRange[0] - s_range[0];
					if (loc > s_range[0]) {
						// If some letters were skipped.
						if (s[loc-1] == ' ') {
							for (j = loc-2; j >= s_range[0]; j--) {
								if (s[j] == ' ')
									score--;
								else
									score -= 0.15F;
							}
						}
						// Else if word is uppercase (?)
						else if (s[loc] >= 'A') {
							for (j = loc-1; j >= s_range[0]; j--) {
								if (s[j] >= 'A')
									score--;
								else
									score -= 0.15F;
							}
						}
						else {
							score -= loc - s_range[0];
						}
					}
					score += remainingScore * remainingSearchRange[1];
					score /= s_range[1];
					return score;
				}
			}
			return 0;
		}

		public static string FormatCommonSubstrings (string main, string other, string format)
		{
			int pos, len, match_pos, last_main_cut;
			string lower_main, result;
			string skipped, matched, remainder;
			bool matchedTermination;

			result = "";
			match_pos = last_main_cut = 0;
			lower_main = main.ToLower ();
			other = other.ToLower ();

			for (pos = 0; pos < other.Length; ++pos) {
				matchedTermination = false;
				for (len = 1; len <= other.Length - pos; ++len) {
					int tmp_match_pos = lower_main.IndexOf (other.Substring (pos, len));
					if (tmp_match_pos < 0) {
						len--;
						matchedTermination = false;
						break;
					} else {
						matchedTermination = true;
						match_pos = tmp_match_pos;
					}
				}
				if (matchedTermination) {
					len--;
				}
				if (0 < len) {
					 //Theres a match starting at match_pos with positive length
					skipped = main.Substring (last_main_cut, match_pos - last_main_cut);
					matched = main.Substring (match_pos, len);
					if ( skipped.Length + matched.Length < main.Length) {
						remainder = FormatCommonSubstrings ( main.Substring (match_pos + len), other.Substring (pos + len), format);
					}
					else {
						remainder = "";
					}
					result = string.Format ("{0}{1}{2}", skipped, string.Format(format, matched), remainder);
					break;
				}
			}
			if (result == "") {
				// no matches
				result = main;
			}
			return result;
		}

		[DllImport ("libc")] // Linux
		private static extern int prctl (int option, byte [] arg2, IntPtr arg3,
		                                 IntPtr arg4, IntPtr arg5);

		[DllImport ("libc")] // BSD
		private static extern void setproctitle (byte [] fmt, byte [] str_arg);

		public static void SetProcessName (string name)
		{
			try {
				if (prctl (15 /* PR_SET_NAME */, Encoding.ASCII.GetBytes (name + "\0"),
					IntPtr.Zero, IntPtr.Zero, IntPtr.Zero) != 0) {
					throw new ApplicationException ("Error setting process name: " +
						Mono.Unix.Native.Stdlib.GetLastError ());
				}
			} catch (EntryPointNotFoundException) {
				setproctitle (Encoding.ASCII.GetBytes ("%s\0"),
					Encoding.ASCII.GetBytes (name + "\0"));
			}
		}
	}
}
