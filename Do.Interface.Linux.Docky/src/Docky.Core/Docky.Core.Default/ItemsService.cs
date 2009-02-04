// ItemsService.cs
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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using Do;
using Do.Interface;
using Do.Universe;
using Do.Platform;

using Docky.Interface;
using Docky.Utilities;

using Wnck;

namespace Docky.Core.Default
{
	public class ItemsService : IItemsService
	{
		
		public event DockItemsChangedHandler DockItemsChanged;
		public event UpdateRequestHandler ItemNeedsUpdate;
		
		IDictionary<string, DockItem> custom_items;
		List<DockItem> statistical_items;
		List<BaseDockItem> output_items;
		ReadOnlyCollection<BaseDockItem> readonly_items; 
		List<ApplicationDockItem> task_items;
		bool enable_serialization = true;
		bool custom_items_read;
		
		string DesktopFilesPath {
			get {
				return Path.Combine (Services.Paths.UserDataDirectory, GetType ().Name + "DesktopFiles");
			}
		}
		
		string SortDictionaryPath {
			get { 
				return Path.Combine (Services.Paths.UserDataDirectory, GetType ().Name + "SortDictionary");
			}
		}
		
		int LastPosition {
			get {
				if (!DraggableItems.Any ())
					return 0;
				// TODO make sane once mono 1.9 support is dropped
				return DraggableItems.Max ((Func<DockItem, int>) (di => di.Position));
			}
		}
		
		BaseDockItem Separator { get; set; }
		BaseDockItem MenuItem { get; set; }
		BaseDockItem TrashItem { get; set; }
		BaseDockItem ClockItem { get; set; }
		
		IEnumerable<DockItem> DraggableItems {
			get { return statistical_items.Concat (custom_items.Values).OrderBy (di => di.Position); }
		}
		
		public bool UpdatesEnabled { get; set; }
		
		public ReadOnlyCollection<BaseDockItem> DockItems {
			get {
				if (output_items.Count == 0) {
					output_items.Add (MenuItem);
					output_items.AddRange (DraggableItems.Cast<BaseDockItem> ());
				
					if (task_items.Any ()) {
						output_items.AddRange (task_items.Cast<BaseDockItem> ().OrderBy (bdi => bdi.DockAddItem));
					}
				
					if (DockPreferences.ShowTrash || DockPreferences.ShowClock) {
						output_items.Add (Separator);
						
						if (DockPreferences.ShowClock)
							output_items.Add (ClockItem);
						
						if (DockPreferences.ShowTrash)
							output_items.Add (TrashItem);
					}
					
					
					
				}
				return readonly_items;
			}
		}
		
		public ItemsService ()
		{
			Separator = new SeparatorItem ();
			MenuItem = new DoDockItem ();
			TrashItem = new TrashDockItem ();
			ClockItem = new ClockDockItem ();
			
			custom_items = new Dictionary<string, DockItem> ();
			statistical_items = new List<DockItem> ();
			task_items = new List<ApplicationDockItem> ();
			
			output_items = new List<BaseDockItem> ();
			readonly_items = output_items.AsReadOnly ();
			
			RegisterEvents ();
		}
		
		void RegisterEvents ()
		{
			Services.Core.UniverseInitialized += OnUniverseInitialized;
			Wnck.Screen.Default.WindowClosed += OnWindowClosed;
			Wnck.Screen.Default.WindowOpened += OnWindowOpened;
			DockPreferences.TrashVisibilityChanged += OnDockItemsChanged;
			DockPreferences.AutomaticIconsChanged += UpdateItems;
		}
		
		void UnregisterEvents ()
		{
			Services.Core.UniverseInitialized -= OnUniverseInitialized;
			Wnck.Screen.Default.WindowClosed -= OnWindowClosed;
			Wnck.Screen.Default.WindowOpened -= OnWindowOpened;
			DockPreferences.TrashVisibilityChanged -= OnDockItemsChanged;
			DockPreferences.AutomaticIconsChanged -= UpdateItems;
		}
		
		private void OnWindowClosed (object o, WindowClosedArgs args) 
		{
			if (args.Window.IsSkipTasklist)
					return;
			UpdateItems ();
		}
		
		private void OnWindowOpened (object o, WindowOpenedArgs args) 
		{
			if (args.Window.IsSkipTasklist)
					return;
			UpdateItems ();
		}
		
