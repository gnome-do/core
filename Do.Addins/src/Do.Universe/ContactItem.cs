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
using System.IO;
using System.Collections.Generic;

namespace Do.Universe
{
	public interface IContactDetailItem : IItem
	{
		string Key { get; }
		string Value { get; }
	}
	
	public class ContactItem : IItem
	{
		static Dictionary<string, ContactItem> contactsByName, contactsByEmail;
		
		static ContactItem ()
		{
			contactsByName = new Dictionary<string, ContactItem> ();
			contactsByEmail = new Dictionary<string, ContactItem> ();
		}
		
		public static ContactItem Create (string name)
		{
			return CreateWithName (name);
		}
		
		public static ContactItem CreateWithName (string name)
		{
			ContactItem contact;
			
			contactsByName.TryGetValue (name.ToLower (), out contact);
			if (null == contact) {
				contactsByName[name.ToLower ()] = contact = new ContactItem ();
				contact["name"] = name;
			}
			return contact;
		}
		
		public static ContactItem CreateWithEmail (string email)
		{
			ContactItem contact;
			
			contactsByEmail.TryGetValue (email.ToLower (), out contact);
			if (null == contact) {
				contactsByEmail[email.ToLower ()] = contact = new ContactItem ();
				contact["email"] = email;
			}
			return contact;
		}
		
		protected Dictionary<string,string> details;
		
		private ContactItem ()
		{
			details = new Dictionary<string,string> ();
		}

		public IEnumerable<string> Details
		{
			get { return details.Keys; }
		}
		
		public string this [string key]
		{
			get {
				string detail;
				details.TryGetValue (key, out detail);
				return detail;
			}
			set {
				if (string.IsNullOrEmpty (key) || string.IsNullOrEmpty (value)) return;
				
				if (key.StartsWith ("email"))
					contactsByEmail[value.ToLower ()] = this;
				else if (key == "name")
					contactsByName[value.ToLower ()] = this;
				
				switch (key) {
				case "photo":
					UpdatePhotoDetail (value);
					break;
				default:
					details[key] = value;
					break;
				}
			}
		}
		
		public string Name
		{
			get { return this["name"] ?? this["email"]; }
		}
		
		public string Photo
		{
			get { return this["photo"]; }
		}
		
		public string Description
		{
			get {
				return AnEmailAddress ?? "No description.";
			}
		}

		public string AnEmailAddress
		{
			get {
				string email;
				
				email = this ["email"] ?? this ["email.work"] ?? this ["email.home"];
				if (email == null) {
					foreach (string detail in Details) {
						if (detail.StartsWith ("email"))
							return this [detail];
					}
				}
				return null;
			}
		}
		
		public string Icon
		{
			get {
				if (null != Photo && File.Exists (Photo))
					return Photo;
				return "stock_person";
			}
		}
		
		protected void UpdatePhotoDetail (string photo)
		{                   
			if (null == Photo) {
				details["photo"] = photo;
			} else if (!File.Exists (Photo)) {
				details["photo"] = photo;
			} else if (File.Exists (photo)) {
				// If there's already a photo file in place for this contact, replace it
				// if the new photo is larger (heuristic for highest quality).
				if (new FileInfo (Photo).Length < new FileInfo (photo).Length) {
					details["photo"] = photo;
				}
			}
		}
	}
}
