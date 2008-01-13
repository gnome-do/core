//  ContactItemStore.cs
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
using System.IO;
using System.Collections.Generic;

using Do.Universe;

namespace Do.Addins
{
	/// <summary>
	/// This class helps de-duplicate ContactItems created from different
	/// item sources; for example, combining partial information about a single
	/// contact from Pidgin and Evolution.
	/// 
	/// TODO: Rewrite this class so that ContactItems can contain arbitrary
	/// attributes. Perhaps use VCard standard as a starting point.
	/// </summary>
	public class ContactItemStore
	{
		static Dictionary<ContactItem, bool> contacts;
		static Dictionary<string, ContactItem> contacts_by_name;
		static Dictionary<string, ContactItem> contacts_by_email;
		static Dictionary<string, ContactItem> contacts_by_aim;
		static Dictionary<string, ContactItem> contacts_by_jabber;

		static ContactItemStore ()
		{
			contacts = new Dictionary<ContactItem, bool> ();
			contacts_by_name = new Dictionary<string, ContactItem> ();
			contacts_by_email = new Dictionary<string, ContactItem> ();
			contacts_by_aim = new Dictionary<string, ContactItem> ();
			contacts_by_jabber = new Dictionary<string, ContactItem> ();
		}
		
		public static ICollection<ContactItem> Contacts
		{
			get { return contacts.Keys; }
		}

		/// <summary>
		/// Given a ContactItem with some information about a contact, this
		/// method will attempt to find information about this contact
		/// already contained in the contact store. Additional information from
		/// the contact passed by reference will be added to the canonical contact
		/// in the store, and the updated contact from the store will be returned
		/// in the reference.
		/// </summary>
		/// <param name="contact">
		/// A <see cref="ContactItem"/> whose information you wish to merge with
		/// the store.
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/> indicating whether the contact was merged
		/// with a pre-existing contact from the store.
		/// </returns>
		public static bool SynchronizeContactWithStore (ref ContactItem contact)
		{
			ContactItem match;

			match = null;
			do {
				if (contact.name != null) {
					contacts_by_name.TryGetValue (contact.Name.ToLower(), out match);
					if (match != null) break;
				}
				foreach (string email in contact.Emails) {
					contacts_by_email.TryGetValue (email.ToLower(), out match);
					if (match != null) break;
				}
				foreach (string aim in contact.AIMs) {
					contacts_by_aim.TryGetValue (aim.ToLower(), out match);
					if (match != null) break;
				}
				foreach (string jabber in contact.Jabbers) {
					contacts_by_jabber.TryGetValue (jabber.ToLower(), out match);
					if (match != null) break;
				}
				// New contact data.
				AddContactData (contact);
				return false;
			} while (false);	

			MergeContactIntoContact (contact, match);
			AddContactData (match);
			contact = match;
			return true;
		}

		static void MergeContactIntoContact (ContactItem source, ContactItem dest)
		{
			// This is a very delicate line; the distinction between fields
			// and properties is essential.
			if (dest.name == null && source.name != null)
				dest.Name = source.Name;

			if (dest.Photo == null || !File.Exists (dest.Photo)) {
				// If dest has no photo, give it source's photo.
				dest.Photo = source.Photo;
			} else if (source.Photo != null && File.Exists (dest.Photo)
			                                && File.Exists (source.Photo)) {
				// If there's already a photo file in place for this contact, replace it
				// if the source photo is larger (heuristic for highest quality).
				long source_photo_size, dest_photo_size;

				source_photo_size = new FileInfo (source.Photo).Length;
				dest_photo_size = new FileInfo (dest.Photo).Length;
				if (source_photo_size > dest_photo_size) {
					dest.Photo = source.Photo;
				}
			}

			AddMissingListElements<string> (source.Emails, dest.Emails);
			AddMissingListElements<string> (source.AIMs, dest.AIMs);
			AddMissingListElements<string> (source.Jabbers, dest.Jabbers);
		}

		static void AddMissingListElements<T> (List<T> source, List<T> dest)
		{
			foreach (T member in source) {
				if (!dest.Contains (member))
					dest.Add (member);
			}
		}

		public static void AddContactData (ContactItem c)
		{
			if (c == null) return;

			contacts[c] = true;

			if (c.name != null)
				contacts_by_name[c.name.ToLower()] = c;

			foreach (string email in c.Emails)
				contacts_by_email[email.ToLower()] = c;

			foreach (string aim in c.AIMs)
				contacts_by_aim[aim.ToLower()] = c;

			foreach (string jabber in c.Jabbers)
				contacts_by_jabber[jabber.ToLower()] = c;
		}
	}
}
