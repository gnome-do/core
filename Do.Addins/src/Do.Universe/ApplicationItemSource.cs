/* ApplicationItemSource.cs
 *
 * GNOME Do is the legal property of its developers. Please refer to the
 * COPYRIGHT file distributed with this
 * source distribution.
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
using System.Collections.Generic;
using System.IO;

namespace Do.Universe
{
	public class ApplicationItemSource : IItemSource
	{
		
		/// <summary>
		/// Locations to search for .desktop files.
		/// </summary>
		public static readonly string[] DesktopFilesDirectories = {
			"/usr/share/applications",
			"/usr/share/applications/kde",
			"/usr/share/gdm/applications",
			"/usr/local/share/applications",
		};
		
		private List<IItem> apps;

		static ApplicationItemSource ()
		{
			Gnome.Vfs.Vfs.Initialize ();
		}
		
		public ApplicationItemSource ()
		{
			apps = new List<IItem> ();			
			UpdateItems ();
		}
		
		public Type[] SupportedItemTypes {
			get { return new Type[] {
					typeof (ApplicationItem),
				};
			}
		}
		
		public string Name {
			get { return "Applications"; }
		}
		
		public string Description {
			get { return "Finds applications in many locations."; }
		}
		
		public string Icon {
			get { return "gnome-applications"; }
		}
		
		/// <summary>
		/// Given an absolute path to a directory, scan that directory for
		/// .desktop files, creating an ApplicationItem for each desktop file
		/// found and adding the ApplicationItem to the list of ApplicationItems.
		/// </summary>
		/// <param name="desktop_files_dir">
		/// A <see cref="System.String"/> containing an absolute path to a directory
		/// where .desktop files can be found.
		/// </param>
		private void LoadDesktopFiles (string desktop_files_dir)
		{
			ApplicationItem app;
			
			if (!Directory.Exists (desktop_files_dir)) return;
			foreach (string filename in Directory.GetFiles (desktop_files_dir)) {
				if (!filename.EndsWith (".desktop")) continue;
				
				try {
					app = new ApplicationItem (filename);
				} catch {
					continue;
				}
				apps.Add(app);
			}
			
		}
		
		public void UpdateItems ()
		{
			apps.Clear ();
			foreach (string dir in DesktopFilesDirectories) {
				LoadDesktopFiles (dir);
			}
		}
		
		public ICollection<IItem> Items {
			get { return apps; }
		}
		
		public ICollection<IItem> ChildrenOfItem (IItem item) {
			return null;
		}
		
	}
}