		public void AddItemToDock (Element item)
		{
			if (!(item is Item)) {
				Log<ItemsService>.Error ("Could not add {0} to custom items for dock", item.Safe.Name);
				return;
			}
			string id = item.UniqueId;
			if (custom_items.ContainsKey (id))
				return;
			
			DockItem di = new DockItem (item as Item);
			di.RemoveClicked += HandleRemoveClicked;
			di.UpdateNeeded += HandleUpdateNeeded;
			di.Position = LastPosition + 1;
			custom_items [id] = di;
			
			UpdateStatisticalItems ();
			OnDockItemsChanged ();
			
			if (enable_serialization)
				SerializeData ();
		}
		
		public void AddItemToDock (string identifier)
		{
			if (custom_items.ContainsKey (identifier))
				return;
			
			DockItem customItem = GetCustomItem (identifier);
			
			if (customItem != null) {
				customItem.RemoveClicked += HandleRemoveClicked;
				customItem.UpdateNeeded += HandleUpdateNeeded;
				customItem.Position = LastPosition + 1;
				custom_items [identifier] = customItem;
			}
			
			UpdateStatisticalItems ();
			OnDockItemsChanged ();
			
			if (enable_serialization)
				SerializeData ();
		}
		
		DockItem GetCustomItem (string identifier)
		{
			DockItem customItem = null;
			
			if (identifier.StartsWith ("file://"))
				identifier = identifier.Substring ("file://".Length);
			
			if (File.Exists (identifier) || Directory.Exists (identifier)) {
				if (identifier.EndsWith (".desktop")) {
					Item o = Services.UniverseFactory.NewApplicationItem (identifier) as Item;
					customItem = new DockItem (o);
				} else {
					Item o = Services.UniverseFactory.NewFileItem (identifier) as Item;
					customItem = new DockItem (o);
				}
			} else {
				Item e = Services.Core.GetElement (identifier) as Item;
				if (e != null)
					customItem = new DockItem (e);
				else
					Log<ItemsService>.Error ("Could not add custom item with id: {0}", identifier);
			}
			return customItem;
		}
		
		public bool ItemCanBeMoved (int item)
		{
			return DockItems [item] is DockItem && DraggableItems.Contains (DockItems [item] as DockItem);
		}
		
		public void DropItemOnPosition (BaseDockItem item, int position)
		{
			if (ItemCanInteractWithPosition (item, position))
				if (DockItems [position] is TrashDockItem)
					RemoveItem (item);
		}

		public void MoveItemToPosition (BaseDockItem item, int position)
		{
			if (ItemCanInteractWithPosition (item, position))
				MoveItemToPosition (DockItems.IndexOf (item), position);
		}
		
		bool ItemCanInteractWithPosition (BaseDockItem item, int position)
		{
			return DockItems.Contains (item) && 0 <= position && position < DockItems.Count;
		}
		
		public void MoveItemToPosition (int item, int position)
		{
			if (item == position || 
			    item < 0 || 
			    position < 0 || 
			    position > DockItems.Count || 
			    item > DockItems.Count)
				return;
			
			IconSource itemSource = GetIconSource (DockItems [item]);
			IconSource targetSource = GetIconSource (DockItems [position]);
			
			if (itemSource == IconSource.Application || itemSource == IconSource.Unknown ||
			    targetSource == IconSource.Application || targetSource == IconSource.Unknown)
				return;
			
			DockItem primaryItem = DockItems [item] as DockItem;
			DockItem targetItem = DockItems [position] as DockItem;
			
			int startPosition = primaryItem.Position;
			int targetPosition = targetItem.Position;
			
			foreach (DockItem di in DraggableItems) {
				if (startPosition < targetPosition) {
					// the item is being shifted to the right.  Everything greater than item up to and including target item
					// needs to be shifted to the left
					if (di.Position > startPosition && di.Position <= targetPosition)
						di.Position--;
				} else {
					// the item is being shifted to the left.  Everthing less than the item and up to and including target item
					// needs to be shifted to the right
					if (di.Position < startPosition && di.Position >= targetPosition)
						di.Position++;
				}
			}
			
			primaryItem.Position = targetPosition;
			
			OnDockItemsChanged ();
			SerializeData ();
		}
		
