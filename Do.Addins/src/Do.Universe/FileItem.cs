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
using System.Collections;

using Mono.Unix;

using Do.Addins;

namespace Do.Universe
{
	/// <summary>
	/// FileItem is an item describing a file. FileItem subclasses
	/// can be created and registered with FileItem for instantiation
	/// in the factory method FileItem.Create.
	/// </summary>
	public class FileItem : IFileItem, IOpenableItem
	{
		static Hashtable extensionTypes;
		
		static FileItem ()
		{
			extensionTypes = new Hashtable ();
		}
		
		/// <summary>
		/// Register a FileItem subtype for use with the FileItem.Create
		/// factory method.
		/// 
		/// For example, FileItem.RegisterExtensionForFileItemType ("jpeg", typeof(JpegFileItem))
		/// will cause FileItem.Create ("/my_picture.jpeg") to return a JpegFileItem.
		/// </summary>
		/// <param name="ext">
		/// A <see cref="System.String"/> containing an extension without the period: "odf".
		/// </param>
		/// <param name="fi_type">
		/// The <see cref="System.Type"/> to be associated with the extension.
		/// </param>
		/// <returns>
		/// A <see cref="bool"/> indicating whether the type of successfully registered.
		/// </returns>
		public static bool RegisterExtensionForFileItemType (string ext, Type fi_type)
		{
			if (fi_type.IsSubclassOf (typeof (FileItem))) {
				return false;
			}
			if (extensionTypes.ContainsKey (ext)) {
				return false;
			}
			extensionTypes[ext] = fi_type;
			return true;
		}
		
		/// <summary>
		/// Given an absolute path to a file, this will create an instance of the
		/// appropriate FileItem subtype based on the file's extension.
		/// 
		/// See FileItem.RegisterExtensionForFileItemType for more information.
		/// </summary>
		/// <param name="uri">
		/// A <see cref="System.String"/> containing an absolute path to a file.
		/// </param>
		/// <returns>
		/// A <see cref="FileItem"/> instance.
		/// </returns>
		public static FileItem Create (string path)
		{
			string ext;
			Type fi_type;
			FileItem result;
			
			if (Directory.Exists (path)) {
				return new DirectoryFileItem (path);
			}
			
			ext = System.IO.Path.GetExtension (path).ToLower ();
			if (ext.StartsWith (".")) {
				ext = ext.Substring (1);
			}
			if (extensionTypes.ContainsKey (ext)) {
				fi_type = extensionTypes[ext] as Type;
			} else {
				fi_type = typeof (FileItem);
			}
			try {
				result = (FileItem) System.Activator.CreateInstance (fi_type, new object[] {path});
			} catch {
				result = new FileItem (path);
			}
			return result;
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
			string home;
			
			path = path ?? "";
			home = System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal);
			path = path.Replace (home, "~");
			return path;
		}
		
		public static bool IsExecutable (FileItem fi)
		{
			if (fi == null) return false;
			return IsExecutable (fi.Path);
		}

		public static bool IsExecutable (string path)
		{
			UnixFileInfo info;

			if (System.IO.Directory.Exists (path)) return false;

			info = new UnixFileInfo (path);
			return (info.FileAccessPermissions & FileAccessPermissions.UserExecute) != 0;
		}
		
		public static bool IsHidden (FileItem fi)
		{
			if (fi == null) return false;
			return IsHidden (fi.Path);
		}

		public static bool IsHidden (string path)
		{
			System.IO.FileInfo info;

			if (path.EndsWith ("~")) return true;

			info = new System.IO.FileInfo (path);
			return (info.Attributes & System.IO.FileAttributes.Hidden) != 0;
		}
		
		protected string path;
		
		/// <summary>
		/// Create a new FileItem for a given file.
		/// </summary>
		/// <param name="path">
		/// A <see cref="System.String"/> containing an absolute path to a file.
		/// </param>
		public FileItem (string path)
		{	
			this.path = path;
		}
		
		public virtual string Name
		{
			get {
				return System.IO.Path.GetFileName (path);
			}
		}
		
		public virtual string Description
		{
			get {
				string short_path;
				
				short_path = ShortPath (path);
				if (short_path == "~")
					// Sowing only "~" looks too abbreviated.
					return path;
				else
					return short_path;
			}
		}
		
		public virtual string Icon
		{
			get {
				string icon;

				try {
					icon = MimeType.Replace ('/', '-');
					icon = string.Format ("gnome-mime-{0}", icon);
					if (icon.StartsWith ("gnome-mime-image")) {
						icon = "gnome-mime-image";
					}
				} catch (NullReferenceException) {
					icon = "file";
				}
				return icon;
			}
		}
		
		public string Path
		{
			get {
				return path;
			}
		}
		
		public string URI
		{
			get {
				return "file://" + path;
			}
		}
		
		public string MimeType
		{
			get {
				return Gnome.Vfs.Global.GetMimeType (path);
			}
		}

		public void Open ()
		{
			Util.Environment.Open (Path);
		}
	}
	
	public class DirectoryFileItem : FileItem
	{
		public DirectoryFileItem (string path) : base (path)
		{
		}

		public override string Icon
		{
			get {
				return "folder";
			}
		}
	}

}
