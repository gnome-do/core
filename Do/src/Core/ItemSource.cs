// ItemSource.cs created with MonoDevelop
// User: dave at 1:08 AM 8/17/2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.Collections.Generic;

using Do.PluginLib;

namespace Do.Core
{
	
	public class ItemSource : GCObject 
	{
	
		public static readonly string DefaultItemSourceName = "";
		public static readonly string DefaultItemSourceDescription = "";
		public static readonly string DefaultItemSourceIcon = "";
		
		private bool enabled;
		protected IItemSource source;
		protected List<Item> items;
		
		public ItemSource (IItemSource source)
		{
			if (source == null) {
				throw new ArgumentNullException ();
			}
			this.source = source;
			items = new List<Item> ();
			foreach (IItem item in source.Items) {
				items.Add (new Item (item));
			}
			enabled = true;
		}
		
		public override string Name {
			get { return (source.Name == null ? DefaultItemSourceName : source.Name); }
		}
		
		public override string Description {
			get { return (source.Description == null ? DefaultItemSourceDescription : source.Description); }
		}
		
		public override string Icon {
			get { return (source.Icon == null ? DefaultItemSourceIcon : source.Icon); }
		}
		
		public bool UpdateItems () {
			if (source.UpdateItems ()) {
				items.Clear ();
				items = new List<Item> ();
				foreach (IItem item in source.Items) {
					items.Add (new Item (item));
				}
				return true;
			} else {
				return false;
			}
		}
		
		public ICollection<Item> Items {
			get { return items; }
		}
		
		public bool Enabled {
			get { return enabled; }
			set { enabled = value; }
		}
		
		public override string ToString ()
		{
			string items_str = GetType().ToString() + " {";
			foreach (Item item in items) {
				items_str = String.Format ("{0}\t{1}\n", items_str, item);
			}
			items_str += "}";
			return items_str;
		}
		
	}
}
