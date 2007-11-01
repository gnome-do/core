//  Evolution.cs (requires libevolution-cil)
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
using System.IO;
using System.Collections.Generic;

using Evolution;

using Do.Addins;

namespace Do.Universe
{

	public class EvolutionContactItemSource : IItemSource
	{
		
		List<IItem> contacts;
		
		public EvolutionContactItemSource ()
		{
			contacts = new List<IItem> ();
			UpdateItems ();
		}
		
		public Type[] SupportedItemTypes {
			get {
				return new Type[] {
					typeof (ContactItem),
				};
			}
		}
		
		public string Name { get { return "Evolution Contacts"; } }
		public string Description { get { return "Evolution Contacts"; } }
		public string Icon { get { return "evolution"; } }
		
		public void UpdateItems ()
		{
			try {
				_UpdateItems ();
			} catch (Exception e) {
				Console.WriteLine ("Cannot index evolution contacts because a {0} was thrown: {1}", e.GetType (), e.Message);
				return;
			}
		}
		
		public ICollection<IItem> Items {
			get { return contacts; }
		}
		
		public ICollection<IItem> ChildrenOfItem (IItem item)
		{
			return null;
		}
		
		void _UpdateItems ()
		{
			SourceList sources;
		
			contacts.Clear ();
			sources = new SourceList ("/apps/evolution/addressbook/sources");
			foreach (SourceGroup group in sources.Groups)
			foreach (Source source in group.Sources) {
				Book address_book;
				Contact[] e_contacts;
				ContactItem contact;
					
				// Only index local address books
				if (!source.IsLocal ()) continue;
				address_book = new Book (source);
				address_book.Open (true);
				e_contacts = address_book.GetContacts (BookQuery.AnyFieldContains (""));
				foreach (Contact e_contact in e_contacts) {
					try {
						contact = CreateEvolutionContactItem (e_contact);
					} catch {
						// bad contact
						continue;
					}
					contacts.Add (contact);
				}
			}
		}
	
		ContactItem CreateEvolutionContactItem (Contact e_contact) {
			ContactItem contact;
						
			contact = new ContactItem (e_contact.FullName);
			
			if (e_contact.Email1 != null && e_contact.Email1 != "")
				contact.Emails.Add (e_contact.Email1);
			if (e_contact.Email2 != null && e_contact.Email2 != "")
				contact.Emails.Add (e_contact.Email2);
			if (e_contact.Email3 != null && e_contact.Email3 != "")
				contact.Emails.Add (e_contact.Email3);
			
			contact.AIMs.AddRange (e_contact.ImAim);
			contact.Jabbers.AddRange (e_contact.ImJabber);
			
			switch (e_contact.Photo.PhotoType) {
			case ContactPhotoType.Inlined:
				contact.Photo = Path.GetTempFileName () + ".jpg";
				try {
					File.WriteAllBytes (contact.Photo, e_contact.Photo.Data);
				} catch {
					contact.Photo = null;
				}
				break;
			case ContactPhotoType.Uri:
				if (File.Exists (e_contact.Photo.Uri)) {
					contact.Photo = e_contact.Photo.Uri;
				}
				break;
			}
			ContactItemStore.SynchronizeContactWithStore (ref contact);
			return contact;
		}
		
	}
}
