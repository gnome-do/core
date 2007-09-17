// Item.cs created with MonoDevelop
// User: dave at 1:07 AM 8/17/2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.Collections;
using System.Collections.Generic;

using Do.PluginLib;

namespace Do.Core
{
	public class Item : GCObject
	{
		public static readonly string DefaultItemIcon = "gnome-fs-executable";
		public static readonly string DefaultItemDescription = "";
		
		protected Item parent;
		protected IItem item;
		
		public Item (IItem item)
		{
			if (item == null) {
				throw new ArgumentNullException ();
			}
			this.item = item;
		}
		
		public IItem IItem {
			get { return item; }
		}
		
		public override string Name {
			get { return (item.Name == null ? DefaultItemName : item.Name); }
		}
		
		public override string Description {
			get { return (item.Description == null ? DefaultItemDescription : item.Description); }
		}
		
		public override string Icon {
			get { return (item.Icon == null ? DefaultItemIcon : item.Icon); }
		}
	
	}
}
