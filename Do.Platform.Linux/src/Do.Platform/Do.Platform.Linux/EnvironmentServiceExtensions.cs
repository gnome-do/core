// EnvironmentServieExtensions
//  
//  GNOME Do is the legal property of its developers. Please refer to the
//  COPYRIGHT file distributed with this
//  source distribution.
//  
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//  
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//  
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>

using System;
using System.IO;

using Do.Platform;

namespace Do.Platform.Linux
{
	public static class EnvironmentServiceExtensions
	{
		/// <summary>
		/// Find the path of the directory that maps to the given XDG dir
		/// if the xdg variable is not set, return null
		/// </summary>
		/// <param name="key">
		/// A <see cref="System.String"/> XDG directory variable name
		/// </param>
		/// <returns>
		/// A <see cref="System.String"/> path for the XDG directory env. variable
		/// </returns>
		public static string MaybePathForXdgVariable (this IEnvironmentService envService, string key)
		{
			return PathForXdgVariable (envService, key, null);
		}
		
		/// <summary>
		/// Find the path of the directory that maps to the given XDG dir
		/// if the xdg variable is not set, use the fallback value passed in
		/// </summary>
		/// <param name="key">
		/// A <see cref="System.String"/> XDG directory variable name
		/// </param>
		/// <param name="fallback">
		/// A <see cref="System.String"/> default XDG directory name to fallback
		/// on if the variable is not set.
		/// </param>
		/// <returns>
		/// A <see cref="System.String"/> path for the XDG directory env. variable
		/// </returns>
		public static string PathForXdgVariable (this IEnvironmentService envService, string key, string fallback)
		{
			string homeDir, configDir, envPath, userDirsPath;

			homeDir = Environment.GetFolderPath (Environment.SpecialFolder.Personal);
			configDir = Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData);

			envPath = Environment.GetEnvironmentVariable (key);
			if (!String.IsNullOrEmpty (envPath)) {
				return envPath;
			}

			userDirsPath = Path.Combine (configDir, "user-dirs.dirs");

			if (File.Exists (userDirsPath)) {
				using (StreamReader reader = new StreamReader (userDirsPath)) {
					string line;
					while ((line = reader.ReadLine ()) != null) {
						line = line.Trim ();
						int delimIndex = line.IndexOf ('=');
						if (delimIndex > 8 && line.Substring (0, delimIndex) == key) {
							string path = line.Substring (delimIndex + 1).Trim ('"');
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
							return relative ? Path.Combine (homeDir, path) : path;
						}
					}
				}
			}

			return fallback == null 
				? null
				: Path.Combine (homeDir, fallback);
		}
	}
}
