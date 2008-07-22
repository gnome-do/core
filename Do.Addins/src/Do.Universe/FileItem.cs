/* FileItem.cs
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

namespace Do.Universe {

	/// <summary>
	/// FileItem is an item describing a file. FileItem subclasses
	/// can be created and registered with FileItem for instantiation
	/// in the factory method FileItem.Create.
	/// </summary>
	public class FileItem : IFileItem, IOpenableItem {
		
		static FileItem ()
		{
			Gnome.Vfs.Vfs.Initialize ();
		}

		/// <summary>
		/// Abbreviates an absolute path by replacing $HOME with ~.
		/// </summary>
		/// <param name="uri">
		/// A <see cref="System.String"/> containing a path.
		/// </param>
		/// <returns>
		/// A <see cref="System.String"/> containing the abbreviated path.
		/// </returns>
		public static string ShortPath (string path)
		{
			if (null == path)
				throw new ArgumentNullException ();

			return path.Replace (Paths.UserHome, "~");
		}
		
		public static bool IsExecutable (IFileItem fi)
		{
			if (fi == null) return false;
			return IsExecutable (fi.Path);
		}

		public static bool IsExecutable (string path)
		{
			UnixFileInfo info;

			if (Directory.Exists (path)) return false;

			info = new UnixFileInfo (path);
			return (info.FileAccessPermissions &
				FileAccessPermissions.UserExecute) != 0;
		}
		
		public static bool IsHidden (IFileItem fi)
		{
			if (fi == null) return false;
			return IsHidden (fi.Path);
		}

		public static bool IsHidden (string path)
		{
			FileInfo info;

			if (path.EndsWith ("~")) return true;

			info = new FileInfo (path);
			return (info.Attributes & FileAttributes.Hidden) != 0;
		}

		public static bool IsDirectory (IFileItem fi)
		{
			return IsDirectory (fi.Path);
		}


		public static bool IsDirectory (string path)
		{
			return Directory.Exists (path);
		}

        public static string EscapedPath (IFileItem fi)
        {
            return EscapedPath (fi.Path);
        }

        public static string EscapedPath (string path)
        {
            return path
				.Replace (" ", "\\ ")
				.Replace ("'", "\\'");
        }
		
		protected string path, name, description, icon;
		
		/// <summary>
		/// Create a new FileItem for a given file.
		/// </summary>
		/// <param name="path">
		/// A <see cref="System.String"/> containing an absolute path to a file.
		/// </param>
		public FileItem (string path)
		{	
			this.path = path;
			this.name = System.IO.Path.GetFileName (Path);
			
			string short_path;
				
			short_path = ShortPath (Path);
			if (short_path == "~")
				// Sowing only "~" looks too abbreviated.
				description = Path;
			else
				description = short_path;
			
			icon = MimeType;
			try {
				if (icon == "x-directory/normal") {
					icon = "folder";
				} else if (icon.StartsWith ("image")) {
					icon = "gnome-mime-image";
				} else {
					icon = icon.Replace ('/', '-');
					icon = string.Format ("gnome-mime-{0}", icon);
				}
			} catch (NullReferenceException) {
				icon = "gtk-file";
			}
		}
		
		public virtual string Name {
			get {
				return name;
			}
		}
		
		public virtual string Description {
			get {
				return description;
			}
		}
		
		public virtual string Icon {
			get {
				return icon;
			}
		}
		
		public string Path {
			get { return path; }
			set { path = value; }
		}
		
		public string URI {
			get {
				return "file://" + Path;
			}
		}
		
		public string MimeType {
			get {
				return Gnome.Vfs.Global.GetMimeType (Path);
			}
		}

		public virtual void Open ()
		{
			Do.Addins.Util.Environment.Open (EscapedPath (this));
		}
	}
}
