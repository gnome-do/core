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
		
		IStatistics statistics;
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
				out_items.AddRange (statistical_items);
				
				if (custom_items.Any ()) {
					out_items.Add (Separator);
					out_items.AddRange (custom_items.Values);
				}
				if (task_items.Any ()) {
					out_items.Add (Separator);
					out_items.AddRange (task_items);
				}
				return out_items;
			}
		}
		
		SeparatorItem Separator { get; set; }
		
		public DockItemProvider(IStatistics statistics)
		{
			this.statistics = statistics;
			Separator = new SeparatorItem ();
			
			custom_items = new Dictionary<string, IDockItem> ();
			statistical_items = new List<IDockItem> ();
			task_items = new List<IDockItem> ();
			
			enable_serialization = false;
			foreach (string s in DeserializeCustomItems ()) {
				if (!File.Exists (s))
					continue;
				AddCustomItemFromFile (s);
			}
			enable_serialization = true;
			
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
				UpdateItems ();
				return false;
			});
		}
		
		public void AddCustomItemFromFile (string filename)
		{
			if (filename.StartsWith ("file://"))
				filename = filename.Substring ("file://".Length);
			
			if (!File.Exists (filename))
				return;
			
			if (filename.EndsWith (".desktop")) {
				Element o = Services.UniverseFactory.NewApplicationItem (filename) as Element;
				custom_items[filename] = new DockItem (o);
			} else {
				Element o = Services.UniverseFactory.NewFileItem (filename) as Element;
				custom_items[filename] = new DockItem (o);
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
			} catch {
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
			return statistics
				.GetMostUsedItems (DockPreferences.AutomaticIcons)
				.Where (item => item.GetType ().Name != "SelectedTextItem")
				.OrderByDescending (item => item is IApplicationItem)
				.ThenBy (item => item.GetType ().Name)
				.ThenBy (item => item.NameSafe);
		}
		
		public bool RemoveItem (int item)
		{
			
			if (enable_serialization)
				SerializeCustomItems ();
			
			if (GetIconSource (DockItems[item]) == IconSource.Statistics) {
				DockPreferences.AddBlacklistItem ((DockItems[item] as DockItem).Element.UniqueId);
				UpdateItems ();
				return true;
			} else if (GetIconSource (DockItems[item]) == IconSource.Custom) {
				foreach (KeyValuePair<string, IDockItem> kvp in custom_items) {
					if (kvp.Value.Equals (DockItems[item])) {
						custom_items.Remove (kvp.Key);
						UpdateItems ();
						return true;
					}
				}
			}
			UpdateItems ();
			return false;
		}
		
		void SerializeCustomItems ()
		{
			try {
				using (Stream s = File.OpenWrite (DesktopFilesPath)) {
					BinaryFormatter f = new BinaryFormatter ();
					f.Serialize (s, custom_items.Keys.ToArray ());
				}
			} catch {
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
