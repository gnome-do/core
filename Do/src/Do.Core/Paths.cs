/* Paths.cs
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
using System.Collections.Generic;

namespace Do.Core
{
	
	internal static class Paths
	{
		const string ApplicationDirectory = "gnome-do";
		const string PluginsDirectory = "plugins";

		//// <value>
		/// Directory where Do saves its Mono.Addins repository cache.
		/// </value>
		public static string UserPluginsDirectory {
			get {
				string userData =
					Environment.GetFolderPath (Environment.SpecialFolder.LocalApplicationData);
				string pluginDirectory
					= string.Format ("{0}-{1}", PluginsDirectory, AssemblyInfo.DisplayVersion);
				return userData.Combine (ApplicationDirectory, pluginDirectory);
			}
		}

		//// <value>
		/// Directories where Do looks for Mono.Addins repositories.
		/// </value>
		public static IEnumerable<string> SystemPluginDirectories {
			get {
				yield return AppDomain.CurrentDomain.BaseDirectory.Combine (PluginsDirectory);

				string systemData =
					Environment.GetFolderPath (Environment.SpecialFolder.CommonApplicationData);
				yield return systemData.Combine (ApplicationDirectory, PluginsDirectory);
			}
		}
		
	}
}
