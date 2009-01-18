//  ProxyItem.cs
//
//  GNOME Do is the legal property of its developers, whose names are too
//  numerous to list here.  Please refer to the COPYRIGHT file distributed with
//  this source distribution.
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


namespace Do.Universe
{
	
	public class ProxyItem : Item
	{
		
		protected string name, description, icon;

		protected ProxyItem ():
			this (null, null, null, null)
		{
		}
		
		public ProxyItem (Item item):
			this (item, null, null, null)
		{
		}
		
		public ProxyItem (Item item, string name):
			this (item, name, null, null)
		{
		}
		
		public ProxyItem (Item item, string name, string description):
			this (item, name, description, null)
		{
		}
		
		public ProxyItem (Item item, string name, string description, string icon)
		{
			Item = item;
			this.name = name;
			this.description = description;
			this.icon = icon;
		}

		public virtual Item Item {
			get; protected set;
		}

		public override string Name {
			get { return name ?? Item.Name; }
		}
		
		public override string Description {
			get { return description ?? Item.Description; }
		}
		
		public override string Icon {
			get { return icon ?? Item.Icon; }
		}


		public override bool PassesTypeFilter (IEnumerable<Type> types)
		{
			return Unwrap (Item).PassesTypeFilter (types);
		}

		public static Item Unwrap (Item item)
		{
			// WARNING: Infinite loop could occur here (stack will overflow)
			return item is ProxyItem ? Unwrap ((item as ProxyItem).Item) : item;
		}

		public static IEnumerable<Item> Unwrap (IEnumerable<Item> items)
		{
			return items.Select (item => Unwrap (item));
		}

	}
}
