//  ThunderbirdContactItemSource.cs
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

// 

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using Do.Addins;
using Beagle.Util;

namespace Do.Universe
{

	public class ThunderbirdContactItemSource : IItemSource
	{
		
		const string BeginProfileName = "Path=";
		const string BeginDefaultProfile = "Name=default";
		
		List<IItem> contacts;
		
		public ThunderbirdContactItemSource ()
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
		
		public string Name { get { return "Thunderbird Contacts"; } }
		public string Description { get { return "Thunderbird Contacts"; } }
		public string Icon { get { return "thunderbird"; } }
		
		public void UpdateItems ()
		{
			try {
				_UpdateItems ();
			} catch (Exception e) {
				Console.WriteLine ("Cannot index Thunderbird contacts because a {0} was thrown: {1}", e.GetType (), e.Message);
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
			MorkDatabase database;
		
			contacts.Clear ();
			database = new MorkDatabase (GetThunderbirdAddressBookFilePath ());
			database.Read ();
			database.EnumNamespace = "ns:addrbk:db:row:scope:card:all";

			foreach (string id in database) {
				Hashtable contact_row;
				ContactItem contact;
				
				contact_row = database.Compile (id, database.EnumNamespace);
				contact = CreateThunderbirdContactItem (contact_row);
				if (contact != null)
					contacts.Add (contact);
			}
		}
	
		ContactItem CreateThunderbirdContactItem (Hashtable row) {
			ContactItem contact;
			string name, email;
			
			contact = new ContactItem ();
			
//			foreach (object o in row.Keys)
//				Console.WriteLine ("\t{0} --> {1}", o, row[o]);
			
			// I think this will detect deleted contacts... Hmm...
			if (row["table"] == null || row["table"] as string == "C6")
				return null;
			
			// Name
			name = row["DisplayName"] as string;
			if (name == null || name == string.Empty)
				name = string.Format ("{0} {1}", row["FirstName"], row["LastName"]);
			contact.Name = name;
			
			// Email
			email = row["PrimaryEmail"] as string;
			if (email != null && email != string.Empty)
				contact.Emails.Add (email);
			
			ContactItemStore.SynchronizeContactWithStore (ref contact);
			return contact;
		}
		
		string GetThunderbirdAddressBookFilePath ()
		{
			string home, path, profile;
			StreamReader reader;

			profile = null;
			home = System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal);
			path = System.IO.Path.Combine (home, ".mozilla-thunderbird/profiles.ini");
			try {
				reader = System.IO.File.OpenText (path);
			} catch {
				return null;
			}
			
			bool got_default = false;
			for (string line = reader.ReadLine (); line != null; line = reader.ReadLine ()) {
				if (got_default && line.StartsWith (BeginProfileName)) {
					line = line.Trim ();
					line = line.Substring (BeginProfileName.Length);
					profile = line;
					break;
				}
				else if (line.StartsWith (BeginDefaultProfile)) {
					got_default = true;
				}
			}
			reader.Close ();
			
			if (profile == null) {
				return null;
			}
			path = System.IO.Path.Combine (home, ".mozilla-thunderbird");
			path = System.IO.Path.Combine (path, profile);
			path = System.IO.Path.Combine (path, "abook.mab");
			return path;
			
		}
		
	}
}
