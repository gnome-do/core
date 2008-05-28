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
using System.IO;
using System.Collections.Generic;

namespace Do.Universe {

	public class ApplicationItemSource : IItemSource {

		private Dictionary<string,IItem> apps;

		bool show_hidden = false;
		
		/// <summary>
		/// Locations to search for .desktop files.
		/// </summary>
		static string [] DesktopFilesDirectories {
			get {
				return new string [] {
					"/usr/share/applications",
					"/usr/share/applications/kde",
					"/usr/share/gdm/applications",
					"/usr/local/share/applications",
					"~/.local/share/applications",
					Desktop,
				};
			}
		}

		static string Desktop {
			get {
				return Paths.ReadXdgUserDir ("XDG_DESKTOP_DIR", "Desktop");
			}
		}

		public ApplicationItemSource ()
		{
			apps = new Dictionary<string,IItem> ();
		}

		public Type [] SupportedItemTypes {
			get {
				return new Type [] {
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
		/// found and adding the ApplicationItem to the list of
		/// ApplicationItems.
		/// </summary>
		/// <param name="dir">
		/// A <see cref="System.String"/> containing an absolute path to a
		/// directory
		/// where .desktop files can be found.
		/// </param>
		private void LoadDesktopFiles (string dir)
		{
			if (!Directory.Exists (dir)) return;
			foreach (string file in Directory.GetFiles (dir, "*.desktop")) {
                ApplicationItem app;

                if (apps.ContainsKey (file)) continue;
				try {
					app = new ApplicationItem (file);
				} catch {
					continue;
				}
				if (!app.Hidden || show_hidden)
					apps [file] = app;
			}
		}

		public void UpdateItems ()
		{
			// Updating is turned off because it uses a ridiculous amount of memory.
			if (apps.Count > 0) return;
			
			foreach (string dir in DesktopFilesDirectories) {
				LoadDesktopFiles (dir.Replace ("~", Paths.UserHome));
			}
		}

		public ICollection<IItem> Items {
			get { return apps.Values; }
		}

		public ICollection<IItem> ChildrenOfItem (IItem item)
		{
			return null;
		}
	}
}
