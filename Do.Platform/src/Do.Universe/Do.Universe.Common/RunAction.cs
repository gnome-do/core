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
using System.Collections.Generic;

using Mono.Unix;

using Do.Platform;

namespace Do.Universe.Common
{

	public class RunAction : Act
	{

		public override string Name {
			get { return Catalog.GetString ("Run"); }
		}
		
		public override string Description {
			get { return Catalog.GetString ("Run an application, script, or other executable."); }
		}
		
		public override string Icon {
			get { return "gnome-run"; }
		}
		
		public override IEnumerable<Type> SupportedItemTypes
		{
			get {
				yield return typeof (IRunnableItem);
				// Files can be run if they're executable.
				yield return typeof (IFileItem);
				// ITextItems canbe run if they're valid command lines.
				yield return typeof (ITextItem);
			}
		}

		public override bool SupportsItem (Item item)
		{
			string command = "";

			if (item is IRunnableItem) return true;

			if (item is IFileItem)
				command = (item as IFileItem).Path;
			else if (item is ITextItem)
				command = (item as ITextItem).Text;

			return Services.Environment.IsExecutable (command);
		}
		
		public override IEnumerable<Item> Perform (IEnumerable<Item> items, IEnumerable<Item> modItems)
		{
			foreach (Item item in items) {
				if (item is IRunnableItem) {
					(item as IRunnableItem).Run ();
				} else if (item is IFileItem) {
					Services.Environment.Execute ((item as IFileItem).Path);
				} else if (item is ITextItem) {
					Services.Environment.Execute ((item as ITextItem).Text);
				}
			}
			return null;
		}

		
		
	}
}
