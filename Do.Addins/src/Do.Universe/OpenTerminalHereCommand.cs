/* OpenTerminalHereCommand.cs
 *
 * GNOME Do is the legal property of its developers. Please refer to the
 * COPYRIGHT file distributed with this
 * source distribution.
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Diagnostics;

using Do.Addins;

namespace Do.Universe
{
	public class OpenTerminalHereCommand : ICommand
	{
		public OpenTerminalHereCommand ()
		{
		}
		
		public string Name
		{
			get { return "Open Terminal Here"; }
		}
		
		public string Description
		{
			get { return "Opens a Terminal in a given location."; }
		}
		
		public string Icon
		{
			get { return "gnome-terminal"; }
		}
		
		public Type[] SupportedItemTypes
		{
			get {
				return new Type[] {
					typeof (FileItem),
				};
			}
		}
		
		public Type[] SupportedModifierItemTypes
		{
			get {
				return null;
			}
		}

		public bool SupportsItem (IItem item)
		{
			return true;
		}
		
		public bool SupportsModifierItemForItems (IItem[] items, IItem modItem)
		{
			return false;
		}
		
		public void Perform (IItem[] items, IItem[] modifierItems)
		{
			GConf.Client client;
			Process term;
			FileItem fi;
			string dir, exec;

			client = new GConf.Client();
			try {
				exec = client.Get ("/desktop/gnome/applications/terminal/exec") as string;
			} catch {
				exec = "gnome-terminal";
			}
			
			fi = items[0] as FileItem;
			dir = fi.Path;
			if (!(fi is DirectoryFileItem))
				dir = System.IO.Path.GetDirectoryName (dir);

			term = new Process ();
			term.StartInfo.WorkingDirectory = dir;
			term.StartInfo.FileName = exec;
			term.Start ();
		}
	}
}
