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
using System.IO;
using System.Runtime.InteropServices;

using Gnome;
using Mono.Unix;

namespace Do.Universe {

	public class ApplicationItem : IRunnableItem {
		
		protected DesktopItem item;
		string name, description, icon, mimetype;

		/// <summary>
		/// Create an application item from a desktop file location.
		/// </summary>
		/// <param name="desktopFile">
		/// A <see cref="System.String"/> containing the absolute path of
		/// a desktop (.desktop) file.
		/// </param>
		public ApplicationItem (string desktopFile)
		{
			item = DesktopItem.NewFromFile (desktopFile,
				DesktopItemLoadFlags.OnlyIfExists);
			if (null == item)
				throw new Exception (desktopFile + " not found.");
			
			// This check should eventually account for xfce too.  Ideally here though, we wish to throw
			// away certain items that are not useful to the current DE.  We are using the same check
			// that xdg-open uses.
			if (item.AttrExists ("OnlyShowIn")) {
				string show_areas = item.GetString ("OnlyShowIn").ToLower ();
				if (System.Environment.GetEnvironmentVariable ("KDE_FULL_SESSION") == "true") { //in KDE
					if (!show_areas.Contains ("kde"))
						throw new Exception ("Non-KDE Item in KDE");
				} else { //not in KDE
					if (show_areas.Contains ("kde") && !show_areas.Contains ("gnome") && !show_areas.Contains ("xfce"))
						throw new Exception ("KDE Item in GNOME");
				}
			}
			if (item.AttrExists ("NoDisplay")) {
				if (item.GetBoolean ("NoDisplay"))
					throw new Exception ("No Display item detected");
			}
			
			name = item.GetLocalestring ("Name");
			description = item.GetLocalestring ("Comment");
			icon = item.GetString ("Icon");
		}
		
		public string Name {
			get {
				return name;
			}
		}

		public string Description {
			get {
				return description;
			}
		}
		
		public string Icon {
			get {
				return icon;
			}
		}
		
		public string Exec {
			get {
				return item.GetString ("Exec");
			}
		}

		public bool Hidden {
			get {
				return item.GetBoolean ("NoDisplay");
			}
		}
		
		public string[] MimeTypes {
			get {
				if (!item.AttrExists ("MimeType")) {
					return null;
				}
				
				if (!string.IsNullOrEmpty (mimetype))
					return mimetype.Split (';');
				
				string s = item.GetString ("MimeType");
				if (s.Length >= 1000) {
					mimetype = ManualMimeParse () ?? item.GetString ("MimeType");
					return mimetype.Split (';');
				}
				return item.GetString ("MimeType").Split (';');
			}
		}
		
		private string ManualMimeParse ()
		{
			StreamReader reader = new StreamReader (item.Location.Replace ("file://",""));
			while (!reader.EndOfStream) {
				string s = reader.ReadLine ();
				if (!s.Trim ().StartsWith ("MimeType"))
					continue;
				s = s.Replace ("MimeType=", "");
				reader.Dispose ();
				return s.Trim ();
			}
			reader.Dispose ();
			return null;
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
	}
}
