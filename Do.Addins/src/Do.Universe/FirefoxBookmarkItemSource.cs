/* ${FileName}
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

using Gnome;
using Gnome.Vfs;
using System.IO;

namespace Do.Universe
{
	public class FirefoxBookmarkItemSource : IItemSource
	{
	
		const string BeginProfileName = "Path=";
		const string BeginDefaultProfile = "Name=default";
		const string BeginURL = "<DT><A HREF=\"";
		const string EndURL = "\"";
		const string BeginName = "\">";
		const string EndName = "</A>";
		
		List<IItem> bookmarks;
		
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
	
		public FirefoxBookmarkItemSource ()
		{
			bookmarks = new List<IItem> ();
			UpdateItems ();
		}
		
		public string Name {
			get { return "Firefox Bookmarks"; }
		}
		
		public string Description {
			get { return "Finds Firefox bookmarks in your default profile."; }
		}
		
		public string Icon {
			get { return "www"; }
		}
		
		public ICollection<IItem> Items {
			get { return bookmarks; }
		}
		
		public bool UpdateItems ()
		{
			bookmarks.AddRange (ReadBookmarksFromFile (GetFirefoxBookmarkFilePath ()));
			return true;
		}
		
		protected ICollection<IItem> ReadBookmarksFromFile (string file)
		{
			ICollection<IItem> list;
			StreamReader reader;
			int urlIndex, nameIndex;
			string url, name;
			
			list = new List<IItem> ();

			try {
				reader = System.IO.File.OpenText (file);
				for (string line = reader.ReadLine (); line != null; line = reader.ReadLine ()) {
					try {
						urlIndex = line.IndexOf (BeginURL);
						if (urlIndex < 0) continue;
						line = line.Substring (urlIndex + BeginURL.Length);
						url = line.Substring (0, line.IndexOf (EndURL));
						
						nameIndex = line.IndexOf (BeginName);
						if (nameIndex < 0) continue;
						line = line.Substring (nameIndex + BeginName.Length);
						name = line.Substring (0, line.IndexOf (EndName));
					} catch {
						continue;
					}
					list.Add (new BookmarkItem (name, url));
				}	
			} catch {
				list.Clear ();
			}
			return list;
		}
		
	}
}
