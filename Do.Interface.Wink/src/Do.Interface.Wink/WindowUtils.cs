// WindowUtils.cs
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using Do.Platform;

using Wnck;

namespace Do.Interface.Wink
{
	
	public static class WindowUtils
	{
		enum OpenOfficeProducts {
			Writer,
			Calc,
			Base,
			Math,
			Impress,
		}
		
		static bool initialized;
		
		static string RemapFile {
			get { return Path.Combine (Services.Paths.UserDataDirectory, "RemapFile"); }
		}
		
		static IEnumerable<string> PrefixStrings {
			get {
				yield return "gksu";
				yield return "sudo";
				yield return "java";
				yield return "mono";
				yield return "ruby";
				yield return "padsp";
				yield return "aoss";
				yield return "python(\\d.\\d)?";
				yield return "(ba)?sh";
			}
		}
		
		public static IEnumerable<Regex> BadPrefixes { get; private set; }
		
		static Dictionary<string, string> RemapDictionary { get; set; }
		
		static List<Window> window_list;
		static bool window_list_update_needed;
		
		static Dictionary<int, string> exec_lines = new Dictionary<int, string> ();
		static DateTime last_update = new DateTime (0);
		
		#region ctor
		public static void Initialize ()
		{
			if (initialized)
				return;
			
			initialized = true;
			Wnck.Global.ClientType = Wnck.ClientType.Pager;
			
			List<Regex> regex = new List<Regex> ();
			foreach (string s in PrefixStrings) {
				 regex.Add (new Regex (string.Format ("^{0}$", s), RegexOptions.IgnoreCase));
			}
			
			BadPrefixes = regex.AsEnumerable ();
			
			Wnck.Screen.Default.WindowClosed += delegate {
				window_list_update_needed = true;
			};
			
			Wnck.Screen.Default.WindowOpened += delegate {
				window_list_update_needed = true;
			};
			
			Wnck.Screen.Default.ApplicationOpened += delegate {
				window_list_update_needed = true;
			};
			
			Wnck.Screen.Default.ApplicationClosed += delegate {
				window_list_update_needed = true;
			};
			
			BuildRemapDictionary ();
		}
		#endregion
		
		#region Private Methods
		static void BuildRemapDictionary ()
		{
			if (!File.Exists (RemapFile)) {
				RemapDictionary = BuildDefaultRemapDictionary ();
				
				try {
					using (StreamWriter writer = new StreamWriter (RemapFile)) {
						writer.WriteLine ("# Docky Remap File");
						writer.WriteLine ("# Add key value pairs following dictionary syntax");
						writer.WriteLine ("# key, value");
						writer.WriteLine ("# key, altKey, value");
						writer.WriteLine ("# Lines starting with # are comments, otherwise # is a valid character");
						
						foreach (KeyValuePair<string, string> kvp in RemapDictionary) {
							writer.WriteLine ("{0}, {1}", kvp.Key, kvp.Value);
						}
						writer.Close ();
					}
				} catch {
				}
			} else {
				RemapDictionary = new Dictionary<string, string> ();
				
				try {
					using (StreamReader reader = new StreamReader (RemapFile)) {
						string line;
						while (!reader.EndOfStream) {
							line = reader.ReadLine ();
							if (line.StartsWith ("#") || !line.Contains (","))
								continue;
							string [] array = line.Split (',');
							if (array.Length < 2 || array [0].Length == 0)
								continue;
							
							string val = array [array.Length - 1].Trim ().ToLower ();
							if (string.IsNullOrEmpty (val))
								continue;
							
							for (int i=0; i < array.Length - 1; i++) {
								string key = array [i].Trim ().ToLower ();
								if (string.IsNullOrEmpty (key))
									continue;
								RemapDictionary [key] = val;
							}
						}
						
						reader.Close ();
					}
				} catch {
					Log.Error ("Could not read remap file");
					RemapDictionary = BuildDefaultRemapDictionary ();
				}
			}
		}
		
		static Dictionary<string, string> BuildDefaultRemapDictionary ()
		{
			Dictionary<string, string> remapDict = new Dictionary<string, string> ();
			remapDict ["banshee.exe"] = "banshee";
			remapDict ["banshee-1"] = "banshee";
			remapDict ["azureus"] = "vuze";
			remapDict ["thunderbird-3.0"] = "thunderbird";
			remapDict ["thunderbird-bin"] = "thunderbird";
			
			return remapDict;
		}
		
