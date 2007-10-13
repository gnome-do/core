// FileItem.cs created with MonoDevelop
// User: dave at 2:25 PM 9/13/2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.IO;
using System.Collections;

namespace Do.Universe
{
	
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
		
		public static bool RegisterExtensionForFileItemType (string ext, Type fi_type)
		{
			if (extensionTypes.ContainsKey (ext)) {
				return false;
			}
			extensionTypes[ext] = fi_type;
			return true;
		}
		
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
		
		public static string ShortUri (string uri) {
			string home;
			
			uri = (uri == null ? "" : uri);
			home = System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal);
			uri = uri.Replace (home, "~");
			return uri;
		}
		
		string uri, name, icon, mime_type;
		
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
			get { return ShortUri (uri); }
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
