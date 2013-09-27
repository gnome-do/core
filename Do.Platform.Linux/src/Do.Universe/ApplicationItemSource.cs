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

	public class ApplicationItemSource : DynamicItemSource {

		const bool show_hidden = false;
		Dictionary<string, ApplicationItem> app_items = new Dictionary<string, ApplicationItem> ();
		List<CategoryItem> categories = new List<CategoryItem> ();

		static IEnumerable<string> desktop_file_directories;
		List<FileSystemWatcher> directoryMonitors = new List<FileSystemWatcher> ();

		static ApplicationItemSource ()
		{
			desktop_file_directories = GetDesktopFileDirectories ();
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
		IEnumerable<KeyValuePair<string, ApplicationItem>> LoadDesktopFiles (string dir)
		{
			return GetDesktopFiles (dir)
				.Where (ShouldUseDesktopFile)
				.Select (f => new KeyValuePair<string, ApplicationItem> (f, ApplicationItem.MaybeCreateFromDesktopItem (f)))
				.Where (a => a.Value != null)
				.Where (a => a.Value.ShouldShow);
		}
		
		IEnumerable<string> GetDesktopFiles ()
		{
			return desktop_file_directories
				.Where (Directory.Exists)
				.SelectMany (GetDesktopFiles);
		}
		
		IEnumerable<string> GetDesktopFiles (string parent)
		{
			IEnumerable<string> baseFiles      = Enumerable.Empty<string> ();
			IEnumerable<string> recursiveFiles = Enumerable.Empty<string> ();
			// done separately so failures allow other directories to recurse
			try {
				baseFiles = Directory.GetFiles (parent, "*.desktop");
			} catch (Exception) { }
			try {
				recursiveFiles = Directory.GetDirectories (parent).SelectMany (GetDesktopFiles);
			} catch (Exception) { }
			return baseFiles.Concat (recursiveFiles);
		}
		
		IEnumerable<CategoryItem> LoadCategoryItems (ApplicationItem appItem)
		{
			return appItem.Categories
				.Where (c => !CategoryItem.ContainsCategory (c))
				.Select (CategoryItem.GetCategoryItem);
		}
		
		bool ShouldUseDesktopFile (string path)
		{
			return !path.Contains ("screensavers");
		}
		
		override protected void Enable ()
		{
			ItemsAvailableEventArgs eventArgs = new ItemsAvailableEventArgs ();
			lock (app_items) {
				foreach (var directory in desktop_file_directories.Where (dir => Directory.Exists (dir))) {
					var monitor = new FileSystemWatcher (directory, "*.desktop");
					monitor.Created += OnFileCreated;
					monitor.Deleted += OnFileDeleted;
					monitor.Renamed += OnFileRenamed;
					monitor.Error += OnWatcherError;
					monitor.EnableRaisingEvents = true;
					directoryMonitors.Add (monitor);
					Log<ApplicationItemSource>.Debug ("Watching directory {0} for application changes.", directory);
				}
				foreach (var fileItemPair in desktop_file_directories.SelectMany (dir => LoadDesktopFiles (dir))) {
					var previousMatch = app_items.FirstOrDefault (pair => pair.Value == fileItemPair.Value);
					if (previousMatch.Key == null && previousMatch.Value == null) {
						app_items.Add (fileItemPair.Key, fileItemPair.Value);
					} else if (fileItemPair.Key != previousMatch.Key){
						Log.Debug ("Desktop file {0} hides previous file {1}", fileItemPair.Key, previousMatch.Key);
						app_items.Remove (previousMatch.Key);
						app_items.Add (fileItemPair.Key, fileItemPair.Value);
					}
				}
				eventArgs.newItems = app_items.Values.Cast<Item> ().ToList ();

				categories = app_items.SelectMany (pair => LoadCategoryItems (pair.Value)).Distinct ().ToList ();
				eventArgs.newItems = eventArgs.newItems.Concat (categories.ToArray ());
			}
			RaiseItemsAvailable (eventArgs);
		}

		override protected void Disable ()
		{
			foreach (var watcher in directoryMonitors) {
				watcher.Dispose ();
			}
			directoryMonitors.Clear ();
			app_items.Clear ();
		}

		void OnWatcherError (object sender, ErrorEventArgs e)
		{
			Log<ApplicationItemSource>.Error ("Error in directory watcher: {0}", e.GetException ().Message);
		}

		void OnFileDeleted (object sender, FileSystemEventArgs e)
		{
			Item disappearingItem;
			lock (app_items) {
				Log<ApplicationItemSource>.Debug ("Deskop file removed: {0}", e.FullPath);
				if (!app_items.ContainsKey (e.FullPath)) {
					Log.Error ("Desktop file {0} deleted, but not found in Universe", e.FullPath);
					// FIXME: Should this throw an exception?
					return;
				}
				disappearingItem = app_items[e.FullPath];
				app_items.Remove (e.FullPath);
			}
			RaiseItemsUnavailable (new ItemsUnavailableEventArgs () { unavailableItems = new Item[] { disappearingItem }});
		}

		void OnFileCreated (object sender, FileSystemEventArgs e)
		{
			Log<ApplicationItemSource>.Debug ("New Desktop file found: {0}", e.FullPath);
			var newItem = ApplicationItem.MaybeCreateFromDesktopItem (e.FullPath);
			if (newItem == null) {
				Log.Error ("Found new Desktop file {0} but unable to create an item in the Universe", e.FullPath);
				return;
			}
			lock (app_items) {
				if (app_items.ContainsKey (e.FullPath)) {
					Log.Error ("Attempting to add duplicate ApplicationItem {0} to Universe", e.FullPath);
					return;
				}
				app_items[e.FullPath] = newItem;
			}
			RaiseItemsAvailable (new ItemsAvailableEventArgs () { newItems = new Item[] { newItem }});
		}

		void OnFileRenamed (object sender, RenamedEventArgs e)
		{
			Item disappearingItem = null;
			ApplicationItem newItem = null;
			lock (app_items) {
				if (app_items.ContainsKey (e.OldFullPath)) {
					Log<ApplicationItemSource>.Debug ("Desktop file {0} moved away", e.OldFullPath);
					disappearingItem = app_items[e.OldFullPath];
					app_items.Remove (e.OldFullPath);
				}
				if (e.FullPath.EndsWith (".desktop", StringComparison.Ordinal)) {
					Log<ApplicationItemSource>.Debug ("Desktop file {0} moved into watched directory", e.FullPath);
					newItem = ApplicationItem.MaybeCreateFromDesktopItem (e.FullPath);
					if (newItem == null) {
						Log.Error ("Found new Desktop file {0} but unable to create an item in the Universe", e.FullPath);
					} else {
						app_items [e.FullPath] = newItem;
					}
				}
			}
			if (disappearingItem != null) {
				RaiseItemsUnavailable (new ItemsUnavailableEventArgs () { unavailableItems = new Item[] { disappearingItem }});
			}
			if (newItem != null) {
				RaiseItemsAvailable (new ItemsAvailableEventArgs () { newItems = new Item[] { newItem }});
			}
		}

		public override IEnumerable<Item> ChildrenOfItem (Item item)
		{
			if (item is CategoryItem) {
				CategoryItem catItem = item as CategoryItem;
				return app_items.Values
					.Where (a => a is ApplicationItem)
					.Where (a => (a as ApplicationItem).Categories.Contains (catItem.Category))
					.Cast<Item> ();
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
				"XDG_DATA_DIRS",
				"XDG_DATA_HOME"
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
					yield return "/usr/share/applications";
					yield return "/usr/local/share/applications";
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
