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

namespace Do.Universe {

	public class FirefoxBookmarkItemSource : IItemSource {

		const string BeginProfileName = "Path=";
		const string BeginDefaultProfile = "Default=1";
		const string BeginURL = "<DT><A HREF=\"";
		const string EndURL = "\"";
		const string BeginShortcut = "SHORTCUTURL=\"";
		const string EndShortcut = "\"";
		const string BeginName = "\">";
		const string EndName = "</A>";
		
		ICollection<IItem> items;
		
		/// <summary>
		/// Initialize the item source.
		/// </summary>
		public FirefoxBookmarkItemSource ()
		{
			items = new IItem [0];
			UpdateItems ();
		}
		
		public Type[] SupportedItemTypes {
			get {
				return new Type[] {
					typeof (BookmarkItem),
				};
			}
		}
		
		public string Name {
			get { return "Firefox Bookmarks"; }
		}
		
		public string Description {
			get { return "Finds Firefox bookmarks in your default profile."; }
		}
		
		public string Icon {
			get { return "firefox"; }
		}
		
		public ICollection<IItem> Items {
			get { return items; }
		}
		
		public ICollection<IItem> ChildrenOfItem (IItem item)
		{
			return null;
		}
		
		public void UpdateItems ()
		{
			items = BookmarkItems;
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
		static string BookmarkFilePath { get {
			StreamReader reader;
			string line, path, profile;

			profile = null;
			path = Path.Combine (Paths.UserHome,
				".mozilla/firefox/profiles.ini");
			try {
				reader = File.OpenText (path);
			} catch {
				return null;
			}
			
			while (null != (line = reader.ReadLine ())) {
				if (line.StartsWith (BeginDefaultProfile)) break;
				if (line.StartsWith (BeginProfileName)) {
					line = line.Trim ();
					line = line.Substring (BeginProfileName.Length);
					profile = line;
				}
			}
			reader.Close ();
			
			if (profile == null) return null;
			path = Paths.Combine (Paths.UserHome,
				".mozilla/firefox", profile, "bookmarks.html");
			return path;
		} }
		
		protected ICollection<IItem> BookmarkItems { get {
			StreamReader reader;
			List<IItem> bookmarks;
			string line, url, name, shortcut;
			int urlIndex, nameIndex, shortcutIndex;
			
			bookmarks = new List<IItem> ();
			try {
				reader = File.OpenText (BookmarkFilePath);
				while (null != (line = reader.ReadLine ())) {
					try {
						urlIndex = line.IndexOf (BeginURL);
						if (urlIndex < 0) continue;
						line = line.Substring (urlIndex + BeginURL.Length);
						url = line.Substring (0, line.IndexOf (EndURL));

						// See if this bookmark has a shortcut
						// (SHORTCUTURL="blog")
						shortcut = null;
						shortcutIndex = line.IndexOf (BeginShortcut);
						if (shortcutIndex > 0) {
							line = line.Substring (shortcutIndex +
								BeginShortcut.Length);
							shortcut = line.Substring (0,
								line.IndexOf (EndShortcut));
						}
						
						nameIndex = line.IndexOf (BeginName);
						if (nameIndex < 0) continue;
						line = line.Substring (nameIndex + BeginName.Length);
						name = line.Substring (0, line.IndexOf (EndName));
					} catch {
						continue;
					}
					bookmarks.Add (new BookmarkItem (name, url));
					if (shortcut != null)
						bookmarks.Add (new BookmarkItem (shortcut, url));
				}	
			} catch {
				bookmarks.Clear ();
			}
			return bookmarks;
		} }
	}
}
