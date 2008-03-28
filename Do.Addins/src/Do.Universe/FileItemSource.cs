/* FileItemSource.cs
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

using Do.Addins;

namespace Do.Universe {

	/// <summary>
	/// Indexes files recursively starting in a specific directory.
	/// </summary>
	public class FileItemSource : IItemSource {

		List<IItem> items;
		bool include_hidden;
		ICollection<DirectoryLevelPair> dirs;

		struct DirectoryLevelPair {
			public string Directory;
			public int Levels;
			
			public DirectoryLevelPair (string dir, int levels)
			{
				Directory = dir.Replace ("~",
				   Environment.GetFolderPath (Environment.SpecialFolder.Personal));
				Levels = levels;
			}
		}
		
		static readonly string ConfigFile;
		
		static readonly DirectoryLevelPair[] DefaultDirectories = {
			new DirectoryLevelPair ("/home",		   1),
			new DirectoryLevelPair (Paths.UserHome,    1),
			new DirectoryLevelPair (Desktop,		   1),
			new DirectoryLevelPair (Documents,	    	3),
		};
		
		static FileItemSource ()
		{
			Gnome.Vfs.Vfs.Initialize ();
			ConfigFile = Paths.Combine (Paths.ApplicationData, "FileItemSource.config");
		}
		
		static ICollection<DirectoryLevelPair> LoadSavedDirectoryLevelPairs ()
		{
			List<DirectoryLevelPair> dirs;

			if (!File.Exists (ConfigFile)) {
				SaveDirectoryLevelPairs (DefaultDirectories);
				return DefaultDirectories;
			}
			
			dirs = new List<DirectoryLevelPair> ();
			if (File.Exists (ConfigFile)) {
				try {
					foreach (string line in File.ReadAllLines (ConfigFile)) {
						string[] parts;
						if (line.Trim ().StartsWith ("#")) continue;
						parts = line.Trim ().Split (':');
						if (parts.Length != 2) continue;
						dirs.Add (new DirectoryLevelPair (parts[0].Trim (),
						          int.Parse (parts[1].Trim ())));
					}
				} catch (Exception e) {
					Console.Error.WriteLine ("Error reading FileItemSource config file {0}: {1}", ConfigFile, e.Message);
				}
			} 
			return dirs;
		}
		
		static void SaveDirectoryLevelPairs (ICollection<DirectoryLevelPair> dirs)
		{
			try {
				if (!Directory.Exists (Path.GetDirectoryName (ConfigFile)))
					Directory.CreateDirectory (Path.GetDirectoryName (ConfigFile));
				foreach (DirectoryLevelPair pair in dirs) {
					File.AppendAllText (ConfigFile, string.Format ("{0}: {1}\n", pair.Directory, pair.Levels)); 
				}
			} catch (Exception e) {
				Console.Error.WriteLine ("Error saving FileItemSource config file {0}: {1}", ConfigFile, e.Message);
			}
		}
		
		public Type [] SupportedItemTypes
		{
			get {
				return new Type[] {
					typeof (IFileItem),
				};
			}
		}
		
		public FileItemSource ()
		{
			dirs = LoadSavedDirectoryLevelPairs ();
			items = new List<IItem> ();
			include_hidden = false;
			UpdateItems ();
		}
		
		public string Name
		{
			get { return "Directory Scanner"; }
		}
		
		public string Description
		{
			get {
				return string.Format ("Catalog files in user-specified directories.");
			}
		}
		
		public string Icon
		{
			get { return "folder"; }
		}
		
		public ICollection<IItem> Items
		{
			get { return items; }
		}
		
		public ICollection<IItem> ChildrenOfItem (IItem item)
		{
			List<IItem> children;
			IFileItem fi;
			
			fi = item as IFileItem;
			children = new List<IItem> ();
			if (FileItem.IsDirectory (fi)) {
				foreach (string path in
					Directory.GetFileSystemEntries (fi.Path)) {
					children.Add (new FileItem (path));
				}
			}
			return children;
		}
		
		public void UpdateItems ()
		{
			items.Clear ();
			foreach (DirectoryLevelPair dir in dirs) {
				ReadItems (dir.Directory, dir.Levels);
			}
		}
		
		/// <summary>
		/// Create items for files found in a given directory. Recurses a
		/// given number of levels deep into nested directories.
		/// </summary>
		/// <param name="dir">
		/// A <see cref="System.String"/> containing the absolute path
		/// to the directory to read FileItems from.
		/// </param>
		/// <param name="levels">
		/// A <see cref="System.Int32"/> specifying the number of levels
		/// of nested directories to explore.
		/// </param>
		protected virtual void ReadItems (string dir, int levels)
		{
			string[] files;
			string[] directories;
			FileItem item;
			
			if (levels == 0) {
				return;
			}		
			try {
				files = Directory.GetFiles (dir);
				directories = Directory.GetDirectories (dir);
			} catch {
				return;
			}
			foreach (string file in files) {
				// Ignore system/hidden files.
				if (!include_hidden && FileItem.IsHidden (file)) continue;

				item = new FileItem (file);
				items.Add (item);
			}
			foreach (string directory in directories) {
				if (!include_hidden && FileItem.IsHidden (directory)) continue;

				item = new FileItem (directory);
				items.Add (item);
				ReadItems (directory, levels - 1);
			}
		}

		public static string Music {
			get { return Paths.ReadXdgUserDir ("XDG_MUSIC_DIR", "Music"); }
		}

		public static string Pictures {
			get { return Paths.ReadXdgUserDir ("XDG_PICTURES_DIR", "Pictures"); }
		}

		public static string Videos {
			get { return Paths.ReadXdgUserDir ("XDG_VIDEOS_DIR", "Videos"); }
		}

		public static string Desktop {
			get { return Paths.ReadXdgUserDir ("XDG_DESKTOP_DIR", "Desktop"); }
		}

		public static string Downloads {
			get { return Paths.ReadXdgUserDir ("XDG_DOWNLOAD_DIR", "Downloads"); }
		}

		public static string Documents {
			get { return Paths.ReadXdgUserDir ("XDG_DOCUMENTS_DIR", "Documents"); }
		}
	}
}
