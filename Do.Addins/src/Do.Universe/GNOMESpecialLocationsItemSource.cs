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
			string uri, name, icon;
			
			public GNOMEURIItem (string uri, string name, string icon)
			{
				this.uri = uri;
				this.name = name;
				this.icon = icon;
			}
			
			public string Name { get { return name; } }
			public string Description { get { return URI; } }
			public string Icon { get { return icon; } }
			public string URI { get { return uri; } }
		}
		
		public GNOMESpecialLocationsItemSource()
		{
			items = new List<IItem> ();
			items.Add (new GNOMEURIItem ("trash://", "Trash", "user-trash"));
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
