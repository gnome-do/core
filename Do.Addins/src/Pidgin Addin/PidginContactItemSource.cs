//  PidginContactItemSource.cs
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
using System.Xml;
using System.Collections.Generic;

using Do.Addins;

namespace Do.Universe
{
	public class PidginContactItemSource : IItemSource
	{

		static readonly string kBuddyListFile;
		static readonly string kBuddyIconDirectory;
		
		static PidginContactItemSource () {
			string home;
			
			home =  Environment.GetFolderPath (Environment.SpecialFolder.Personal);
			kBuddyListFile = "~/.purple/blist.xml".Replace("~", home);
			kBuddyIconDirectory = "~/.purple/icons".Replace("~", home);
		}
		
		List<IItem> buddies;
		
		public PidginContactItemSource ()
		{
			buddies = new List<IItem> ();
			UpdateItems ();
		}
		
		public Type[] SupportedItemTypes {
			get {
				return new Type[] {
					typeof (ContactItem),
				};
			}
		}
		
		public string Name { get { return "Pidgin Buddies"; } }
		public string Description { get { return "Buddies on your Pidgin buddy list."; } }
		public string Icon {get { return "pidgin"; } }
		
		public ICollection<IItem> Items {
			get { return buddies; }
		}
		
		public ICollection<IItem> ChildrenOfItem (IItem item)
		{
			return null;
		}
		
		public void UpdateItems ()
		{
			XmlDocument blist;
			// Add buddies as they are encountered to this hash so we don't create duplicates.
			Dictionary<ContactItem, bool> buddies_seen;
			
			buddies.Clear ();
			buddies_seen = new Dictionary<ContactItem, bool> ();
			blist = new XmlDocument ();
			try {
				blist.Load (kBuddyListFile);

				foreach (XmlNode contact_node in blist.GetElementsByTagName ("contact"))
				foreach (XmlNode buddy_node in contact_node.ChildNodes) {
					ContactItem buddy;		
					
					buddy = ContactItemFromBuddyXmlNode (buddy_node);
					if (buddy == null) continue;
					ContactItemStore.SynchronizeContactWithStore (ref buddy);
					buddies_seen[buddy] = true;
				}
				
			} catch (Exception e) {
				Console.Error.WriteLine ("Could not read Pidgin buddy list file: " + e.Message);
			}
			foreach (ContactItem buddy in buddies_seen.Keys) {
				buddies.Add (buddy);
			}
		}
		
		ContactItem ContactItemFromBuddyXmlNode (XmlNode buddy_node)
		{
			ContactItem buddy;
			string proto, name, alias, icon;
			
			try {
				name = alias = icon = null;
				// The messaging protocol (e.g. "prpl-jabber" for Jabber).
				proto = buddy_node.Attributes.GetNamedItem ("proto").Value;
				foreach (XmlNode attr in buddy_node.ChildNodes) {
					switch (attr.Name) {
					// The screen name.
					case "name":
						name = attr.InnerText;
						break;
					// The alias, or real name.
					case "alias":
						alias = attr.InnerText;
						break;
					// Buddy icon image file.
					case "setting":
						if (attr.Attributes.GetNamedItem ("name").Value == "buddy_icon") {
							icon = Path.Combine (kBuddyIconDirectory, attr.InnerText);
						}
						break;
					}
				}
			} catch {
				// Bad buddy.
				return null;
			}
			// If crucial details are missing, we can't make a buddy.
			if (name == null || proto == null) return null;
			
			// Create a new buddy, add the details we have.
			buddy = new ContactItem ();
			if (alias != null)
					buddy.Name = alias;
			if (icon != null)
					buddy.Photo = icon;
			AddScreenNameToContact (buddy, proto, name);
			return buddy;
		}
		
		void AddScreenNameToContact (ContactItem buddy, string proto, string name)
		{
			if (buddy == null || proto == null || name == null)
				return;
			
			switch (proto) {
			case "prpl-aim":
				if (!buddy.AIMs.Contains (name))
					buddy.AIMs.Add (name);
				break;
			case "prpl-jabber":
				if (!buddy.Jabbers.Contains (name))
					buddy.Jabbers.Add (name);
				break;
			}
		}
	}
}
