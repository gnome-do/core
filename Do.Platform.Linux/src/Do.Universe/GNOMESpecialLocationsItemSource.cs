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
		List<Item> items;
		
		public GNOMESpecialLocationsItemSource()
		{
			items = new List<Item> ();
		}
		
		class GNOMEURItem : Item, IUriItem {
			protected string uri, name, icon;
			
			public GNOMEURItem (string uri, string name, string icon)
			{
				this.uri = uri;
				this.name = name;
				this.icon = icon;
			}
			
			protected override string Name { get { return name; } }
			protected override string Description { get { return Uri; } }
			protected override string Icon { get { return icon; } }
			public string Uri { get { return uri; } }
		}
			
		protected override string Name  { 
			get { return Catalog.GetString ("GNOME Special Locations"); } 
		}
		
		protected override string Description  {
			get { return Catalog.GetString ("Special locations in GNOME, "
				+ "such as Computer and Network.");
			} 
		}
		
		protected override string Icon  { get { return "user-home"; } }

		public override IEnumerable<Type> SupportedItemTypes {
			get {
				yield return typeof (IUriItem);
			}
		}
		
		protected override IEnumerable<Item> Items {
			get { return items; }
		}
		
		protected override void UpdateItems ()
		{
			items.Clear ();			
			items.Add (new GNOMETrashFileItem ());
			items.Add (new GNOMEURItem ("computer:///", "Computer", "computer"));
			items.Add (new GNOMEURItem ("network://", "Network", "network"));
			FillGNOMEBookmarkItems ();
		}
			
		private void FillGNOMEBookmarkItems ()
		{
			// Assemble the path to the bookmarks file.
			string bookmarks_file = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal), ".gtk-bookmarks");

			try {
				string line;
				Regex regex = new Regex ("([^ ]*) (.*)");
				
				string URI;
				string name;
				string path;
				bool network;
				
				using (StreamReader reader = new StreamReader (bookmarks_file)) {
					while ((line = reader.ReadLine ()) != null) {
						Match match = regex.Match (line);
						network = (match.Groups.Count == 3); 
						if (network) {
							name = match.Groups [2].ToString ();
							URI = GetURI(match.Groups [1].ToString ());
							path = GetPath(match.Groups [1].ToString ());
						}						
						else {
							URI = GetURI  (line);
							path = GetPath (line);
							name = GetDirectory (line);
						} 
																	
						items.Add (new GNOMEBookmarkItem (name, URI, path, network));
					}
				}
			} catch (Exception e) {
				// Something went horribly wrong, so we print the error message.
				Console.Error.WriteLine ("Could not read Gnome Bookmarks file {0}: {1}", bookmarks_file, e.Message);
			}
		}
		
		private int GetURIDelimiter (string fullPath)
		{
			return fullPath.IndexOf ("//");
		}
		
		private string GetURI (string fullpath)
		{
			int delimindex = GetURIDelimiter (fullpath);
			
			if (delimindex > -1) 
				return fullpath.Substring (0, delimindex+2 );
			else
				return "file://";
		}

		private string GetPath (string fullpath)
		{
			int delimindex = GetURIDelimiter (fullpath);
			
			if (delimindex > -1) 
				return fullpath.Substring (delimindex+2, fullpath.Length - delimindex-2);
			else
				return Environment.GetFolderPath (Environment.SpecialFolder.Personal);
		}		

		private string GetDirectory (string fullpath)
		{
			int lastSlashPosition = fullpath.LastIndexOf ("/");
			return fullpath.Substring(lastSlashPosition+1, fullpath.Length-lastSlashPosition-1);
		}		
		
	}
	
	class GNOMETrashFileItem : Item, IFileItem, IOpenableItem
	{

		string path;
		public string Path {
			get {
				if (path != null) return path;
				
				return path = new [] {
					Environment.GetFolderPath (Environment.SpecialFolder.LocalApplicationData),
					"Trash",
					"files",
				}.Aggregate (System.IO.Path.Combine);
			}
		}

		protected override string Name  {
			get { return "Trash"; }
		}

		protected override string Description  {
			get { return "Trash"; }
		}

		public string Uri {
			get { return "trash://"; }
		}

		protected override string Icon 
		{
			get {
				if (Directory.Exists (Path) &&
					Directory.GetFileSystemEntries (Path).Length > 0) {
					return "user-trash-full";
				} else {
					return "user-trash";
				}
			}
		}

		public void Open ()
		{
			Services.Environment.OpenUrl ("trash://");
		}
	}

	class GNOMEBookmarkItem : Item, IFileItem, IOpenableItem 
	{  
		private string uri;		
		private string icon;
		private string name;
		private string path;
		
		public GNOMEBookmarkItem (string fullname, string fullURI, string fullpath, bool networkType)
		{
			uri = fullURI;			
			name = fullname;
			path = fullpath;
			icon = networkType ? "network" : "folder";
		}

		public string Path {
			get { return path; }
		}		
		
		protected override string Name  {
			get { return name; }
		}		
		
		protected override string Description  { 
			get { return uri + path; } 
		}

		public string Uri {
			get { return uri; }
		}		
		
		protected override string Icon  {
			get { return icon; }
		}
		
		public void Open ()
		{
			Services.Environment.OpenUrl (Uri + Path);
		}	
	}
}
