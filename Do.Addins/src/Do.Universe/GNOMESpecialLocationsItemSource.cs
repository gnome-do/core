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
using System.Collections.Generic;

using Do.Addins;

namespace Do.Universe
{
	
	
	public class GNOMESpecialLocationsItemSource : IItemSource
	{
		
		List<IItem> items;
		
		class GNOMEURIItem : IURIItem
		{
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
		
		class GNOMETrashURIItem : GNOMEURIItem
		{
			public GNOMETrashURIItem () : base ("trash://", "Trash", null)
			{
			}
			
			override public string Icon {
				get {
					string trash_dir;
					
					trash_dir = "~/.Trash".Replace ("~",
					     System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal));
					if (System.IO.Directory.Exists (trash_dir)
						&& System.IO.Directory.GetFileSystemEntries (trash_dir).Length > 0) {
						icon = "user-trash-full";
					} else {
						icon = "user-trash";
					}
					return icon;
				}
			}
		}
		
		public GNOMESpecialLocationsItemSource()
		{
			items = new List<IItem> ();
			items.Add (new GNOMETrashURIItem ());
			items.Add (new GNOMEURIItem ("computer:///", "Computer", "computer"));
			items.Add (new GNOMEURIItem ("network://", "Network", "network"));
			items.Add (new GNOMEURIItem ("~", "Home", "user-home"));
		}
		
		public string Name { get { return "GNOME Special Locations"; } }
		public string Description { get { return "Special locations in GNOME, such as Computer and Network."; } }
		public string Icon { get { return "gnome"; } }

		public Type[] SupportedItemTypes {
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
		}
	}
}
