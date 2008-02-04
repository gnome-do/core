/* OpenTerminalHereAction.cs
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

using Mono.Unix;

using Do.Addins;

namespace Do.Universe
{
	public class OpenTerminalHereAction : AbstractAction
	{
		public OpenTerminalHereAction ()
		{
		}
		
		public override string Name
		{
			get { return Catalog.GetString ("Open Terminal Here"); }
		}
		
		public override string Description
		{
			get { return Catalog.GetString ("Opens a Terminal in a given location."); }
		}
		
		public override string Icon
		{
			get { return "terminal"; }
		}
		
		public override Type[] SupportedItemTypes
		{
			get {
				return new Type[] {
					typeof (IFileItem),
				};
			}
		}
		
		public override IItem[] Perform (IItem[] items, IItem[] modifierItems)
		{
			GConf.Client client;
			Process term;
			IFileItem fi;
			string dir, exec;

			client = new GConf.Client();
			try {
				exec = client.Get ("/desktop/gnome/applications/terminal/exec") as string;
			} catch {
				exec = "gnome-terminal";
			}
			
			fi = items[0] as IFileItem;
			dir = fi.Path;
			if (!FileItem.IsDirectory (fi))
				dir = System.IO.Path.GetDirectoryName (dir);

			term = new Process ();
			term.StartInfo.WorkingDirectory = dir;
			term.StartInfo.FileName = exec;
			term.Start ();
			return null;
		}
	}
}
