//  ContactItem.cs
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

namespace Do.Universe
{
	
	public class ContactItem : IItem
	{
		// These fields are internal so they can be accessed by ContactItemStore
		internal string name, photo;
		internal List<string> emails, aims, jabbers;
		
		public ContactItem () :
			this (null)
		{
		}
		
		public ContactItem (string name)
		{
			this.name = name;
			emails = new List<string> ();
			aims = new List<string> ();
			jabbers = new List<string> ();
		}
		
		public string Name
		{
			get {
				if (name != null) return name;
				if (emails.Count > 0) return emails[0];
				if (aims.Count > 0) return aims[0];
				if (jabbers.Count > 0) return jabbers[0];
				return "Unnamed Contact";
			}
			set { name = value; }
		}
		
		public string Photo
		{
			get { return photo; }
			set { photo = value; }
		}
		
		public string Description
		{
			get {
				if (emails.Count > 0) return emails[0];
				if (aims.Count > 0) return "AIM: " + aims[0];
				if (jabbers.Count > 0) return "Jabber: " + jabbers[0];
				return "No description.";
			}
		}
		
		public string Icon
		{
			get {
				return Photo ?? "stock_person";
			}
		}
		
		public List<string> Emails { get { return emails; } }
		public List<string> AIMs { get { return aims; } }
		public List<string> Jabbers { get { return jabbers; } }
	}
}
