using System;
using System.Collections.Generic;

using Gnome;
using Gnome.Vfs;
using System.IO;

using Do.PluginLib;

namespace Do.PluginLib.Builtin
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
