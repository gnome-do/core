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

using Mono.Unix;

namespace Do.Universe
{
	/// <summary>
	/// If this exception is thrown in the ApplicationItem constructor, the
	/// ApplicationItemSource will catch it and discard the item.
	/// </summary>
	public class ApplicationDetailMissingException: ApplicationException
	{
		public ApplicationDetailMissingException (string message) : base (message)
		{
		}
	}

	public class ApplicationItem : IRunnableItem
	{
		protected string desktopFile;
		protected IntPtr desktopFilePtr;
		
		/// <summary>
		/// Create an application item from a desktop file location.
		/// </summary>
		/// <param name="desktopFile">
		/// A <see cref="System.String"/> containing the absolute path of
		/// a desktop (.desktop) file.
		/// </param>
		public ApplicationItem (string desktopFile)
		{
			this.desktopFile = desktopFile;

			desktopFilePtr = gnome_desktop_item_new_from_file (desktopFile, 0, IntPtr.Zero);
			if (desktopFilePtr == IntPtr.Zero) {
				throw new ApplicationDetailMissingException ("Failed to load launcher");
			}
		}

		~ApplicationItem ()
		{
			if (desktopFilePtr != IntPtr.Zero) {
				gnome_desktop_item_unref (desktopFilePtr);
			}
		}
		
		public string Name
		{
			get {
				return Marshal.PtrToStringAuto (
					gnome_desktop_item_get_localestring (desktopFilePtr, "Name"));
			}
		}

		public string Description
		{
			get {
				return Marshal.PtrToStringAuto (
					gnome_desktop_item_get_localestring (desktopFilePtr, "Comment"));
			}
		}
		
		public string Icon
		{
			get {
				return Marshal.PtrToStringAuto (
				    gnome_desktop_item_get_string (desktopFilePtr, "Icon"));
			}
		}
		
		public string Exec {
			get {
				return Marshal.PtrToStringAuto (
				    gnome_desktop_item_get_string (desktopFilePtr, "Exec"));
			}
		}

		public string MimeTypes
		{
			get {
				return Marshal.PtrToStringAuto (
				    gnome_desktop_item_get_string (desktopFilePtr, "MimeTypes"));
			}
		}
		
		/// <summary>
		/// Executes the application by launching the desktop item given in the
		/// constructor.
		/// </summary>
		public void Run ()
		{
			if (desktopFilePtr == IntPtr.Zero) return;
			gnome_desktop_item_launch (desktopFilePtr, IntPtr.Zero, 0, IntPtr.Zero);
		}

		public void RunWithURIs (ICollection<string> uris)
		{
			string uri_list;

			// FIXME: Hardy/GNOME bug causes Do to run out of memory here...
			return;

			uri_list = "";
			foreach (string uri in uris) {
				uri_list += uri + "\r\n";
			}
			gnome_desktop_item_drop_uri_list (desktopFilePtr, uri_list, 0, IntPtr.Zero);
		}
		
		[DllImport ("libgnome-desktop-2.so.2")]
		private static extern IntPtr gnome_desktop_item_new_from_file (string file, int flags, IntPtr error);

		[DllImport ("libgnome-desktop-2.so.2")]
		private static extern void gnome_desktop_item_unref (IntPtr item);

		[DllImport ("libgnome-desktop-2.so.2")]
		private static extern IntPtr gnome_desktop_item_drop_uri_list (IntPtr item, string list, int flags, IntPtr error);

		[DllImport ("libgnome-desktop-2.so.2")]
		private static extern int gnome_desktop_item_launch (IntPtr item, IntPtr args, int flags, IntPtr error);

		[DllImport ("libgnome-desktop-2.so.2")]
		private static extern IntPtr gnome_desktop_item_get_localestring (IntPtr item, string id);

		[DllImport ("libgnome-desktop-2.so.2")]
		private static extern IntPtr gnome_desktop_item_get_string (IntPtr item, string id);
	}
}
