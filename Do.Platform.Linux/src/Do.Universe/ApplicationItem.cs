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
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Gnome;
using Mono.Unix;

using Do.Universe;

namespace Do.Universe.Linux {

	public class ApplicationItem : IApplicationItem {
		
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
			item = DesktopItem.NewFromFile (desktopFile, DesktopItemLoadFlags.OnlyIfExists);

			name = item.GetLocalestring ("Name");
			description = item.GetLocalestring ("Comment");
			icon = item.GetString ("Icon");
		}
		
		public string Name {
			get { return name; }
		}

		public string Description {
			get { return description; }
		}
		
		public string Icon {
			get { return icon; }
		}

		public bool NoDisplay {
			get {
				return item.AttrExists ("NoDisplay") && item.GetBoolean ("NoDisplay");
			}
		}
		
		public string Exec {
			get { return item.GetString ("Exec"); }
		}

		public bool Hidden {
			get { return item.GetBoolean ("NoDisplay"); }
		}
		
		public bool IsUserCustomItem {
			get { return item.Location.StartsWith ("file:///home"); }
		}

		public bool IsAppropriateForCurrentDesktop {
			get {
				// This check should eventually account for xfce too.  Ideally here
				// though, we wish to throw away certain items that are not useful to
				// the current DE.  We are using the same check that xdg-open uses.
				if (!item.AttrExists ("OnlyShowIn")) return true;

				string show_in = item.GetString ("OnlyShowIn").ToLower ();
				return !show_in.Contains ("kde") || 
					Environment.GetEnvironmentVariable ("KDE_FULL_SESSION") == "true";
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

		public void LaunchWithFiles (IEnumerable<IFileItem> files)
		{
			string [] uris = files.Select (file => file.Uri).ToArray ();
			GLib.List glist = new GLib.List (uris as object[], typeof (string), false, true);
			item.Launch (glist, DesktopItemLaunchFlags.OnlyOne);
		}
	}
}
