// Paths.cs
//
// GNOME Do is the legal property of its developers. Please refer to the
// COPYRIGHT file distributed with this source distribution.
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
using System.IO;
using System.Collections.Generic;

using Do.Universe;

namespace Do.Platform
{
	
	public static class Paths
	{

		public interface Implementation
		{
			string UserHome { get; }
			string UserDesktop { get; }
			string UserData { get; }
			string ApplicationData { get; }
			IEnumerable<string> SystemPlugins { get; }
		}

		public static Implementation Imp { get; private set; }

		public static void Initialize (Implementation imp)
		{
			if (Imp != null)
				throw new Exception ("Already has Implementation");
			if (imp == null)
				throw new ArgumentNullException ("Implementation may not be null");
			
			Imp = imp;

			if (Directory.Exists (Temp)) {
				try {
					Directory.Delete (Temp, true);
				} catch (Exception e) {
					Console.Error.WriteLine ("Could not delete temporary directory {0}: {1}", Temp, e.Message);
				}
			}
			CreateDirs (ApplicationData, UserData, UserPlugins, Temp);
		}

		private static void CreateDirs (string first, params string [] rest)
		{
			if (!Directory.Exists (first)) {
				try {
					Directory.CreateDirectory (first);
				} catch (Exception e) {
					Console.Error.WriteLine ("Failed to create directory {0}: {1}", first, e.Message);
				}
			}

			foreach (string dir in rest) {
				if (!Directory.Exists (dir)) {
					try {
						Directory.CreateDirectory (dir);
					} catch (Exception e) {
						Console.Error.WriteLine ("Failed to create directory {0}: {1}", dir, e.Message);
					}
				}
			}
		}

		public static string Combine (string first, params string [] components)
		{
			if (string.IsNullOrEmpty (first)) {
				throw new ArgumentException ("First component must not be null or empty", "first");
			} else if (components == null || components.Length < 1) {
				throw new ArgumentException ("One or more path components must be provided", "components");
			}

			string result = first;
			foreach (string component in components) {
				result = Path.Combine (result, component);
			}

			return result;
		}

		#region Implementation

		public static string UserHome {
			get {
				return Imp.UserHome;
			}
		}

		public static string UserData {
			get {
				return Imp.UserData;
			}
		}

		public static string UserDesktop {
			get {
				return Imp.UserDesktop;
			}
		}
		
		public static string ApplicationData {
			get {
				return Imp.ApplicationData;
			}
		}

		public static IEnumerable<string> SystemPlugins {
			get {
				return Imp.SystemPlugins;
			}
		}

		#endregion

		public static string Temp {
			get {
				return Combine (Paths.ApplicationData, "tmp");
			}
		}
		
		public static string Log {
			get {
				return Combine (Paths.ApplicationData, "log");
			}
		}

		public static string UserPlugins {
			get {
 				return Combine (UserData, "plugins-" + Version);
			}
		}
		
		public static string GetTemporaryFilePath ()
		{
			int fileId;
			Random random;
			string fileName;

			// Ensure temp directory exists so loop doesn't diverge.
			CreateDirs (Temp);
			
			random = new Random ();
			do {
				fileId = random.Next ();
				fileName = Combine (Temp, fileId.ToString ());
			} while (File.Exists (fileName));
			return fileName;
		}

		private static string Version {
			get {
				System.Reflection.AssemblyName name;

				name = typeof (Paths).Assembly.GetName ();
				return string.Format ("{0}.{1}.{2}",
					name.Version.Major, name.Version.Minor, name.Version.Build);
			}
		}
	}
}
