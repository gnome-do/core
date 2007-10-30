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

namespace Do.Universe
{

	/// <summary>
	/// FileItem is an item describing a file. FileItem subclasses
	/// can be created and registered with FileItem for instantiation
	/// in the factory method FileItem.Create (string uri).
	/// </summary>
	public class FileItem : IFileItem
	{

		static Hashtable extensionTypes;
		
		static FileItem ()
		{
			string[] extentions;
			
			extensionTypes = new Hashtable ();
			
			// Register extensions for specialized subclasses.
			// See note in ImageFileItem.cs
			extentions = new string[] { "jpg", "jpeg", "png", "gif" };
			foreach (string ext in extentions) {
				FileItem.RegisterExtensionForFileItemType (ext, typeof (ImageFileItem));
			}
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
		public static FileItem Create (string uri)
		{
			string ext;
			Type fi_type;
			FileItem result;
			
			ext = System.IO.Path.GetExtension (uri).ToLower ();
			if (ext.StartsWith (".")) {
				ext = ext.Substring (1);
			}
			if (extensionTypes.ContainsKey (ext)) {
				fi_type = extensionTypes[ext] as Type;
			} else {
				fi_type = typeof (FileItem);
			}
			try {
				result = (FileItem) System.Activator.CreateInstance (fi_type, new string[] {uri});
			} catch {
				result = new FileItem (uri);
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
		public static string ShortUri (string uri) {
			string home;
			
			uri = (uri == null ? "" : uri);
			home = System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal);
			uri = uri.Replace (home, "~");
			return uri;
		}
		
		string uri, name, icon, mime_type;
		
		/// <summary>
		/// Create a new FileItem for a given file.
		/// </summary>
		/// <param name="uri">
		/// A <see cref="System.String"/> containing an absolute path to a file.
		/// </param>
		public FileItem (string uri)
		{	
			this.uri = uri;
			this.name = Path.GetFileName (uri);
			this.mime_type = Gnome.Vfs.Global.GetMimeType (uri);

			if (System.IO.Directory.Exists (uri)) {
				icon = "folder";
			} else {
				try {
					icon = mime_type.Replace ('/', '-');
					icon = string.Format ("gnome-mime-{0}", icon);
				} catch (NullReferenceException) {
					icon = "file";
				}
			}
		}
		
		public virtual string Name {
			get { return name; }
		}
		
		public virtual string Description {
			get {
				string uri_short;
				
				uri_short = ShortUri (uri);
				if (uri_short == "~")
					// Sowing only "~" looks too abbreviated.
					return uri;
				else
					return uri_short;
			}
		}
		
		public virtual string Icon {
			get { return icon; }
		}
		
		public string URI {
			get { return uri; }
		}
		
		public string MimeType {
			get { return mime_type; }
		}

	}
}
