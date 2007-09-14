// Util.cs created with MonoDevelop
// User: dave at 11:41 AM 8/20/2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.Text;

using Gtk;
using Gdk;

using Mono.Unix;
using System.Runtime.InteropServices;


namespace Do.Core
{
	
	public class Util
	{
		
		public static readonly Pixbuf UnknownPixbuf;
		public const int DefaultIconSize = 48;
		
		static Util ()
		{
			UnknownPixbuf = new Pixbuf (Colorspace.Rgb, true, 8, DefaultIconSize, DefaultIconSize);
			UnknownPixbuf.Fill (0x00000000);
		}
		
		public Util ()
		{
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
		
		public static Gdk.Pixbuf PixbufFromIconName (string name, int size)
		{
			Gtk.IconTheme iconTheme;
			Gdk.Pixbuf pixbuf;

			if (name == null || name.Length == 0) {
				return null;
			}
			
			if (name.StartsWith ("/")) {
				return new Gdk.Pixbuf (name, size, size);
			}
			
			iconTheme = Gtk.IconTheme.Default;
			try {
				pixbuf = iconTheme.LoadIcon (name, size, 0);
			} catch {
				pixbuf = null;
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
				pixbuf = iconTheme.LoadIcon ("gnome-mime-text", size, 0);
			}
			if (pixbuf == null) {
				pixbuf = UnknownPixbuf;
			}
			return pixbuf;
		}
		
		public static string MarkupSubstring (string main, string substring)
		{
			return main;
		}
		
		public static void PresentWindow (Gtk.Window window)
		{
			window.Present ();
			window.GdkWindow.Raise ();
			
			for (int i = 0; i < 5; i++) {
				if (TryGrabWindow (window)) {
					break;
				}
				System.Threading.Thread.Sleep (100);
			}
		}
		
		[DllImport ("/usr/lib/libgtk-x11-2.0.so.0")]
		private static extern uint gdk_x11_get_server_time (IntPtr gdk_window);
		
		private static bool TryGrabWindow (Gtk.Window window)
		{
			uint time;
			try {
				time = gdk_x11_get_server_time (window.GdkWindow.Handle);
			} catch (DllNotFoundException) {
				Console.WriteLine ("/usr/lib/libgtk-x11-2.0.so.0 not found - cannot grab window");
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
}