		public void ForceUpdate ()
		{
			UpdateItems ();
		}
		
		public IconSource GetIconSource (BaseDockItem item) {
			if (item is ApplicationDockItem && task_items.Contains (item as ApplicationDockItem))
				return IconSource.Application;
			
			if (item is DockItem && statistical_items.Contains (item as DockItem))
				return IconSource.Statistics;
			
			if (item is DockItem && custom_items.Values.Contains (item as DockItem))
				return IconSource.Custom;
			
			return IconSource.Unknown;
		}
		
		void HandleUpdateNeeded(object sender, UpdateRequestArgs args)
		{
			if (ItemNeedsUpdate != null)
				ItemNeedsUpdate (this, args);
		}

		void HandleRemoveClicked(object sender, EventArgs e)
		{
			if (sender is BaseDockItem)
				RemoveItem (sender as BaseDockItem);
		}
		
		IEnumerable<Item> MostUsedItems ()
		{
			return Services.Core
				.GetItemsOrderedByRelevance ()
				.Where (item => item.GetType ().Name != "SelectedTextItem" && item.GetType ().Name != "GNOMETrashFileItem")
				.Where (item => !DockPreferences.ItemBlacklist.Contains (item.UniqueId))
				.Take (DockPreferences.AutomaticIcons)
				.OrderByDescending (item => item is IApplicationItem)
				.ThenBy (item => item.GetType ().Name)
				.ThenBy (item => item.Safe.Name);
		}

		public bool RemoveItem (BaseDockItem item)
		{
			if (!DockItems.Contains (item))
				return false;
			return RemoveItem (DockItems.IndexOf (item));
		}
		
		public bool RemoveItem (int item)
		{
			bool ret_val = false;
			
			if (GetIconSource (DockItems [item]) == IconSource.Statistics) {
				DockPreferences.AddBlacklistItem ((DockItems [item] as DockItem).Element.UniqueId);
				DockPreferences.AutomaticIcons = Math.Max (0, DockPreferences.AutomaticIcons - 1);
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
				SerializeData ();
			return ret_val;
		}
		
		void SerializeData ()
		{
			SerializeCustomItems ();
			SerializeSortDictionary ();
		}
		
		void SerializeCustomItems ()
		{
			try {
				using (Stream s = File.OpenWrite (DesktopFilesPath)) {
					BinaryFormatter f = new BinaryFormatter ();
					f.Serialize (s, custom_items.Keys.ToArray ());
				}
			} catch (Exception e) {
				Log<ItemsService>.Error ("Could not serialize custom items");
				Log<ItemsService>.Error (e.Message);
			}
		}
		
		void SerializeSortDictionary ()
		{
			try {
				using (Stream s = File.OpenWrite (SortDictionaryPath)) {
					BinaryFormatter f = new BinaryFormatter ();
					f.Serialize (s, DraggableItems.ToDictionary (di => di.Element.UniqueId, di => di.Position));
				}
			} catch (Exception e) {
				Log<ItemsService>.Error ("Could not serialize sort items");
				Log<ItemsService>.Error (e.Message);
			}
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
				Log<ItemsService>.Debug ("Custom items file not present, nothing to add. " + e.Message);
				filenames = new string[0];
			} catch {
				Log<ItemsService>.Error ("Could not deserialize custom items");
				filenames = new string[0];
			}
			return filenames;
		}
		
		Dictionary<string, int> DeserializeSortDictionary ()
		{
			Dictionary<string, int> sortDictionary;
			try {
				using (Stream s = File.OpenRead (SortDictionaryPath)) {
					BinaryFormatter f = new BinaryFormatter ();
					sortDictionary = f.Deserialize (s) as Dictionary<string, int>;
				}
			} catch (FileNotFoundException e) {
				Log<ItemsService>.Debug ("Sort Dictionary file not present, nothing to add. " + e.Message);
				sortDictionary = new Dictionary<string, int> ();
			} catch {
				Log<ItemsService>.Error ("Could not deserialize sort dictionary");
				sortDictionary = new Dictionary<string, int> ();
			}
			return sortDictionary;
		}
		
