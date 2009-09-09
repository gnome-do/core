//  ContactDetailItem.cs
//
//  GNOME Do is the legal property of its developers.
//  Please refer to the COPYRIGHT file distributed with this
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

namespace Do.Universe.Common
{
	public class ContactDetailItem : Item, IContactDetailItem 
	{
		public readonly ContactItem Owner;
		string detail;

		public ContactDetailItem (ContactItem owner, string detail)
		{
			Owner = owner;
			this.detail = detail;
		}

		public override string Name { get { return Key; } }
		public override string Description { get { return Value; } }
		public override string Icon { get { return "stock_person"; } }

		public virtual string Key { get { return detail; } }
		public virtual string Value { get { return Owner [detail]; } }
	}
}