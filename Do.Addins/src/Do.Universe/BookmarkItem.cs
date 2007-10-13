using System;
using System.Collections.Generic;

namespace Do.Universe
{

	public class BookmarkItem : IURLItem
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
		
		public string URL {
			get { return url; }
		}

	}
}
