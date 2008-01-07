/* FirefoxBookmarkItemSource.cs
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

using Gnome;
using Gnome.Vfs;

namespace Do.Universe
{
	public class FirefoxBookmarkItemSource : IItemSource
	{
		const string BeginProfileName = "Path=";
		const string BeginDefaultProfile = "Name=default";
		const string BeginURL = "<DT><A HREF=\"";
		const string EndURL = "\"";
		const string BeginShortcut = "SHORTCUTURL=\"";
		const string EndShortcut = "\"";
		const string BeginName = "\">";
		const string EndName = "</A>";
		
		List<IItem> bookmarks;
		
		/// <summary>
		/// Initialize the item source.
		/// </summary>
		public FirefoxBookmarkItemSource ()
		{
			bookmarks = new List<IItem> ();
			UpdateItems ();
		}
		
		public Type[] SupportedItemTypes
		{
			get {
				return new Type[] {
					typeof (BookmarkItem),
				};
			}
		}
		
		public string Name
		{
			get { return "Firefox Bookmarks"; }
		}
		
		public string Description
		{
			get { return "Finds Firefox bookmarks in your default profile."; }
		}
		
		public string Icon
		{
			get { return "www"; }
		}
		
		public ICollection<IItem> Items
		{
			get { return bookmarks; }
		}
		
		public ICollection<IItem> ChildrenOfItem (IItem item)
		{
			return null;
		}
		
		public void UpdateItems ()
		{
			foreach (IItem item in ReadBookmarksFromFile (GetFirefoxBookmarkFilePath ()))
				bookmarks.Add (item);
		}
		
		/// <summary>
		/// Looks in the firefox profiles file (~/.mozilla/firefox/profiles.ini)
		/// for the name of the default profile, and returns the path to the
		/// default profile.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> containing the absolute path to the
		/// bookmarks.html file of the default firefox profile for the current user.
		/// </returns>
		public static string GetFirefoxBookmarkFilePath ()
		{
			string home, path, profile;
			StreamReader reader;

			profile = null;
			home = System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal);
			path = System.IO.Path.Combine (home, ".mozilla/firefox/profiles.ini");
			try {
				reader = System.IO.File.OpenText (path);
			} catch {
				return null;
			}
			
			bool got_default = false;
			for (string line = reader.ReadLine (); line != null; line = reader.ReadLine ()) {
				if (got_default && line.StartsWith (BeginProfileName)) {
					line = line.Trim ();
					line = line.Substring (BeginProfileName.Length);
					profile = line;
					break;
				}
				else if (line.StartsWith (BeginDefaultProfile)) {
					got_default = true;
				}
			}
			reader.Close ();
			
			if (profile == null) {
				return null;
			}
			path = System.IO.Path.Combine (home, ".mozilla/firefox");
			path = System.IO.Path.Combine (path, profile);
			path = System.IO.Path.Combine (path, "bookmarks.html");
			return path;
		}
		
		/// <summary>
		/// Given a bookmarks file, create a BookmarkItem for each bookmark found
		/// in the file, returning a collection of BookmarkItems created.
		/// </summary>
		/// <param name="file">
		/// A <see cref="System.String"/> containing the absolute path to a Firefox
		/// bookmarks.html file (e.g. the path returned by GetFirefoxBookmarkFilePath).
		/// </param>
		/// <returns>
		/// A <see cref="ICollection`1"/> of BookmarkItems.
		/// </returns>
		protected ICollection<BookmarkItem> ReadBookmarksFromFile (string file)
		{
			ICollection<BookmarkItem> list;
			StreamReader reader;
			int urlIndex, nameIndex, shortcutIndex;
			string url, name, shortcut;
			
			list = new List<BookmarkItem> ();

			try {
				reader = System.IO.File.OpenText (file);
				for (string line = reader.ReadLine (); line != null; line = reader.ReadLine ()) {
					try {
						urlIndex = line.IndexOf (BeginURL);
						if (urlIndex < 0) continue;
						line = line.Substring (urlIndex + BeginURL.Length);
						url = line.Substring (0, line.IndexOf (EndURL));

						// See if this bookmark has a shortcut (SHORTCUTURL="blog")
						shortcut = null;
						shortcutIndex = line.IndexOf (BeginShortcut);
						if (shortcutIndex > 0) {
							line = line.Substring (shortcutIndex + BeginShortcut.Length);
							shortcut = line.Substring (0, line.IndexOf (EndShortcut));
						}
						
						nameIndex = line.IndexOf (BeginName);
						if (nameIndex < 0) continue;
						line = line.Substring (nameIndex + BeginName.Length);
						name = line.Substring (0, line.IndexOf (EndName));
					} catch {
						continue;
					}

					list.Add (new BookmarkItem (name, url));
					if (shortcut != null)
						list.Add (new BookmarkItem (shortcut, url));
				}	
			} catch {
				list.Clear ();
			}
			return list;
		}
	}
}
