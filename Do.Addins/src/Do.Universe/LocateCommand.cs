/* LocateCommand.cs
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
using System.Collections.Generic;

namespace Do.Universe
{
	public class LocateCommand : AbstractCommand
	{
		
		bool allowHidden = false;
		uint maxResults = 100;
		
		public override string Name
		{
			get { return "Locate"; }
		}
		
		public override string Description
		{
			get { return "Search your filesystem using locate."; }
		}
		
		public override string Icon
		{
			get { return "search"; }
		}
		
		public override Type[] SupportedItemTypes
		{
			get {
				return new Type[] {
					typeof (ITextItem)
				};
			}
		}
		
		public override IItem[] Perform (IItem[] items, IItem[] modifierItems)
		{
			List<IItem> files;
			System.Diagnostics.Process locate;
			string query, file_list;
				
			files = new List<IItem> ();
			query = (items[0] as ITextItem).Text;
			
			locate = new System.Diagnostics.Process ();
			locate.StartInfo.FileName = "locate";
			locate.StartInfo.Arguments = string.Format ("-i -n {0} {1}", maxResults, query);
			locate.StartInfo.RedirectStandardOutput = true;
			locate.StartInfo.UseShellExecute = false;
			try {
				locate.Start ();
				locate.WaitForExit ();
				file_list = locate.StandardOutput.ReadToEnd ();
			} catch {
				Console.Error.WriteLine ("LocateCommand error: The program 'locate' could not be found.");
				file_list = "";
			}
			foreach (string path in file_list.Split ('\n')) {
				if (!System.IO.File.Exists (path) &&
						!System.IO.Directory.Exists (path)) continue;
				// Don't allow files in hidden directories (like .svn directories).
				if (!allowHidden &&
						System.IO.Path.GetDirectoryName (path).Contains ("/.")) continue;
				files.Add (FileItem.Create (path));
			}
			return files.ToArray ();
		}
	}
}
