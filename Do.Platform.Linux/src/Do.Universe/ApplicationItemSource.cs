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
using System.Linq;
using System.Collections.Generic;

using Do.Platform;
using Do.Universe;

using Mono.Unix;

namespace Do.Universe.Linux {

	public class ApplicationItemSource : ItemSource {

		const bool show_hidden = false;

		private IEnumerable<Item> app_items;

		/// <summary>
		/// Locations to search for .desktop files.
		/// </summary>
		static IEnumerable<String> DesktopFilesDirectories {
			get {
				return new [] {
					Environment.GetFolderPath (Environment.SpecialFolder.Desktop),
					"~/.local/share/applications",
					"~/.local/share/applications/wine",
					"~/.local/share/applications/wine/Programs",
					"/usr/share/applications",
					"/usr/share/applications/kde",
					"/usr/share/applications/kde4",
					"/usr/share/gdm/applications",
					"/usr/local/share/applications",
				};
			}
		}
		
		public ApplicationItemSource ()
		{
			app_items = Enumerable.Empty<Item> ();
		}

		public override IEnumerable<Type> SupportedItemTypes {
			get { yield return typeof (ApplicationItem); }
		}

		protected override string Name {
			get { return Catalog.GetString ("Applications"); }
		}

		protected override string Description {
			get { return Catalog.GetString ("Finds applications in many locations."); }
		}

		protected override string Icon {
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
		private static IEnumerable<ApplicationItem> LoadDesktopFiles (string dir)
		{
			if (!Directory.Exists (dir))
				return Enumerable.Empty<ApplicationItem> ();

			return Directory.GetFiles (dir, "*.desktop")
				.Select (file => ApplicationItem.CreateFromDesktopItem (file))
				.Where (app => app != null &&
								app.IsAppropriateForCurrentDesktop &&
								(show_hidden || !app.NoDisplay));
		}

		protected override void UpdateItems ()
		{
			app_items = DesktopFilesDirectories
				.Select (dir => dir.Replace ("~", Environment.GetFolderPath (Environment.SpecialFolder.Personal)))
				.SelectMany (dir => LoadDesktopFiles (dir))
				.Cast<Item> ()
				.ToArray ();
		}

		protected override IEnumerable<Item> Items {
			get { return app_items; }
		}

	}
}
