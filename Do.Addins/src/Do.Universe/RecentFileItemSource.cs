/* ${FileName}
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
	public class RecentFileItemSource : IItemSource
	{
		List<IItem> files;
		
		public RecentFileItemSource()
		{
			files = new List<IItem> ();
			Gtk.RecentManager.Default.Changed += OnRecentChanged;

			ForceUpdateItems ();
		}
		
		protected void OnRecentChanged (object sender, EventArgs args)
		{
			ForceUpdateItems ();
		}
		
		public Type[] SupportedItemTypes
		{
			get { return new Type[] {
					typeof (FileItem),
				};
			}
		}
		
		public string Name
		{
			get { return "Recent Files"; }
		}
		
		public string Description
		{
			get { return "Finds recently-opened files."; }
		}
		
		public string Icon
		{
			get { return "document"; }
		}
		
		public ICollection<IItem> Items {
			get { return files; }
		}
		
		public ICollection<IItem> ChildrenOfItem (IItem item) {
			return null;
		}
		
		public void UpdateItems ()
		{
		}
		
		protected virtual void ForceUpdateItems ()
		{
			/*
			foreach (IntPtr info_ptr in Gtk.RecentManager.Default.Items) {
				Console.WriteLine ("Recent items source adding item: {0}", info);
				files.Add (new FileItem (info.DisplayName, info.Uri));
			}
			*/
		}
	}
}
