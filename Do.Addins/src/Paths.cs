/* Paths.cs
 *
 * Original Author:
 *  Aaron Bockover <abockover@novell.com>
 *
 * Copyright (C) 2005-2007 Novell, Inc.
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
using System.IO;
using Mono.Unix;

namespace Do {

	public static class Paths {

		static Paths ()
		{
			if (Directory.Exists (Temp)) {
				try {
					Directory.Delete (Temp, true);
				} catch (Exception e) {
					Console.Error.WriteLine (
						"Could not delete temporary directory {0}: {1}",
						Temp, e.Message);
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
					Console.Error.WriteLine (
						"Failed to create directory {0}: {1}",
						first, e.Message);
				}
			}

			foreach (string dir in rest) {
				if (!Directory.Exists (dir)) {
					try {
						Directory.CreateDirectory (dir);
					} catch (Exception e) {
						Console.Error.WriteLine (
							"Failed to create directory {0}: {1}",
							dir, e.Message);
					}
				}
			}
		}

		public static string ReadXdgUserDir (string key, string fallback)
		{
			string home_dir, config_dir, env_path, user_dirs_path;

			home_dir = UserHome;
			config_dir = Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData);

			env_path = Environment.GetEnvironmentVariable (key);
			if (!String.IsNullOrEmpty (env_path)) {
				return env_path;
			}

			user_dirs_path = Path.Combine (config_dir, "user-dirs.dirs");
			if (!File.Exists (user_dirs_path)) {
				return Path.Combine (home_dir, fallback);
			}

			try {
				using (StreamReader reader = new StreamReader (user_dirs_path)) {
					string line;
					while ((line = reader.ReadLine ()) != null) {
						line = line.Trim ();
						int delim_index = line.IndexOf ('=');
						if (delim_index > 8 && line.Substring (0, delim_index) == key) {
							string path = line.Substring (delim_index + 1).Trim ('"');
							bool relative = false;

							if (path.StartsWith ("$HOME/")) {
								relative = true;
								path = path.Substring (6);
							} else if (path.StartsWith ("~")) {
								relative = true;
								path = path.Substring (1);
							} else if (!path.StartsWith ("/")) {
								relative = true;
							}
							return relative ? Path.Combine (home_dir, path) : path;
						}
					}
				}
			} catch (FileNotFoundException) {
			}
			return Path.Combine (home_dir, fallback);
		}

		public static string Combine (string first, params string [] components)
		{
			if (String.IsNullOrEmpty (first)) {
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

		public static string ApplicationData {
			get { 
				return Combine (ReadXdgUserDir ("XDG_CONFIG_HOME", ".config"), "gnome-do");
			}
		}

		public static string UserHome {
			get {
				string home = Environment.GetEnvironmentVariable ("HOME");
				if (string.IsNullOrEmpty (home))
					home = Environment.GetFolderPath (Environment.SpecialFolder.Personal);
				return home;
			}
		}

		public static string Temp {
			get { return Combine (Paths.ApplicationData, "tmp"); }
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

		public static string UserData {
			get {
				return Combine (ReadXdgUserDir ("XDG_DATA_HOME", ".local/share"), "gnome-do");
			}
		}

		public static string UserPlugins {
			get { return Combine (UserData, "plugins-" + Version); }
		}

		public static string[] SystemData {
			get {
				string envVal;
				string [] dirs;
				
				envVal = Environment.GetEnvironmentVariable ("XDG_DATA_DIRS");
				if (string.IsNullOrEmpty (envVal))
					envVal = "/usr/local/share:/usr/share";

				dirs = envVal.Split (':');

				for (int i = 0; i < dirs.Length; ++i)
					dirs [i] = Combine (dirs [i], "gnome-do");

				return dirs;
			}
		}

		public static string [] SystemPlugins {
			get {
				string [] dirs = SystemData;
				for (int i = 0; i < dirs.Length; ++i)
					dirs [i] = Combine (dirs [i], "plugins");
				return dirs;
			}
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



