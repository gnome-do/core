/* RunAction.cs
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
using System.IO;
using Mono.Unix;
using Do.Addins;

namespace Do.Universe
{
	public class RunAction : AbstractAction
	{
		public override string Name
		{
			get { return Catalog.GetString ("Run"); }
		}
		
		public override string Description
		{
			get { return Catalog.GetString ("Run an application, script, or other executable."); }
		}
		
		public override string Icon
		{
			get { return "gnome-run"; }
		}
		
		public override Type[] SupportedItemTypes
		{
			get {
				return new Type[] {
					typeof (IRunnableItem),
					// Files can be run if they're executable.
					typeof (FileItem),
				};
			}
		}

		public override bool SupportsItem (IItem item)
		{
			if (item is FileItem) {
				return FileItem.IsExecutable (item as FileItem);
			}
			return true;
		}
		
		public override IItem[] Perform (IItem[] items, IItem[] modifierItems)
		{
			foreach (IItem item in items) {
				if (item is IRunnableItem) {
					(item as IRunnableItem).Run ();
				} else if (item is FileItem) {
					System.Diagnostics.Process proc;
					
					proc = new System.Diagnostics.Process ();
					proc.StartInfo.FileName = (item as FileItem).Path;
					proc.StartInfo.UseShellExecute = false;
					proc.Start ();
				}
			}
			return null;
		}
		
	}
}
