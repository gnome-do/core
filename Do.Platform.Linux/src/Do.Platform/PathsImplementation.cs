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
using System.Linq;
using System.Collections.Generic;

using Mono.Unix;

namespace Do.Platform {

	public class PathsImplementation : Paths.Implementation
	{

		#region Paths.Implementation
		
		public string ApplicationData {
			get { 
				return Paths.Combine (ReadXdgUserDir ("XDG_CONFIG_HOME", ".config"), "gnome-do");
			}
		}

		public string UserHome {
			get {
				string home = Environment.GetEnvironmentVariable ("HOME");
				if (string.IsNullOrEmpty (home))
					home = Environment.GetFolderPath (Environment.SpecialFolder.Personal);
				return home;
			}
		}

		public string UserData {
			get {
				return Paths.Combine (ReadXdgUserDir ("XDG_DATA_HOME", ".local/share"), "gnome-do");
			}
		}

		public string UserDesktop {
			get {
				return ReadXdgUserDir ("XDG_DESKTOP_DIR", "Desktop");
			}
		}

		public IEnumerable<string> SystemPlugins {
			get {
				return SystemData.Select (dir => Paths.Combine (dir, "plugins"));
			}
		}

		#endregion
		
		public static IEnumerable<string> SystemData {
			get {
				string envVal;
				
				envVal = Environment.GetEnvironmentVariable ("XDG_DATA_DIRS");
				if (string.IsNullOrEmpty (envVal))
					envVal = "/usr/local/share:/usr/share";

				return envVal.Split (':').Select (dir => Paths.Combine (dir, "gnome-do"));
			}
		}
		
		public static string ReadXdgUserDir (string key, string fallback)
		{
			string home_dir, config_dir, env_path, user_dirs_path;

			home_dir = Paths.UserHome;
			config_dir = System.Environment.GetFolderPath (System.Environment.SpecialFolder.ApplicationData);

			env_path = System.Environment.GetEnvironmentVariable (key);
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
	}
}



