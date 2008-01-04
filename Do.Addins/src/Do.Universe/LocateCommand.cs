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
using System.IO; 
using System.Collections.Generic;

namespace Do.Universe
{
	public class LocateCommand : AbstractCommand
	{
		
		bool allowHidden = false;
		uint maxResults = 1000;
		
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
			} catch {
				Console.Error.WriteLine ("LocateCommand error: The program 'locate' could not be found.");
				return null;
			}

			string path;
			query = query.ToLower ();
			while (null != (path = locate.StandardOutput.ReadLine ())) {
				// Disallow hidden directories in the absolute path.
				// This gets rid of messy .svn directories and their contents.
				if (!allowHidden &&
						Path.GetDirectoryName (path).Contains ("/."))
					continue;

				// Only allow files that contain the query as a substring.
				// It may be faster to use grep, but I've tested this and it
				// seems prety snappy.
				if (Path.GetFileName (path).ToLower().Contains (query))
					files.Add (FileItem.Create (path));
			}
			files.Sort (new FileItemNameComparer (query));
			return files.ToArray ();
		}

		// Order files by (A) position of query in the file name and
		// (B) by length.
		private class FileItemNameComparer : IComparer<IItem>
		{
			string query;

			public FileItemNameComparer (string query)
			{
				this.query = query.ToLower ();
			}

			public int Compare (IItem a, IItem b)
			{
				string a_name_lower, b_name_lower;
				int a_score, b_score;

				a_name_lower = (a as FileItem).Path;
				a_name_lower = Path.GetFileName (a_name_lower).ToLower (); 
				b_name_lower = (b as FileItem).Path;
				b_name_lower = Path.GetFileName (b_name_lower).ToLower (); 

				a_score = a_name_lower.IndexOf (query);
				b_score = b_name_lower.IndexOf (query);

				if (a_score == b_score)
					return a_name_lower.Length - b_name_lower.Length;
				else
					return a_score - b_score;
			}
		}
	}
}
