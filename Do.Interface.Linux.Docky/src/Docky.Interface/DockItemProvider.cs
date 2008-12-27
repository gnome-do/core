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
	
	public delegate void UpdateRequestHandler (object sender, UpdateRequestArgs args);
	public delegate void DockItemsChangedHandler (IEnumerable<IDockItem> items);
	
	public class DockItemProvider
	{
		
		public event DockItemsChangedHandler DockItemsChanged;
		public event UpdateRequestHandler ItemNeedsUpdate;
		
		Dictionary<string, DockItem> custom_items;
		List<DockItem> statistical_items; 
		List<ApplicationDockItem> task_items;
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
				out_items.AddRange (statistical_items.Cast<IDockItem> ());
				
				if (custom_items.Any ()) {
					out_items.AddRange (custom_items.Values.Cast<IDockItem> ());
				}
				
				out_items.Add (Separator);
				if (task_items.Any ()) {
					out_items.AddRange (task_items.Cast<IDockItem> ());
				}
				
				out_items.Add (TrashItem);
				return out_items;
			}
		}
		
		IDockItem Separator { get; set; }
		IDockItem MenuItem { get; set; }
		IDockItem TrashItem { get; set; }
		
		public DockItemProvider ()
		{
			Separator = new SeparatorItem ();
			MenuItem = new DoDockItem ();
			TrashItem = new TrashDockItem ();
			
			custom_items = new Dictionary<string, DockItem> ();
			statistical_items = new List<DockItem> ();
			task_items = new List<ApplicationDockItem> ();
			
			
			Wnck.Screen.Default.WindowClosed += delegate(object o, WindowClosedArgs args) {
				if (args.Window.IsSkipTasklist) return;
				UpdateItems ();
			};
			
			Wnck.Screen.Default.WindowOpened += delegate(object o, WindowOpenedArgs args) {
				if (args.Window.IsSkipTasklist)	return;
				UpdateItems ();
			};
			
			// We give core 3 seconds to update its universe.  Eventually we will need a signal or something,
			// but for now this works.
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
			di.RemoveClicked += HandleRemoveClicked;
			di.UpdateNeeded += HandleUpdateNeeded;
			custom_items [id] = di;
			
			if (enable_serialization)
				SerializeCustomItems ();
		}
		
		public void AddCustomItem (string identifier)
		{
			if (identifier.StartsWith ("file://"))
				identifier = identifier.Substring ("file://".Length);
			
			if (File.Exists (identifier) || Directory.Exists (identifier)) {
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
			if (custom_items.ContainsKey (identifier) && custom_items [identifier] is DockItem) {
				(custom_items [identifier] as IRightClickable).RemoveClicked += HandleRemoveClicked;
				(custom_items [identifier] as DockItem).UpdateNeeded += HandleUpdateNeeded;
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
			if (item is ApplicationDockItem && task_items.Contains (item as ApplicationDockItem))
				return IconSource.Application;
			
			if (item is DockItem && statistical_items.Contains (item as DockItem))
				return IconSource.Statistics;
			
			if (item is DockItem && custom_items.Values.Contains (item as DockItem))
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
				foreach (KeyValuePair<string, DockItem> kvp in custom_items) {
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
			List<DockItem> new_items = new List<DockItem> ();
			IEnumerable<Item> mostUsedItems = MostUsedItems ();
			
			foreach (Item item in mostUsedItems) {
				if (custom_items.Values.Any (di => di.Element == item))
					continue;
				
				if (statistical_items.Any (di => di.Element == item)) {
					new_items.Add (statistical_items.Where (di => di.Element == item).First ());
				} else {
					DockItem di = new DockItem (item);
					di.DockAddItem = DateTime.UtcNow;
					new_items.Add (di);
				}
			}
			
			foreach (DockItem item in statistical_items.Where (di => !new_items.Contains (di))) {
				item.RemoveClicked -= HandleRemoveClicked;
				item.UpdateNeeded -= HandleUpdateNeeded;
				item.Dispose ();
			}
			
			statistical_items = new_items;
			UpdateWindowItems ();
			if (DockItemsChanged != null)
				DockItemsChanged (DockItems);
			
		}

		void HandleUpdateNeeded(object sender, UpdateRequestArgs args)
		{
			if (ItemNeedsUpdate != null)
				ItemNeedsUpdate (this, args);
		}

		void HandleRemoveClicked(object sender, EventArgs e)
		{
			for (int i=0; i<DockItems.Count; i++) {
				if (DockItems [i] == sender) {
					RemoveItem (i);
					break;
				}
			}
		}
		
		void UpdateWindowItems ()
		{
			foreach (DockItem di in statistical_items.Concat (custom_items.Values)) {
				di.UpdateApplication ();
			}
			
			if (Wnck.Screen.Default.ActiveWorkspace == null)
				return;
			List<ApplicationDockItem> out_items = new List<ApplicationDockItem> ();
			
			foreach (Wnck.Application app in WindowUtils.GetApplications ()) {
				bool good = false;
				//we dont want any applications that dont have any "real" windows
				foreach (Wnck.Window w in app.Windows) {
					if (!w.IsSkipTasklist)
						good = true;
				}
				
				// Anything we already have, we dont need additional copies of
				foreach (DockItem di in statistical_items.Concat (custom_items.Values)) {
					if (di.Apps.Count () == 0)
						continue;
					if (di.Pids.Contains (app.Pid)) {
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
				out_items.Add (api);
			}
			
			foreach (IDockItem item in task_items)
				item.Dispose ();
					
			task_items = out_items;
		}
	}
}
