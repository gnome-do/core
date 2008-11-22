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
using System.Linq;
using System.Collections.Generic;

using Do.Core;
using Mono.Unix;

namespace Do.Universe {
	
	public class ItemSourceItemSource : IItemSource {

		class ItemSourceItem : IItem {

			public IItemSource Source { get; private set; }

			public string Name { get { return Source.Name; } }
			public string Description { get { return Source.Description; } }
			public string Icon { get { return Source.Icon; } }

			public ItemSourceItem (DoItemSource source)
			{
				Source = source;
			}
		}

		IEnumerable<IItem> items;

		public ItemSourceItemSource ()
		{
			items = Enumerable.Empty<IItem> ();
		}
		
		public IEnumerable<Type> SupportedItemTypes
		{
			get {
				yield return typeof (ItemSourceItem);
			}
		}
		
		public string Name {
			get {
				return Catalog.GetString ("GNOME Do Item Sources");
			}
		}
		
		public string Description {
			get {
				return Catalog.GetString ("Item Sources providing all items GNOME Do knows about.");
			}
		}
		
		public string Icon {
			get {
				return "gnome-do";
			}
		}
		
		public IEnumerable<IItem> Items {
			get { return items; }
		}

		public void UpdateItems ()
		{
			items = PluginManager.GetItemSources ()
				.Select (source => new ItemSourceItem (source))
				.Cast<IItem> ();
		}
		
		public IEnumerable<IItem> ChildrenOfItem (IItem item)
		{
			return (item as ItemSourceItem).Source.Items;
		}
	}
}
