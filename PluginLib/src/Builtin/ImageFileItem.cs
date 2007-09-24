// ImageFileItem.cs created with MonoDevelop
// User: dave at 8:27 PM 9/23/2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using Do.PluginLib.Builtin;

namespace Do.PluginLib.Builtin
{
	
	public class ImageFileItem : FileItem
	{
		/*
		 * This was not being called because no other class references this
		 * class. I am guessing this will be called when FileItem subclasses
		 * are loaded from plugins. For now this code is moved to FileItem.cs
		 * 
		 * TODO: fix this.
		
		static string[] imageExtentions = { "jpg", "jpeg", "png", "gif" };
		
		static ImageFileItem ()
		{
			foreach (string ext in imageExtentions) {
				FileItem.RegisterExtensionForFileItemType (ext, typeof (ImageFileItem));
			}
		}
		*/
		
		public ImageFileItem (string name, string uri)
			: base (name, uri)
		{	
		}
		
		// ImageFileItems use themselves as their icons!
		public override string Icon {
			get { return Uri; }
		}
	}
}