		static void UpdateExecList ()
		{
			if ((DateTime.UtcNow - last_update).TotalMilliseconds < 200) return;
			
			Dictionary<int, string> old = exec_lines;
			
			exec_lines = new Dictionary<int, string> ();
			
			foreach (string dir in Directory.GetDirectories ("/proc")) {
				int pid;
				try { pid = Convert.ToInt32 (Path.GetFileName (dir)); } 
				catch { continue; }
				
				if (old.ContainsKey (pid)) {
					exec_lines [pid] = old [pid];
					continue;
				}
				
				string exec_line = CmdLineForPid (pid);
				if (string.IsNullOrEmpty (exec_line))
					continue;
				
				if (exec_line.Contains ("java") && exec_line.Contains ("jar")) {
					foreach (Window window in GetWindows ()) {
						if (window == null)
							continue;
						
						if (window.Pid == pid || window.Application.Pid == pid) {
							exec_line = window.ClassGroup.ResClass;
								
							// Vuze is retarded
							if (exec_line == "SWT")
								exec_line = window.Name;
						}
					}
				}	
				
				exec_line = ProcessExecString (exec_line);
					
				exec_lines [pid] = exec_line;
			}
			
			last_update = DateTime.UtcNow;
		}
		
		static ClickAction GetClickAction (IEnumerable<Window> windows)
		{
			if (!windows.Any ())
				return ClickAction.None;
			
			if (windows.Any (w => w.IsMinimized && w.IsInViewport (Wnck.Screen.Default.ActiveWorkspace)))
				return ClickAction.Restore;
			
			if (windows.Any (w => w.IsActive && w.IsInViewport (Wnck.Screen.Default.ActiveWorkspace)))
				return ClickAction.Minimize;
			
			return ClickAction.Focus;
		}
		#endregion
		
		#region Public Methods
		/// <summary>
		/// Returns a list of all windows on the default screen
		/// </summary>
		/// <returns>
		/// A <see cref="List"/>
		/// </returns>
		public static List<Window> GetWindows ()
		{
			if (window_list == null || window_list_update_needed)
				window_list = new List<Window> (Wnck.Screen.Default.WindowsStacked);
			
			return window_list;
		}
		
		/// <summary>
		/// Gets the command line excec string for a PID
		/// </summary>
		/// <param name="pid">
		/// A <see cref="System.Int32"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public static string CmdLineForPid (int pid)
		{
			string cmdline = null;
			
			try {
				string procPath = new [] { "/proc", pid.ToString (), "cmdline" }.Aggregate (Path.Combine);
				using (StreamReader reader = new StreamReader (procPath)) {
					cmdline = reader.ReadLine ();
					reader.Close ();
				}
			} catch { }
			
			return cmdline;
		}
		
		public static List<Window> WindowListForCmd (string exec)
		{
			List<Window> windows = new List<Window> ();
			if (string.IsNullOrEmpty (exec))
				return windows;
			
			// open office hakk
			if (exec.Contains ("ooffice")) {
				return GetOpenOfficeWindows (exec);
			}
			
			exec = ProcessExecString (exec);
			if (string.IsNullOrEmpty (exec))
				return windows;
			
			UpdateExecList ();
			
			foreach (KeyValuePair<int, string> kvp in exec_lines) {
				if (!string.IsNullOrEmpty (kvp.Value) && kvp.Value.Contains (exec)) {
					// we have a matching exec, now we just find every window whose PID matches this exec
					foreach (Window window in GetWindows ()) {
						if (window == null)
							continue;
						
						// this window matches the right PID and exec string, we can match it.
						bool pidMatch = window.Pid == kvp.Key || 
							(window.Application != null && window.Application.Pid == kvp.Key);
						
						if (pidMatch)
							windows.Add (window);
					}
				}
			}
			
			return windows.Distinct ().ToList ();
		}
		
		static List<Window> GetOpenOfficeWindows (string exec)
		{
			if (exec.Contains ("writer")) {
				return GetWindows ().Where ((Wnck.Window w) => w.Name.Contains ("OpenOffice.org Writer")).ToList ();
			} else if (exec.Contains ("math")) {
				return GetWindows ().Where ((Wnck.Window w) => w.Name.Contains ("OpenOffice.org Math")).ToList ();
			} else if (exec.Contains ("calc")) {
				return GetWindows ().Where ((Wnck.Window w) => w.Name.Contains ("OpenOffice.org Calc")).ToList ();
			} else if (exec.Contains ("impress")) {
				return GetWindows ().Where ((Wnck.Window w) => w.Name.Contains ("OpenOffice.org Impress")).ToList ();
			} else if (exec.Contains ("draw")) {
				return GetWindows ().Where ((Wnck.Window w) => w.Name.Contains ("OpenOffice.org Draw")).ToList ();
			} else {
				return new List<Window> (0);
			}
		}
		
