// DockItemProvider.cs
// 
// Copyright (C) 2008 GNOME Do
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using Do.Interface;
using Do.Universe;
using Do.Platform;

using Docky.Utilities;

using Wnck;

namespace Docky.Interface
{
	public enum IconSource {
		Statistics,
		Custom,
		Application,
		Unknown,
	}
	
	public class DockItemProvider
	{
		public delegate void DockItemsChangedHandler (IEnumerable<IDockItem> items);
		public event DockItemsChangedHandler DockItemsChanged;
		
		Dictionary<string, IDockItem> custom_items;
		List<IDockItem> statistical_items, task_items;
		bool enable_serialization = true;
		
		string DesktopFilesPath {
			get {
				return Path.Combine (Services.Paths.UserDataDirectory, "dock_desktop_files");
			}
		}
		
		public List<IDockItem> DockItems {
			get {
				List<IDockItem> out_items = new List<IDockItem> ();
				out_items.Add (MenuItem);
				out_items.AddRange (statistical_items);
				
				if (custom_items.Any ()) {
					out_items.AddRange (custom_items.Values);
				}
				if (task_items.Any ()) {
					if (out_items.Any ())
						out_items.Add (Separator);
					out_items.AddRange (task_items);
				}
				return out_items;
			}
		}
		
		SeparatorItem Separator { get; set; }
		DoDockItem MenuItem { get; set; }
		
		public DockItemProvider ()
		{
			Separator = new SeparatorItem ();
			MenuItem = new DoDockItem ();
			
			custom_items = new Dictionary<string, IDockItem> ();
			statistical_items = new List<IDockItem> ();
			task_items = new List<IDockItem> ();
			
			
			Wnck.Screen.Default.WindowClosed += delegate(object o, WindowClosedArgs args) {
				if (args.Window.IsSkipTasklist)
					return;
				UpdateItems ();
			};
			
			Wnck.Screen.Default.WindowOpened += delegate(object o, WindowOpenedArgs args) {
				if (args.Window.IsSkipTasklist)
					return;
				UpdateItems ();
			};
			
			GLib.Timeout.Add (3000, delegate {
				enable_serialization = false;
				foreach (string s in DeserializeCustomItems ())
					AddCustomItem (s);
				enable_serialization = true;
				
				UpdateItems ();
				return false;
			});
		}
		
		public void AddCustomItem (Element item)
		{
			if (!(item is Item)) {
				Log.Error ("Could not add {0} to custom items for dock", item.Safe.Name);
				return;
			}
			string id = item.UniqueId;
			DockItem di = new DockItem (item);
			custom_items [id] = di;
			
			if (enable_serialization)
				SerializeCustomItems ();
		}
		
		public void AddCustomItem (string identifier)
		{
			if (identifier.StartsWith ("file://"))
				identifier = identifier.Substring ("file://".Length);
			
			if (File.Exists (identifier)) {
				if (identifier.EndsWith (".desktop")) {
					Element o = Services.UniverseFactory.NewApplicationItem (identifier) as Element;
					custom_items [identifier] = new DockItem (o);
				} else {
					Element o = Services.UniverseFactory.NewFileItem (identifier) as Element;
					custom_items [identifier] = new DockItem (o);
				}
			} else {
				Element e = Services.Core.GetElement (identifier);
				if (e != null)
					custom_items [identifier] = new DockItem (e);
				else
					Log.Error ("Could not add custom item with id: {0}", identifier);
			}
			
			if (enable_serialization)
				SerializeCustomItems ();
		}
		
		string[] DeserializeCustomItems ()
		{
			string[] filenames;
			try {
				using (Stream s = File.OpenRead (DesktopFilesPath)) {
					BinaryFormatter f = new BinaryFormatter ();
					filenames = f.Deserialize (s) as string[];
				}
			} catch (FileNotFoundException e) {
				Log.Debug ("Custom items file not present, nothing to add. " + e.Message);
				filenames = new string[0];
			} catch {
				Log.Error ("Could not deserialize custom items");
				filenames = new string[0];
			}
			return filenames;
		}
		
		public void ForceUpdate ()
		{
			UpdateItems ();
		}
		
		public IconSource GetIconSource (IDockItem item) {
			if (task_items.Contains (item))
				return IconSource.Application;
			
			if (statistical_items.Contains (item))
				return IconSource.Statistics;
			
			if (custom_items.Values.Contains (item))
				return IconSource.Custom;
			
			return IconSource.Unknown;
		}
		
