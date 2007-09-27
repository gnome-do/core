using System;
using System.Collections.Generic;

using Do.PluginLib;

namespace Do.PluginLib.Builtin
{

	public class BookmarkItem : IOpenableItem
	{
		
		protected string name, url;
		
		public BookmarkItem (string name, string url)
		{
			this.name = name;
			this.url = url;
		}
		
		public string Name {
			get { return name; }
		}
		
		public string Description {
			get { return url; }
		}
		
		public string Icon {
			get { return "www"; }
		}
		
		public string Url {
			get { return url; }
		}

		public void Open ()
		{
			Gnome.Url.Show (Url);
		}
	}
}