		/// <summary>
		/// This method takes in an "execution string" from proc and applies a heureustic to try
		/// to magic out the name of the actual executing application.  The executing binary is not
		/// the desired target all the time.
		/// </summary>
		/// <param name="exec">
		/// A <see cref="System.String"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public static string ProcessExecString (string exec)
		{
			if (string.IsNullOrEmpty (exec))
				return exec;
			
			// lower it and trim off white space so we can abuse whitespace a bit
			exec = exec.ToLower ().Trim ();
			
			// if the user has specified a specific mapping, we can use that here
			if (RemapDictionary.ContainsKey (exec))
				return RemapDictionary [exec];
			
			// this is the "split" character or the argument separator.  If the string contains a null
			// it was fetched from /proc/PID/cmdline and will be nicely split up. Otherwise things get a bit
			// nasty, and it likely came from a .desktop file.
			char splitChar = Convert.ToChar (0x0);
			splitChar = exec.Contains (splitChar) ? splitChar : ' ';
			
			// this part is here soley for the remap file so that users may specify to remap based on just the name
			// without the full path.  If no remap file match is found, the net effect of this is nothing.
			if (exec.StartsWith ("/")) {
				string first_part = exec.Split (splitChar) [0];
				int length = first_part.Length;
				first_part = first_part.Split ('/').Last ();
				
				if (length < exec.Length)
					 first_part = first_part + " " + exec.Substring (length + 1);
						
				if (RemapDictionary.ContainsKey (first_part)) {
					return RemapDictionary [first_part];
				}
			}
			
			string [] parts = exec.Split (splitChar);
			for (int i = 0; i < parts.Length; i++) {
				// we're going to use this a lot
				string out_val = parts [i];
				
				// arguments are useless
				if (out_val.StartsWith ("-"))
					continue;
				
				// we want the end of paths
				if (out_val.Contains ("/"))
					out_val = out_val.Split ('/').Last ();
				
				// wine apps can do it backwards... who knew?
				if (out_val.Contains ("\\"))
					out_val = out_val.Split ('\\').Last ();
				
				// null out our part if is a bad prefix
				foreach (Regex regex in BadPrefixes) {
					if (regex.IsMatch (out_val)) {
						out_val = null;
						break;
					}
				}
				
				// check if it was a bad prefix...
				if (!string.IsNullOrEmpty (out_val)) {
					// sometimes we hide things with shell scripts.  This is the most common method of doing it.
					if (out_val.EndsWith (".real"))
						out_val = out_val.Substring (0, out_val.Length - ".real".Length);
					
					// give the remap dictionary one last shot at this
					if (RemapDictionary.ContainsKey (out_val))
						out_val = RemapDictionary [out_val];
					return out_val;
				}
			}
			return null;
		}
		
		/// <summary>
		/// Performs the "logical" click action on an entire group of applications
		/// </summary>
		/// <param name="apps">
		/// A <see cref="IEnumerable"/>
		/// </param>
		public static void PerformLogicalClick (IEnumerable<Window> windows)
		{
			List<Window> stack = new List<Window> (Wnck.Screen.Default.WindowsStacked);
			windows = windows.OrderByDescending (w => stack.IndexOf (w));
			
			bool not_in_viewport = !windows.Any (w => !w.IsSkipTasklist && w.IsInViewport (w.Screen.ActiveWorkspace));
			bool urgent = windows.Any (w => w.NeedsAttention ());
			
			if (not_in_viewport || urgent) {
				foreach (Wnck.Window window in windows) {
					if (urgent && !window.NeedsAttention ())
						continue;
					if (!window.IsSkipTasklist) {
						WindowControl.IntelligentFocusOffViewportWindow (window, windows);
						return;
					}
				}
			}
			
			switch (GetClickAction (windows)) {
			case ClickAction.Focus:
				WindowControl.FocusWindows (windows);
				break;
			case ClickAction.Minimize:
				WindowControl.MinimizeWindows (windows);
				break;
			case ClickAction.Restore:
				WindowControl.RestoreWindows (windows);
				break;
			}
		}
		
		#endregion
	}
}
