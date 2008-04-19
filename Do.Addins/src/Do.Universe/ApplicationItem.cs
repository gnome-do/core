/* ApplicationItem.cs
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
using System.Runtime.InteropServices;

using Gnome;
using Mono.Unix;

namespace Do.Universe {

	public class ApplicationItem : IRunnableItem {
		
		protected DesktopItem item;
		
		/// <summary>
		/// Create an application item from a desktop file location.
		/// </summary>
		/// <param name="desktopFile">
		/// A <see cref="System.String"/> containing the absolute path of
		/// a desktop (.desktop) file.
		/// </param>
		public ApplicationItem (string desktopFile)
		{
			IntPtr desktopFileP;
			
			desktopFileP = gnome_desktop_item_new_from_file (
				desktopFile, 0, IntPtr.Zero);			

			if (desktopFileP == IntPtr.Zero)
				throw new Exception ("Failed to load desktop file "
					+ desktopFile);

			item = new DesktopItem (desktopFileP);

			// We may need to call this depending on how DesktopItem works.
			// gnome_desktop_item_unref (desktopFileP);
		}
		
		public string Name
		{
			get {
				return item.GetLocalestring ("Name");
			}
		}

		public string Description
		{
			get {
				return item.GetLocalestring ("Comment");
			}
		}
		
		public string Icon
		{
			get {
				return item.GetString ("Icon");
			}
		}
		
		public string Exec {
			get {
				return item.GetString ("Exec");
			}
		}

		public bool Hidden
		{
			get {
				return item.GetBoolean ("NoDisplay");
			}
		}
		
		/// <summary>
		/// Executes the application.
		/// </summary>
		public void Run ()
		{
			item.Launch (null, DesktopItemLaunchFlags.OnlyOne);
		}

		public void RunWithUris (IEnumerable<string> uris)
		{
			item.Launch (IEnumerableToList<string> (uris),
			             DesktopItemLaunchFlags.OnlyOne);
		}
		
		/// <summary>
		/// Simple helper function to convert an IEnumerable<T> to a
		/// GLib.List.
		/// </summary>
		/// <param name="es">
		/// A <see cref="IEnumerable`1"/>.
		/// </param>
		/// <returns>
		/// A <see cref="GLib.List"/> representation of the IEnumerable.
		/// </returns>
		GLib.List IEnumerableToList<T> (IEnumerable<T> es)
		{
			object [] arr;
	
			arr = new List<T> (es).ToArray ();
			return new GLib.List (arr, typeof (T), false, true);
		}
		
		[DllImport ("libgnome-desktop-2.so.2")]
		private static extern IntPtr gnome_desktop_item_new_from_file (string file, int flags, IntPtr error);

		[DllImport ("libgnome-desktop-2.so.2")]
		private static extern void gnome_desktop_item_unref (IntPtr item);
	}
}
