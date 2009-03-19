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
		static IEnumerable<string> desktop_file_directories;
		
		IEnumerable<Item> app_items;
		
		static ApplicationItemSource ()
		{
			desktop_file_directories = ReadXdgDataDirs ();
		}
		
		public ApplicationItemSource ()
		{
			app_items = Enumerable.Empty<Item> ();
		}

		public override IEnumerable<Type> SupportedItemTypes {
			get { yield return typeof (ApplicationItem); }
		}

		public override string Name {
			get { return Catalog.GetString ("Applications"); }
		}

		public override string Description {
			get { return Catalog.GetString ("Finds applications in many locations."); }
		}

		public override string Icon {
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
		static IEnumerable<ApplicationItem> LoadDesktopFiles (string dir)
		{
			Queue<string> queue;
			List<ApplicationItem> apps;
			
			if (!Directory.Exists (dir))
				return Enumerable.Empty<ApplicationItem> ();
			
			queue = new Queue<string> ();
			queue.Enqueue (dir);
			
			apps = new List<ApplicationItem> ();
				
			while (queue.Count > 0) {
				dir = queue.Dequeue ();
				foreach (string d in Directory.GetDirectories (dir))
					queue.Enqueue (d);
				
				apps.AddRange (Directory.GetFiles (dir, "*.desktop")
					.Select (file => ApplicationItem.MaybeCreateFromDesktopItem (file))
					.Where (app => app != null && app.IsAppropriateForCurrentDesktop && (show_hidden || !app.NoDisplay))
				);
			}
			
			return apps;
		}
		
		public override void UpdateItems ()
		{
			app_items = desktop_file_directories
				.Select (dir => dir.Replace ("~", Environment.GetFolderPath (Environment.SpecialFolder.Personal)))
				.SelectMany (dir => LoadDesktopFiles (dir))
				.Cast<Item> ()
				.ToArray ();
		}

		public override IEnumerable<Item> Items {
			get { return app_items; }
		}
		
		static IEnumerable<string> ReadXdgDataDirs ()
		{
			string home, envPath;
			
			const string appDirSuffix = "applications";
			string [] xdgVars = new [] {"XDG_DATA_HOME", "XDG_DATA_DIRS"};
			
			home = Environment.GetFolderPath (Environment.SpecialFolder.Personal);
				
			foreach (string xdgVar in xdgVars) {
				envPath = Environment.GetEnvironmentVariable (xdgVar);
						
				if (string.IsNullOrEmpty (envPath)) {
					if (xdgVar == "XDG_DATA_HOME") {
						yield return (new [] {home, ".local/share", appDirSuffix}.Aggregate (Path.Combine));
					} else if (xdgVar == "XDG_DATA_DIRS") {
						yield return Path.Combine ("/usr/local/share/", appDirSuffix);
						yield return Path.Combine ("/usr/share/", appDirSuffix);
					}
				} else {
					foreach (string dir in envPath.Split (':'))
						yield return Path.Combine (dir, appDirSuffix);
				}
			}
		}
	}
}
