/* DirectoryFileItemSource.cs
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
	/// <summary>
	/// Indexes files recursively starting in a specific directory.
	/// </summary>
	public class DirectoryFileItemSource : IItemSource
	{
		List<IItem> items;
		int levels;
		string path;
		bool include_hidden;

		static DirectoryFileItemSource ()
		{
			Gnome.Vfs.Vfs.Initialize ();
		}
		
		public Type[] SupportedItemTypes {
			get { return new Type[] {
					typeof (FileItem),
				};
			}
		}

		public DirectoryFileItemSource (string path, int levels)
		{
			string home;
			
			home = System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal);
			path = path.Replace ("~", home);
			
			this.path = path;
			this.levels = levels;
			this.items = new List<IItem> ();
			this.include_hidden = false;
			UpdateItems ();
		}
		
		public string Name {
			get { return path; }
		}
		
		public string Description {
			get { return string.Format("Finds items in directory '{0}'", path); }
		}
		
		public string Icon {
			get { return "folder"; }
		}
		
		public ICollection<IItem> Items {
			get { return items; }
		}
		
		public ICollection<IItem> ChildrenOfItem (IItem item) {
			return null;
		}
		
		public void UpdateItems ()
		{
			ReadItems (path, levels);
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
			
			if (levels == 0) return;
			try {
				files = Directory.GetFiles (dir);
				directories = Directory.GetDirectories (dir);
			} catch (DirectoryNotFoundException) {
				return;
			}
			foreach (string file in files) {
				// Ignore system/hidden files.
				if (!include_hidden && Path.GetFileName (file).StartsWith (".")) {
					continue;
				}
				item = FileItem.Create (file);
				items.Add (item);
			}
			foreach (string directory in directories) {
				if (!include_hidden && Path.GetFileName (directory).StartsWith (".")) {
					continue;
				}
				item = FileItem.Create (directory);
				items.Add (item);
				ReadItems (directory, levels - 1);
			}
		}
	}
}
