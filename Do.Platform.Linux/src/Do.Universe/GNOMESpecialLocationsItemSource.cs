//  GNOMESpecialLocationsItemSource.cs
//
//  GNOME Do is the legal property of its developers, whose names are too numerous
//  to list here.  Please refer to the COPYRIGHT file distributed with this
//  source distribution.
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Mono.Unix;

using Do.Platform;
using Do.Universe;

namespace Do.Universe.Linux {

	public class GNOMESpecialLocationsItemSource : ItemSource {

		static readonly string BookmarksFile = FindBookmarksFile();

		static string FindBookmarksFile()
		{
			// Try GTK 3.0 path first...
			string candidate = Path.Combine (Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
			                                 "gtk-3.0",
			                                 "bookmarks");
			if (File.Exists (candidate))
				return candidate;

			// Then try GTK 2.0 path...
			candidate = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal),
			                          ".gtk-bookmarks");
			if  (File.Exists (candidate))
				return candidate;

			// Whoops! No bookmarks for us!
			return null;
		}

		List<Item> items;
		
		public GNOMESpecialLocationsItemSource()
		{
			items = new List<Item> ();
		}
		
		public override string Name { 
			get { return Catalog.GetString ("GNOME Special Locations"); } 
		}
		
		public override string Description {
			get {
				return Catalog.GetString ("Special locations in GNOME, such as Computer and Network.");
			} 
		}
		
		public override string Icon { get { return "user-bookmarks"; } }
			
		public override IEnumerable<Type> SupportedItemTypes {
			get { yield return typeof (IUriItem); }
		}
		
		public override IEnumerable<Item> Items {
			get { return items; }
		}
		
		public override void UpdateItems ()
		{
			items.Clear ();			
			items.Add (new GNOMETrashItem ());
			items.Add (new GNOMEBookmarkItem ("Computer", "computer://", "computer"));
			items.Add (new GNOMEBookmarkItem ("Network", "network://", "network"));
			foreach (Item item in ReadBookmarkItems ()) items.Add (item);
		}
			
		IEnumerable<IUriItem> ReadBookmarkItems ()
		{
			string line, uri, name;
			Regex regex = new Regex ("([^ ]*) (.*)");

			if (BookmarksFile == null)
				yield break;
			
			using (StreamReader reader = new StreamReader (BookmarksFile)) {
				while ((line = reader.ReadLine ()) != null) {

					Match match = regex.Match (line);
					bool isNetworkBookmark = match.Groups.Count == 3; 

					if (line.StartsWith ("file://")) {
						string path = line;
						
						path = path.Substring ("file://".Length);
						// Some entries contain more information after the URI. We
						// discard it.
						if (line.Contains (" "))
							path = path.Substring (0, path.IndexOf (" "));
						path = Uri.UnescapeDataString (path);

						yield return Services.UniverseFactory.NewFileItem (path);	
					} else if (isNetworkBookmark) {
						name = match.Groups [2].ToString ();
						uri = match.Groups [1].ToString ();
						yield return new GNOMEBookmarkItem (name, uri, "network");
					}
				}
			}
		}

	}
}
