/* FileItemActions.cs
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

using Do.Universe;

namespace Do.Files {

	class CopyToAction : IAction {

		public string Name { get { return "Copy to..."; } } 
		public string Description { get { return "Copies a file or folder to another location."; } } 
		public string Icon { get { return "gtk-copy"; } } 

		public Type [] SupportedItemTypes {
			get {
				return new Type [] {
					typeof (FileItem),
				};
			}
		}

		public Type [] SupportedModifierItemTypes {
			get {
				return new Type [] {
					typeof (IFileItem),
				};
			}
		}

		public bool ModifierItemsOptional {
			get { return false; }
		}

		public bool SupportsItem (IItem item)
		{
			return true;
		}

		public bool SupportsModifierItemForItems (IItem [] items, IItem modItem)
		{
			return FileItem.IsDirectory (modItem as IFileItem);
		}

		public IItem [] DynamicModifierItemsForItem (IItem item)
		{
			return null;
		}

		public IItem [] Perform (IItem [] items, IItem [] modItems)
		{
			IFileItem dest;
			List<string> seenPaths;

			dest = modItems [0] as IFileItem;
			seenPaths = new List<string> ();
			foreach (FileItem src in items) {
				if (seenPaths.Contains (src.Path)) continue;
				try {
					System.Diagnostics.Process.Start ("cp",
							string.Format ("-r {0} {1}",
								FileItem.EscapedPath (src), FileItem.EscapedPath (dest)));

					seenPaths.Add (src.Path);
					src.Path = Path.Combine (dest.Path, Path.GetFileName (src.Path));
				} catch (Exception e) {
					Console.Error.WriteLine ("CopyToAction could not copy "+
							src.Path + " to " + dest.Path + ": " + e.Message);
				}
			}
			return null;
		}
	}

	class MoveToAction : IAction {

		public string Name { get { return "Move to..."; } } 
		public string Description { get { return "Moves a file or folder to another location."; } } 
		public string Icon { get { return "forward"; } } 

		public Type [] SupportedItemTypes {
			get {
				return new Type [] {
					typeof (FileItem),
				};
			}
		}

		public Type [] SupportedModifierItemTypes {
			get {
				return new Type [] {
					typeof (IFileItem),
				};
			}
		}

		public bool ModifierItemsOptional {
			get { return false; }
		}

		public bool SupportsItem (IItem item)
		{
			return true;
		}

		public bool SupportsModifierItemForItems (IItem [] items, IItem modItem)
		{
			return items.Length == 1 ||
				FileItem.IsDirectory (modItem as IFileItem);
		}

		public IItem [] DynamicModifierItemsForItem (IItem item)
		{
			return null;
		}

		public IItem [] Perform (IItem [] items, IItem [] modItems)
		{
			IFileItem dest;
			List<string> seenPaths;

			dest = modItems [0] as IFileItem;
			seenPaths = new List<string> ();
			foreach (FileItem src in items) {
				if (seenPaths.Contains (src.Path)) continue;
				try {
					System.Diagnostics.Process.Start ("mv",
							string.Format ("{0} {1}",
								FileItem.EscapedPath (src), FileItem.EscapedPath (dest)));
					seenPaths.Add (src.Path);

					if (FileItem.IsDirectory (dest)) {
						src.Path = Path.Combine (dest.Path,
								Path.GetFileName (src.Path));
					} else {
						src.Path = dest.Path;
					}
				} catch (Exception e) {
					Console.Error.WriteLine ("MoveToAction could not move "+
							src.Path + " to " + dest.Path + ": " + e.Message);
				}
			}
			return null;
		}
	}

	class MoveToTrashAction : AbstractAction {

		public override string Name { get { return "Move to Trash"; } } 
		public override string Description { get { return "Moves a file or folder to the trash."; } } 
		public override string Icon { get { return "user-trash-full"; } } 

		string Trash {
			get { 
				return Paths.Combine (
						Paths.ReadXdgUserDir ("XDG_DATA_HOME", ".local/share"),
						"Trash/files");
			}
		}

		public override Type [] SupportedItemTypes {
			get {
				return new Type [] {
					typeof (FileItem),
				};
			}
		}

		public override IItem [] Perform (IItem [] items, IItem [] modItems)
		{
			List<string> seenPaths;

			seenPaths = new List<string> ();
			foreach (FileItem src in items) {
				if (seenPaths.Contains (src.Path)) continue;
				try {
					System.Diagnostics.Process.Start ("mv",
							string.Format ("{0} {1}", FileItem.EscapedPath (src), Trash));
					seenPaths.Add (src.Path);
					src.Path = Path.Combine (Trash, Path.GetFileName (src.Path));
				} catch (Exception e) {
					Console.Error.WriteLine ("MoveToTrashAction could not move "+
							src.Path + " to the trash: " + e.Message);
				}
			}
			return null;
		}
	}

	abstract class DeleteAction : AbstractAction {

		public override string Name { get { return "Delete File"; } } 
		public override string Description { get { return "Deletes a file or folder."; } } 
		public override string Icon { get { return "gtk-delete"; } } 

		public override Type [] SupportedItemTypes {
			get {
				return new Type [] {
					typeof (IFileItem),
				};
			}
		}

		public override IItem [] Perform (IItem [] items, IItem [] modItems)
		{
			foreach (IFileItem src in items) {
				try {
					System.Diagnostics.Process.Start ("rm",
							string.Format ("-rf {0}", FileItem.EscapedPath (src)));
				} catch (Exception e) {
					Console.Error.WriteLine ("DeleteFileAction could not delete "+
							src.Path + ": " + e.Message);
				}
			}
			return null;
		}
	}
}
