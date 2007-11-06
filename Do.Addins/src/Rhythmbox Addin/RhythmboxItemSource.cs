//  RhythmboxItemSource.cs
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
	
	public class RhythmboxItemSource : IItemSource
	{
		
		List<IItem> items;
		
		public RhythmboxItemSource()
		{
			items = new List<IItem> ();
			items.AddRange (RhythmboxRunnableItem.DefaultItems);
		}
		
		public string Name { get { return "Rhythmbox Music Jukebox Actions"; } }
		public string Description { get { return "Control Rhythmbox using commands like Play, Pause, etc."; } }
		public string Icon { get { return "rhythmbox"; } }

		public Type[] SupportedItemTypes {
			get {
				return new Type[] {
					typeof (RhythmboxRunnableItem),
				};
			}
		}
		
		public ICollection<IItem> Items { get { return items; } }
		public ICollection<IItem> ChildrenOfItem (IItem item) { return null; }
		public void UpdateItems () { }
	}
}