		IEnumerable<Item> MostUsedItems ()
		{
			return Services.Core
				.GetItemsOrderedByRelevance ()
				.Where (item => item.GetType ().Name != "SelectedTextItem")
				.Where (item => !DockPreferences.ItemBlacklist.Contains (item.UniqueId))
				.Take (DockPreferences.AutomaticIcons)
				.OrderByDescending (item => item is IApplicationItem)
				.ThenBy (item => item.GetType ().Name)
				.ThenBy (item => item.Safe.Name);
		}
		
		public bool RemoveItem (int item)
		{
			bool ret_val = false;
			
			if (GetIconSource (DockItems [item]) == IconSource.Statistics) {
				DockPreferences.AddBlacklistItem ((DockItems [item] as DockItem).Element.UniqueId);
				UpdateItems ();
				ret_val = true;
			} else if (GetIconSource (DockItems [item]) == IconSource.Custom) {
				foreach (KeyValuePair<string, IDockItem> kvp in custom_items) {
					if (kvp.Value.Equals (DockItems [item])) {
						custom_items.Remove (kvp.Key);
						
						UpdateItems ();
						ret_val = true;
						break;
					}
				}
			}
			
			UpdateItems ();
			if (enable_serialization)
				SerializeCustomItems ();
			return ret_val;
		}
		
		void SerializeCustomItems ()
		{
			try {
				using (Stream s = File.OpenWrite (DesktopFilesPath)) {
					BinaryFormatter f = new BinaryFormatter ();
					f.Serialize (s, custom_items.Keys.ToArray ());
				}
			} catch {
				Log.Error ("Could not serialize custom items");
			}
		}
		
		void UpdateItems ()
		{
			List<IDockItem> new_items = new List<IDockItem> ();
			foreach (Item item in MostUsedItems ()) {
				if (DockPreferences.ItemBlacklist.Contains (item.UniqueId))
					continue;
				IDockItem di = new DockItem (item);
				if (custom_items.Values.Contains (di)) {
					di.Dispose ();
					continue;
				}
				new_items.Add (di);
				
				bool is_set = false;
				foreach (IDockItem ditem in statistical_items) {
					if (ditem.Equals (di)) {
						di.DockAddItem = ditem.DockAddItem;
						is_set = true;
						break;
					}
				}
				if (!is_set)
					di.DockAddItem = DateTime.UtcNow;
			}
			
			foreach (IDockItem dock_item in statistical_items)
				dock_item.Dispose ();
			
			statistical_items = new_items;
			UpdateWindowItems ();
			if (DockItemsChanged != null)
				DockItemsChanged (DockItems);
			
		}
		
		void UpdateWindowItems ()
		{
			foreach (IDockItem di in statistical_items.Concat (custom_items.Values)) {
				if (!(di is DockItem))
					continue;
				(di as DockItem).UpdateApplication ();
			}
			
			if (Wnck.Screen.Default.ActiveWorkspace == null)
				return;
			List<IDockItem> out_items = new List<IDockItem> ();
			
			foreach (Wnck.Application app in WindowUtils.GetApplications ()) {
				bool good = false;
				//we dont want any applications that dont have any "real" windows
				foreach (Wnck.Window w in app.Windows) {
					if (!w.IsSkipTasklist)
						good = true;
				}
				
				// Anything we already have, we dont need additional copies of
				foreach (IDockItem di in statistical_items.Concat (custom_items.Values)) {
					if (!(di is DockItem) || (di as DockItem).Apps.Count () == 0)
						continue;
					if ((di as DockItem).Pids.Contains (app.Pid)) {
						//we found a match already, mark as not good
						good = false;
						break;
					}
				}
						
				if (!good) 
					continue;
				
				ApplicationDockItem api = new ApplicationDockItem (app);
				bool is_set = false;
				foreach (ApplicationDockItem di in task_items) {
					if (api.Equals (di)) {
						api.DockAddItem = di.DockAddItem;
						is_set = true;
						break;
					}
				}
				if (!is_set)
					api.DockAddItem = DateTime.UtcNow;
				out_items.Add (new ApplicationDockItem (app));
			}
			
			foreach (IDockItem item in task_items)
				item.Dispose ();
					
			task_items = out_items;
		}
	}
}