		void UpdateItems ()
		{
			if (!UpdatesEnabled)
				return;
			
			UpdateStatisticalItems ();
			
			if (!custom_items_read) {
				enable_serialization = false;
				foreach (string s in DeserializeCustomItems ())
					AddItemToDock (s);
				enable_serialization = true;
				
				custom_items_read = true;
				
				Dictionary<string, int> sortDictionary = DeserializeSortDictionary ();
				foreach (DockItem item in DraggableItems) {
					if (sortDictionary.ContainsKey (item.Element.UniqueId))
						item.Position = sortDictionary [item.Element.UniqueId];
				}
			}
			
			UpdateWindowItems ();
			SimplifyPositions (DraggableItems);
			OnDockItemsChanged ();
			
		}
		
		void UpdateStatisticalItems ()
		{
			List<DockItem> old_items = statistical_items;
			statistical_items = new List<DockItem> ();
			
			IEnumerable<Item> mostUsedItems = MostUsedItems ();
			
			foreach (Item item in mostUsedItems) {
				if (custom_items.Values.Any (di => di.Element == item))
					continue;
				
				if (old_items.Any (di => di.Element == item)) {
					statistical_items.AddRange (old_items.Where (di => di.Element == item));
				} else {
					DockItem di = new DockItem (item);
					di.RemoveClicked += HandleRemoveClicked;
					di.UpdateNeeded += HandleUpdateNeeded;
					di.DockAddItem = DateTime.UtcNow;
					
					int position = LastPosition + 1;

					//TODO fixme once mono 1.9 support is dropped
					if (old_items.Any ())
						position += old_items.Max ((Func<DockItem, int>) (oi => oi.Position));

					di.Position = position;
					statistical_items.Add (di);
				}
			}
			
			foreach (DockItem item in  old_items.Where (di => !statistical_items.Contains (di))) {
				item.RemoveClicked -= HandleRemoveClicked;
				item.UpdateNeeded -= HandleUpdateNeeded;
				item.Dispose ();
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

			IEnumerable<int> knownPids = statistical_items.Concat (custom_items.Values).SelectMany (di => di.Pids);
			
			IEnumerable<Application> prunedApps = WindowUtils.GetApplications ()
				.Where (app => app.Windows.Any (w => !w.IsSkipTasklist))
				.Where (app => !knownPids.Contains (app.Pid) && app.Windows.Any ());
			
			foreach (IEnumerable<Wnck.Application> apps in prunedApps
			         .GroupBy (app => (app as Wnck.Application).Windows [0].ClassGroup.ResClass)) {
				ApplicationDockItem api = new ApplicationDockItem (apps);

				if (task_items.Any (di => di.Equals (api)))
					api.DockAddItem = task_items.Where (di => di.Equals (api)).First ().DockAddItem;
				else
					api.DockAddItem = DateTime.UtcNow;

				out_items.Add (api);
				api.UpdateNeeded += HandleUpdateNeeded;
			}
			
			foreach (ApplicationDockItem item in task_items) {
				item.Dispose ();
				item.UpdateNeeded -= HandleUpdateNeeded;
			}
					
			task_items = out_items;
		}
		
		void SimplifyPositions (IEnumerable<DockItem> items)
		{
			int i = 0;
			// we call ToArray so our enumerator does get screwed up when we change the Position
			foreach (DockItem item in items.OrderBy (di => di.Position).ToArray ())
				item.Position = i++;
		}
		
		void OnDockItemsChanged ()
		{
			// clear the cache
			output_items.Clear ();
			
			if (DockItemsChanged != null)
				DockItemsChanged (DockItems);
		}

		void OnUniverseInitialized (object sender, EventArgs e) 
		{
			UpdatesEnabled = true;
			UpdateItems ();
		}
		
		public void Dispose ()
		{
			UnregisterEvents ();
			
			foreach (BaseDockItem di in DockItems) {
				if (di is IRightClickable)
					(di as IRightClickable).RemoveClicked -= HandleRemoveClicked;
				di.UpdateNeeded -= HandleUpdateNeeded;
				
				di.Dispose ();
			}

			if (!DockItems.Contains (TrashItem))
				TrashItem.Dispose ();
			
			if (!DockItems.Contains (Separator))
				Separator.Dispose ();

			custom_items.Clear ();
			statistical_items.Clear ();
			output_items.Clear (); 
			task_items.Clear ();
		}
	}
}
