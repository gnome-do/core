//  PidginChatCommand.cs (requires package libpurple-bin to use purple-remote)
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
using System.Threading;
using System.Diagnostics;

using Do.Addins;

namespace Do.Universe
{
	
	public class PidginChatCommand : ICommand
	{
		
		public PidginChatCommand ()
		{
		}
		
		public string Name {
			get { return "Chat"; }
		}
		
		public string Description {
			get { return "Send an instant message to a friend."; }
		}
		
		public string Icon {
			get { return "internet-group-chat"; }
		}
		
		public Type[] SupportedItemTypes {
			get {
				return new Type[] {
					typeof (ContactItem),
				};
			}
		}
		
		public Type[] SupportedModifierItemTypes {
			get { return null; }
		}

		public bool SupportsItem (IItem item)
		{
			bool has_im;
			ContactItem c;
			
			c = item as ContactItem;
			has_im = c.AIMs.Count > 0 ||
				     c.Jabbers.Count > 0;
			return has_im;
		}
		
		public bool SupportsModifierItemForItems (IItem[] items, IItem modItem)
		{
			return false;
		}
		
		public void Perform (IItem[] items, IItem[] modifierItems)
		{
			string protocol, screenname, dbus_instruction;
			ContactItem c;

			protocol = screenname = "";
			foreach (IItem item in items) {
				if (item is ContactItem) {
					c = item as ContactItem;
					if (c.Jabbers.Count > 0) {
						protocol = "jabber";
						screenname = c.Jabbers[0];
					} else if (c.AIMs.Count > 0) {
						protocol = "aim";
						screenname = c.AIMs[0];
					}

					new Thread ((ThreadStart) delegate {
						if (!Pidgin.InstanceIsRunning ()) {
							Process.Start ("pidgin");
							Thread.Sleep (4 * 1000);
						}
						dbus_instruction = string.Format ("\"{0}:goim?screenname={1}\"", protocol, screenname);
						Process.Start ("purple-remote", dbus_instruction);
					}).Start ();
				}
			}
		}
		
	}
}
