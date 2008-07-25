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
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Mono.Unix;

using Do.Addins;

namespace Do.Universe {

	public class GNOMESpecialLocationsItemSource : IItemSource {
		List<IItem> items;
		
		public GNOMESpecialLocationsItemSource()
		{
			items = new List<IItem> ();
		}
		
		class GNOMEURIItem : IURIItem {
			protected string uri, name, icon;
			
			public GNOMEURIItem (string uri, string name, string icon)
			{
				this.uri = uri;
				this.name = name;
				this.icon = icon;
			}
			
			virtual public string Name { get { return name; } }
			virtual public string Description { get { return URI; } }
			virtual public string Icon { get { return icon; } }
			virtual public string URI { get { return uri; } }
		}
			
		public string Name { 
			get { return Catalog.GetString ("GNOME Special Locations"); } 
		}
		
		public string Description {
			get { return Catalog.GetString ("Special locations in GNOME, "
				+ "such as Computer and Network.");
			} 
		}
		
		public string Icon { get { return "user-home"; } }

		public Type[] SupportedItemTypes
		{
			get {
				return new Type[] {
					typeof (IURIItem),
				};
			}
		}
		
		public ICollection<IItem> Items
		{
			get { return items; }
		}
		
		public ICollection<IItem> ChildrenOfItem (IItem item)
		{
			return null;
		}
		
		public void UpdateItems ()
		{
			items.Clear ();			
			items.Add (new GNOMETrashFileItem ());
			items.Add (new GNOMEURIItem ("computer:///", "Computer", "computer"));
			items.Add (new GNOMEURIItem ("network://", "Network", "network"));
			FillGNOMEBookmarkItems ();
		}
			
		private void FillGNOMEBookmarkItems ()
		{
			// Assemble the path to the bookmarks file.
			string home = Environment.GetFolderPath (Environment.SpecialFolder.Personal);
			string bookmarks_file = "~/.gtk-bookmarks".Replace ("~", home);

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
	
	class GNOMETrashFileItem : IFileItem, IOpenableItem {
		
		public string Path {
			get { 
				return Paths.Combine (
					Paths.ReadXdgUserDir ("XDG_DATA_HOME", ".local/share"),
					"Trash/files");
			}
		}

		public string Name {
			get { return "Trash"; }
		}

		public string Description {
			get { return "Trash"; }
		}

		public string URI {
			get { return "trash://"; }
		}

		public string Icon
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
			// Override Open to open trash:// instead of ~/.Trash.
			Util.Environment.Open ("trash://");
		}
	}

	class GNOMEBookmarkItem : IFileItem, IOpenableItem 
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
		
		public string Name {
			get { return name; }
		}		
		
		public string Description { 
			get { return uri + path; } 
		}

		public string URI {
			get { return uri; }
		}		
		
		public string Icon {
			get { return icon; }
		}
		
		public void Open ()
		{
			Util.Environment.Open(URI + Path);
		}	
	}
}
