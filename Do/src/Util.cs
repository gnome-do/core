// Util.cs created with MonoDevelop
// User: dave at 11:41 AM 8/20/2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Gtk;
using Gdk;

using Mono.Unix;

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
			
			// System utilities
			Addins.Util.Desktop.Open = Desktop.Open;
			
			// Appearance utilities
			Addins.Util.Appearance.PixbufFromIconName = Appearance.PixbufFromIconName;
			Addins.Util.Appearance.MarkupSafeString = Appearance.MarkupSafeString;
		}
		
		public class Desktop
		{
			public static bool Open (string open_item, out string error)
			{
				bool success;
				
				error = null;
				success = false;
				if (open_item == null) {
					success = false;
				} else {
					Log.Info ("Opening \"{0}\"...", open_item);
					try {
						open_item = string.Format ("\"{0}\"", open_item);
						Process.Start ("gnome-open", open_item);
						success = true;
					} catch (Exception e) {
						Log.Error ("Failed to open \"{0}\": ", e.Message);
						error = e.Message;
						success = false;
					}
				}
				return success;
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
				UnknownPixbuf = new Pixbuf (Colorspace.Rgb,
																		true, 8,
																		DefaultIconSize,
																		DefaultIconSize);
				UnknownPixbuf.Fill (0x00000000);
			}

			public static string MarkupSafeString (string s)
			{
				if (s == null) {
					return "";
				}
				s = s.Replace ("&", "&amp;");
				s = s.Replace ("=", "");
				return s;
			}

			public static Pixbuf PixbufFromIconName (string name, int size)
			{
				IconTheme iconTheme;
				Pixbuf pixbuf;
				string icon_description;

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
						return null;
					}
				}

				iconTheme = Gtk.IconTheme.Default;
				if (pixbuf == null) {
					try {
						pixbuf = iconTheme.LoadIcon (name, size, 0);
					} catch {
						pixbuf = null;
					}
				}
				if (pixbuf == null) {
					try {
						string newname = name.Remove (name.LastIndexOf ("."));
						pixbuf = iconTheme.LoadIcon (newname, size, 0);
					} catch {
						pixbuf = null;
					}
				}
				if (pixbuf == null && name == "gnome-mime-text-plain") {
					try {
						pixbuf = iconTheme.LoadIcon ("gnome-mime-text", size, 0);
					} catch {
						pixbuf = null;
					}
				}
				if (pixbuf == null) {
					try {
						pixbuf = iconTheme.LoadIcon ("empty", size, 0);
					} catch {
						pixbuf = UnknownPixbuf;
					}
				}
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
			
			[DllImport ("/usr/lib/libgtk-x11-2.0.so.0")]
			private static extern uint gdk_x11_get_server_time (IntPtr gdk_window);
			
			private static bool TryGrabWindow (Gtk.Window window)
			{
				uint time;
				try {
					time = gdk_x11_get_server_time (window.GdkWindow.Handle);
				} catch (DllNotFoundException e) {
					Log.Error ("Cannot grab window: {0}", e.Message);
					return true;
				}
				if (Pointer.Grab (window.GdkWindow,
										true,
										EventMask.ButtonPressMask |
										EventMask.ButtonReleaseMask |
										EventMask.PointerMotionMask,
										null,
										null,
										time) == GrabStatus.Success) {
					if (Keyboard.Grab (window.GdkWindow, true, time) == GrabStatus.Success) {
						return true;
					} else {
						Pointer.Ungrab (time);
						return false;
					} 
				}
				return false;
			}
				
			[DllImport ("libc")] // Linux
					private static extern int prctl (int option, byte [] arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5);
					
					[DllImport ("libc")] // BSD
					private static extern void setproctitle (byte [] fmt, byte [] str_arg);

			// Results in SIGSEGV
					public static void SetProcessName (string name)
					{
							try {
									if(prctl(15 /* PR_SET_NAME */, Encoding.ASCII.GetBytes(name + "\0"), 
											IntPtr.Zero, IntPtr.Zero, IntPtr.Zero) != 0) {
											throw new ApplicationException("Error setting process name: " + 
													Mono.Unix.Native.Stdlib.GetLastError());
									}
							} catch(EntryPointNotFoundException) {
									setproctitle(Encoding.ASCII.GetBytes("%s\0"), 
											Encoding.ASCII.GetBytes(name + "\0"));
							}
					}
			
		}
		
		public static IComparable Min (IComparable a, IComparable b) {
			return a.CompareTo (b) < 0 ? a : b;
		}
		
		public static IComparable Max (IComparable a, IComparable b) {
			return a.CompareTo (b) < 0 ? b : a;
		}
		
		public static IComparable Min3 (IComparable a, IComparable b, IComparable c) {
			IComparable min_ab = Min(a, b);
			IComparable min_bc = Min(b, c);
			return min_ab.CompareTo (min_bc) < 0 ? min_ab : min_bc;
		}
		
		public static IComparable Max3 (IComparable a, IComparable b, IComparable c) {
			IComparable max_ab = Max(a, b);
			IComparable max_bc = Max(b, c);
			return max_ab.CompareTo (max_bc) < 0 ? max_bc : max_ab;
		}
		
		// Quicksilver algorithm.
		// http://docs.blacktree.com/quicksilver/development/string_ranking?DokuWiki=10df5a965790f5b8cc9ef63be6614516
		public static float StringScoreForAbbreviation (string s, string ab) {
			return StringScoreForAbbreviationInRanges (s, ab,
			                                           new int[] {0, s.Length},
			                                           new int[] {0, ab.Length});
		}

		private static float StringScoreForAbbreviationInRanges (string s, string ab, int[] s_range, int[] ab_range) {
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

		public static string FormatCommonSubstrings (string main, string other, string format) {
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
	}		
}
