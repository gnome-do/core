// ShelfItemSource.cs
//
//GNOME Do is the legal property of its developers. Please refer to the
//COPYRIGHT file distributed with this
//source distribution.
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
//
//

using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using Do.Universe;
using Do.Core;

namespace Do.Universe
{	
	[Serializable]
	public class ShelfItem : IItem
	{
		private string name;
		private List<string> uids;
		
		[NonSerialized]
		private List<IItem> tempItems = new List<IItem> ();
		
		private List<IItem> TempItems {
			get {
				return tempItems ?? tempItems = new List<IItem> ();
			}
		}
		
		public ShelfItem (string name)
		{
			this.name = name;
			uids = new List<string> ();
		}
		
		public string ShelfName {
			get {
				return name;
			}
		}
		
		public List<IItem> Items {
			get {
				List<IItem> items = new List<IItem> ();
				foreach (string uid in uids) {
					IObject item;
					Do.UniverseManager.TryGetObjectForUID (uid, out item);
					if (item != null && item is IItem)
						items.Add (item as IItem);
				}
				
				items.AddRange (TempItems);
				
				return items;
			}
		}
		
		public string Name {
			get {
				return name + " Shelf";
			}
		}
		
		public string Description {
			get {
				return "Your " + name + " Shelf Items";
			}
		}

		public string Icon {
			get {
				return "folder-saved-search";
			}
		}
		
		public void AddItem (IItem item)
		{
			string uid = Do.UniverseManager.UIDForObject (item);
			if (uids.Contains (uid)) return;
		
			//certain objects are not really in universe.  We still want to hold on to these
			//so we have to keep the object for ourself.  We will not serialize these however.
			IObject obj;
			Do.UniverseManager.TryGetObjectForUID (uid, out obj);
			
			if (obj == null) {
				if (!TempItems.Contains (item))
				    TempItems.Add (item); //temp items
			} else {
				uids.Add (uid); //real items
			}
			ShelfItemSource.Serialize ();
		}
		
		public void RemoveItem (IItem item)
		{
			if (TempItems.Remove (item)) return;
			
			string uid = Do.UniverseManager.UIDForObject (item);
			
			uids.Remove (uid);
			ShelfItemSource.Serialize ();
		}
	}
	
	public class ShelfItemSource : IItemSource
	{	
		static string ShelfFile {
			get {
				return Paths.Combine (Paths.UserData, "shelf");
			}
		}
		
		private void Deserialize () 
		{
			try {
				using (Stream s = File.OpenRead (ShelfFile)) {
					BinaryFormatter f = new BinaryFormatter ();
					shelf = f.Deserialize (s) as Dictionary<string,ShelfItem>;
				}
			} catch (FileNotFoundException) {
				shelf = new Dictionary<string,ShelfItem> ();
			} catch (Exception e) {
				Log.Error (e.Message);
			}
		}
		
		static public void Serialize ()
		{
			try {
				using (Stream stream = File.OpenWrite (ShelfFile)) {
					BinaryFormatter f = new BinaryFormatter ();
					f.Serialize (stream, shelf);
				}
			} catch (Exception e) {
				Log.Error (e.Message);
			}
		}
		
		static Dictionary<string,ShelfItem> shelf;
		static string defaultName;
		
		public string Name {
			get {
				return "Shelf Item Source";
			}
		}

		public string Description {
			get {
				return "Your Shelf Items";
			}
		}

		public string Icon {
			get {
				return "folder-saved-search";
			}
		}

		public Type[] SupportedItemTypes {
			get {
				return new Type [] {
					typeof (ShelfItem),
				};
			}
		}

		public ICollection<IItem> Items {
			get {
				List<IItem> items = new List<IItem> ();
				if (shelf != null) {
					foreach (IItem item in shelf.Values)
						items.Add (item);
				}
				
				return items;
			}
		}
		
		public ICollection<IItem> ChildrenOfItem (IItem item)
		{
			return (item as ShelfItem).Items;
		}

		static ShelfItemSource ()
		{
			shelf = new Dictionary<string,ShelfItem> ();
		}
		
		public ShelfItemSource ()
		{
			Deserialize ();
			defaultName = "Default";
			
			if (shelf.Count == 0) {
				shelf.Add (defaultName, new ShelfItem (defaultName));
			}
		}

		public void UpdateItems ()
		{
		}
		
		static public void AddToDefault (IItem item)
		{
			shelf[defaultName].AddItem (item);
			Serialize ();
		}
		
		static public void RemoveFromDefault (IItem item)
		{
			shelf[defaultName].RemoveItem (item);
			Serialize ();
		}
		
		static public ShelfItem NewShelf (string name)
		{
			ShelfItem newShelf = new ShelfItem (name);
			shelf.Add (name, newShelf);
			Do.UniverseManager.AddItems (new IItem[] { newShelf });

			Serialize ();
			
			return newShelf;
		}
	}
}
