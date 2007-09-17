// FileItem.cs created with MonoDevelop
// User: dave at 2:25 PM 9/13/2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using Do.PluginLib;

namespace Do.PluginLib.Builtin
{
	
	public class FileItem : IFileItem
	{
		string uri, name, icon;
		
		public FileItem (string name, string uri)
		{	
			this.uri = uri;
			this.name = name;
			
			if (System.IO.Directory.Exists (uri)) {
				icon = "folder";
			} else {
				try {
					icon = Gnome.Vfs.Global.GetMimeType (uri);
					icon = icon.Replace ('/', '-');
					icon = string.Format ("gnome-mime-{0}", icon);
				} catch (NullReferenceException) {
					icon = "file";
				}
			}
		}
		
		public string Name {
			get { return name; }
		}
		
		public string Description {
			get { return uri; }
		}
		
		public string Icon {
			get { return icon; }
		}
		
		public string Uri {
			get { return uri; }
		}
		
		public virtual void Open ()
		{
			Console.WriteLine ("Opening \"{0}\"...", uri);
			try {
				System.Diagnostics.Process.Start ("gnome-open", string.Format ("\"{0}\"", uri));
			} catch (Exception e) {
				Console.WriteLine ("Failed to open \"{0}\": ", e.Message);
			}
		}
	}
}
