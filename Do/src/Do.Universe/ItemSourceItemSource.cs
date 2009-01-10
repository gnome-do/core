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

namespace Do.Universe
{
	
	internal class ItemSourceItemSource : ItemSource
	{

		public class ItemSourceItem : Item {

			public ItemSource Source { get; private set; }

			public override string Name { get { return Source.Name; } }
			public override string Description { get { return Source.Description; } }
			public override string Icon { get { return Source.Icon; } }

			public ItemSourceItem (ItemSource source)
			{
				Source = source;
			}
		}

		IEnumerable<Item> items;

		public ItemSourceItemSource ()
		{
			items = Enumerable.Empty<Item> ();
		}
		
		public override IEnumerable<Type> SupportedItemTypes
		{
			get { yield return typeof (ItemSourceItem); }
		}
		
		public override string Name {
			get { return Catalog.GetString ("GNOME Do Item Sources"); }
		}
		
		public override string Description {
			get { return Catalog.GetString ("Item Sources providing all items GNOME Do knows about."); }
		}
		
		public override string Icon {
			get { return "gnome-do"; }
		}
		
		public override IEnumerable<Item> Items {
			get { return items; }
		}

		public override void UpdateItems ()
		{
			items = PluginManager.ItemSources
				.Select (source => new ItemSourceItem (source))
				.Cast<Item> ();
		}
		
		public override IEnumerable<Item> ChildrenOfItem (Item item)
		{
			return (item as ItemSourceItem).Source.Safe.Items;
		}
	}
}
