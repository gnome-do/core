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
			this.icon = "document";
		}
		
		public string Name {
			get { return name; }
		}
		
		public string Icon {
			get { return icon; }
		}
		
		public string Uri {
			get { return uri; }
		}
		
		public virtual void Open ()
		{
		}
	}
}
