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

using Gdk;

using Mono.Unix;
using Do.UI;
using Do.Addins;

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
			Addins.Util.GetPreferences = GetPreferences;

			// Environment utilities
			Addins.Util.Environment.Open = Environment.Open;

			// Appearance utilities			
			Addins.Util.Appearance.MarkupSafeString = Appearance.MarkupSafeString;
			Addins.Util.Appearance.PresentWindow = Appearance.PresentWindow;
			Addins.Util.Appearance.PopupMainMenuAtPosition = MainMenu.Instance.PopupAtPosition;
		}
		
		public static IPreferences GetPreferences (string id)
		{
			return new Preferences (id);
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
					Log.Debug ("Opening \"{0}\"...", open_item);
					start_proc.Start ();
				} catch (Exception e) {
					Log.Error ("Failed to open {0}: {1}", open_item, e.Message);
				}
			}
		}

		public class Appearance
		{			

			public static string MarkupSafeString (string s)
			{
				if (s == null) return string.Empty;
				s = s.Replace ("&", "&amp;");
				return s;
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

		public static string FormatCommonSubstrings (string main, string other, string format)
		{
			int pos, len, match_pos, last_main_cut;
			string lower_main, result;
			string skipped, matched, remainder;
			bool matchedTermination;

			result = string.Empty;
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
						remainder = string.Empty;
					}
					result = string.Format ("{0}{1}{2}", skipped, string.Format(format, matched), remainder);
					break;
				}
			}
			if (result == string.Empty) {
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
