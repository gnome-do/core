//  ItemSourceItemSource.cs
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

using Do.Universe;

namespace Do.Core
{
	public class ItemSourceItemSource : IItemSource
	{
		public ItemSourceItemSource ()
		{
		}
		
		public Type[] SupportedItemTypes
		{
			get {
				return new Type[] {
					typeof (DoItemSource),
					typeof (ApplicationItem),
				};
			}
		}
		
		public string Name
		{
			get { return "GNOME Do Item Sources"; }
		}
		
		public string Description
		{
			get { return "Item Sources providing all items GNOME Do knows about."; }
		}
		
		public string Icon
		{
			get { return "gnome-run"; }
		}
		
		public void UpdateItems ()
		{
		}
		
		public ICollection<IItem> Items
		{
			get { return null; }
		}
		
		public ICollection<IItem> ChildrenOfItem (IItem item)
		{
			List<IItem> children;
			bool parent_is_this;
		
			children = new List<IItem> ();
			parent_is_this = (item is DoItemSource && (item as DoItemSource).Inner == this);
			if (item is DoItemSource && !parent_is_this) {
				foreach (DoItem child in (item as DoItemSource).Items) {
					children.Add (child);
				}
			}
			else if (parent_is_this ||
			         (item is ApplicationItem &&(item as ApplicationItem).Name == "GNOME Do")) {   // parent is GNOME Do
				foreach (DoItemSource source in Do.UniverseManager.ItemSources) {
					children.Add (source);
				}
			}
			return children;
		}
	}
}
