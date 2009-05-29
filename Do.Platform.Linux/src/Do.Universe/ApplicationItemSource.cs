// ApplicationItemSource.cs
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
			desktop_file_directories = GetDesktopFileDirectories ();
		}
		
		public ApplicationItemSource ()
		{
			app_items = Enumerable.Empty<Item> ();
		}

		public override IEnumerable<Type> SupportedItemTypes {
			get { 
				yield return typeof (ApplicationItem); 
				yield return typeof (CategoryItem);
			}
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
		IEnumerable<ApplicationItem> LoadDesktopFiles (string dir)
		{
			return GetDesktopFiles ()
				.Where (ShouldUseDesktopFile)
				.Select (f => ApplicationItem.MaybeCreateFromDesktopItem (f)).Where (a => a != null)
				.Where (ShouldUseApplicationItem);
		}
		
		IEnumerable<string> GetDesktopFiles ()
		{
			return desktop_file_directories
				.Where (d => Directory.Exists (d))
				.SelectMany (d => GetDesktopFiles (d));
		}
		
		IEnumerable<string> GetDesktopFiles (string parent)
		{
			IEnumerable<string> baseFiles      = Directory.GetFiles (parent, "*.desktop");
			IEnumerable<string> recursiveFiles = Directory.GetDirectories (parent).SelectMany (d => GetDesktopFiles (d));
			return baseFiles.Concat (recursiveFiles);
		}
		
		IEnumerable<CategoryItem> LoadCategoryItems (ApplicationItem appItem)
		{
			return appItem.Categories
				.Where (c => !CategoryItem.ContainsCategory (c))
				.Select (c => CategoryItem.GetCategoryItem (c));
		}
		
		bool ShouldUseDesktopFile (string path)
		{
			return !path.Contains ("screensavers");
		}
		
		bool ShouldUseApplicationItem (ApplicationItem app)
		{
			return app.IsAppropriateForCurrentDesktop && (show_hidden || !app.NoDisplay);
		}
		
		public override void UpdateItems ()
		{
			IEnumerable<ApplicationItem> appItems = desktop_file_directories
				.SelectMany (dir => LoadDesktopFiles (dir));
			
			IEnumerable<CategoryItem> categoryItems = appItems
				.SelectMany (a => LoadCategoryItems (a));

			app_items = appItems
				.Cast<Item> ()
				.Concat (categoryItems.Cast<Item> ())
				.Distinct ()
				.ToArray ();
		}

		public override IEnumerable<Item> Items {
			get { return app_items; }
		}
		
		public override IEnumerable<Item> ChildrenOfItem (Item item)
		{
			if (item is CategoryItem) {
				CategoryItem catItem = item as CategoryItem;
				return app_items
					.Where (a => a is ApplicationItem)
					.Where (a => (a as ApplicationItem).Categories.Contains (catItem.Category));
			} else {
				return Enumerable.Empty<Item> ();
			}
		}
		
		/// <summary>
		/// Return list of directories to scan for .desktop files.
		/// </summary>
		/// <comment>
		/// Returns absolute paths.
		/// Implements XDG data directory specification.
		/// </comment>
		/// <returns>
		/// A <see cref="IEnumerable"/>
		/// </returns>
		static IEnumerable<string> GetDesktopFileDirectories ()
		{
			return new [] {
				// These are XDG variables...
				"XDG_DATA_HOME",
				"XDG_DATA_DIRS"
			}.SelectMany (v => GetXdgEnvironmentPaths (v));
		}
		
		static IEnumerable<string> GetXdgEnvironmentPaths (string xdgVar)
		{
			string envPath = Environment.GetEnvironmentVariable (xdgVar);
			
			if (string.IsNullOrEmpty (envPath)) {
				switch (xdgVar) {
				case "XDG_DATA_HOME":
					yield return Path.Combine (
						Environment.GetFolderPath (Environment.SpecialFolder.Personal),
						".local/share/applications"
					);
					break;
				case "XDG_DATA_DIRS":
					yield return "/usr/local/share/applications";
					yield return "/usr/share/applications";
					break;
				}
			} else {
				foreach (string dir in envPath.Split (':')) {
					yield return Path.Combine (dir, "applications");
				}
			}
		}
	}
}
