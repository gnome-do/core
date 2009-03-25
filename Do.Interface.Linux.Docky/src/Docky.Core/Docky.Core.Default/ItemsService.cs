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
using Do.Interface.Wink;
using Do.Universe;
using Do.Platform;

using Docky.Interface;
using Docky.Utilities;

using Wnck;

namespace Docky.Core.Default
{
	public class ItemsService : IItemsService
	{
		// stores items tied with their original identifier so we can remove them later
		Dictionary<string, AbstractDockItem> custom_items;
		
		// a collection for each of the major types of item
		List<AbstractDockItem> output_items, task_items, stat_items;
		
		// this will be a readonly collection to track out output items
		ReadOnlyCollection<AbstractDockItem> readonly_output_items;
		
		bool CustomItemsRead { get; set; }
		
		//// <value>
		/// Our main menu.  We only ever need one so we will store it here
		/// </value>
		AbstractDockItem MenuItem { get; set; }
		
		/// <value>
		/// Our separator.  we can re-use this to render over and over if need be.
		/// </value>
		AbstractDockItem Separator { get; set; }
		
		/// <value>
		/// The path to the file we will use to serialize out our custom items
		/// </value>
		string CustomItemsPath {
			get {
				return Path.Combine (Services.Paths.UserDataDirectory, GetType ().Name + "_CustomItems");
			}
		}
		
		/// <value>
		/// The path to the file we will use to store our sort dictionary
		/// </value>
		string SortDictionaryPath {
			get { 
				return Path.Combine (Services.Paths.UserDataDirectory, GetType ().Name + "_SortDictionary");
			}
		}
		
		IEnumerable<AbstractDockItem> OrderedItems {
			get {
				return stat_items
					    .Concat (custom_items.Values)
						.Concat (task_items)
						.OrderBy (di => di.Position);
			}
		}
		
		int LastPosition {
			get {
				if (!OrderedItems.Any ())
					return 0;
				return OrderedItems.Max ((Func<AbstractDockItem, int>) (di => di.Position));
			}
		}
		
		#region Constructor
		
		public ItemsService ()
		{
			// build our data structures
			custom_items = new Dictionary<string, AbstractDockItem> ();
			task_items = new List<AbstractDockItem> ();
			output_items = new List<AbstractDockItem> ();
			stat_items = new List<AbstractDockItem> ();
			
			// hook up our read only collection
			readonly_output_items = output_items.AsReadOnly ();
			
			Separator = new SeparatorItem ();
			MenuItem = new DoDockItem ();
			
			RegisterEvents ();
		}
		
		#endregion
		
		#region Event Handling
		void RegisterEvents ()
		{
			// core services
			Services.Core.UniverseInitialized += HandleUniverseInitialized;
			
			// wnck events
			Wnck.Screen.Default.WindowClosed += HandleWindowClosed; 
			Wnck.Screen.Default.WindowOpened += HandleWindowOpened; 
			
			// Dock Services
			DockPreferences.AutomaticIconsChanged += HandleAutomaticIconsChanged; 
			DockServices.DockletService.AppletVisibilityChanged += HandleAppletVisibilityChanged; 
			
			RegisterDocklets ();
		}
		
		void RegisterDocklets ()
		{
			RegisterDockItem (MenuItem);
			
			foreach (AbstractDockItem item in DockServices.DockletService.Docklets) {
				RegisterDockItem (item);
			}
		}
		
		void UnregisterEvents ()
		{
			// core services
			Services.Core.UniverseInitialized -= HandleUniverseInitialized;
			
			// wnck events
			Wnck.Screen.Default.WindowClosed -= HandleWindowClosed; 
			Wnck.Screen.Default.WindowOpened -= HandleWindowOpened; 
			
			// Dock Services
			DockPreferences.AutomaticIconsChanged -= HandleAutomaticIconsChanged; 
			DockServices.DockletService.AppletVisibilityChanged -= HandleAppletVisibilityChanged; 
			
			UnregisterDocklets ();
		}
		
		void UnregisterDocklets ()
		{
			UnregisterDockItem (MenuItem);
			
			foreach (AbstractDockItem item in DockServices.DockletService.Docklets) {
				UnregisterDockItem (item);
			}
		}
		
		void RegisterDockItem (AbstractDockItem dockItem)
		{
			dockItem.UpdateNeeded += HandleUpdateNeeded;
			if (dockItem is IRightClickable)
				(dockItem as IRightClickable).RemoveClicked += HandleRemoveClicked;
		}
		
		void UnregisterDockItem (AbstractDockItem dockItem)
		{
			dockItem.UpdateNeeded -= HandleUpdateNeeded;
			if (dockItem is IRightClickable)
				(dockItem as IRightClickable).RemoveClicked -= HandleRemoveClicked;
		}

		void HandleAppletVisibilityChanged(object sender, EventArgs e)
		{
			OnDockItemsChanged ();
		}

		void HandleAutomaticIconsChanged()
		{
			UpdateItems ();
		}
		
		void HandleRemoveClicked(object sender, EventArgs e)
		{
			if (sender is AbstractDockItem)
				RemoveItem (sender as AbstractDockItem);
		}

		void HandleWindowOpened(object o, WindowOpenedArgs args)
		{
			// we do a delayed update so that we allow a small gap for wnck to catch up
			if (!args.Window.IsSkipTasklist)
				DelayUpdateItems ();
		}

		void HandleWindowClosed(object o, WindowClosedArgs args)
		{
			if (!args.Window.IsSkipTasklist)
				DelayUpdateItems ();
		}

		void HandleUniverseInitialized(object sender, EventArgs e)
		{
			UpdatesEnabled = true;
			UpdateItems ();
		}
		
		void HandleUpdateNeeded(object sender, UpdateRequestArgs args)
		{
			if (!DockItems.Contains (args.Item))
				return;
			if (ItemNeedsUpdate != null)
				ItemNeedsUpdate (this, args);
		}
		
		#endregion
		
		#region Item Updating
		AbstractDockItem MaybeCreateCustomItem (string identifier)
		{
			ItemDockItem customItem = null;
			
			if (identifier.StartsWith ("file://"))
				identifier = identifier.Substring ("file://".Length);
			
			if (File.Exists (identifier) || Directory.Exists (identifier)) {
				if (identifier.EndsWith (".desktop")) {
					Item o = Services.UniverseFactory.NewApplicationItem (identifier) as Item;
					customItem = new ItemDockItem (o);
				} else {
					Item o = Services.UniverseFactory.NewFileItem (identifier) as Item;
					customItem = new ItemDockItem (o);
				}
			} else {
				Item e = Services.Core.GetElement (identifier) as Item;
				if (e != null)
					customItem = new ItemDockItem (e);
				else
					Log<ItemsService>.Error ("Could not add custom item with id: {0}", identifier);
			}
			return customItem;
		}
		
		void SimplifyPositions (IEnumerable<AbstractDockItem> items)
		{
			int i = 0;
			// we call ToArray so our enumerator does get screwed up when we change the Position
			foreach (AbstractDockItem item in items.OrderBy (di => di.Position).ToArray ())
				item.Position = i++;
		}
		
		void DelayUpdateItems ()
		{
			GLib.Timeout.Add (20, delegate {
				UpdateItems ();
				return false;
			});
		}
		
		void UpdateItems ()
		{
			if (!UpdatesEnabled)
				return;
			
			UpdateStatItems ();
			
			if (!CustomItemsRead) {
				foreach (string s in ReadCustomItems ())
					InternalAddItemToDock (s, LastPosition + 1);
				
				Dictionary<string, int> sortDictionary = ReadSortDictionary ();
				foreach (ItemDockItem item in OrderedItems.Where (di => di is ItemDockItem)) {
					if (sortDictionary.ContainsKey (item.Element.UniqueId))
						item.Position = sortDictionary [item.Element.UniqueId];
				}
				
				CustomItemsRead = true;
			}
			
			UpdateTaskItems ();
			SimplifyPositions (OrderedItems);
			
			OnDockItemsChanged ();
		}
		
		void UpdateStatItems ()
		{
			List<ItemDockItem> old_items = new List<ItemDockItem> (stat_items.Where (di => di is ItemDockItem)
			                                                       .Cast<ItemDockItem> ());
			List<ItemDockItem> local_cust = new List<ItemDockItem> (custom_items.Values
			                                                        .Where (di => di is ItemDockItem)
			                                                        .Cast<ItemDockItem> ());
			
			stat_items = new List<AbstractDockItem> ();
			
			IEnumerable<Item> mostUsedItems = MostUsedItems ();
			
			DateTime currentTime = DateTime.UtcNow;
			foreach (Item item in mostUsedItems) {
				if (local_cust.Any (di => di.Element == item))
					continue;
				
				if (old_items.Any (di => di.Element == item)) {
					stat_items.AddRange (old_items.Where (di => di.Element == item).Cast<AbstractDockItem> ());
				} else {
					ItemDockItem di = new ItemDockItem (item);
					RegisterDockItem (di);
					di.DockAddItem = currentTime;
					
					int position = LastPosition + 1;

					//TODO fixme once mono 1.9 support is dropped
					if (old_items.Any ())
						position += old_items.Max ((Func<ItemDockItem, int>) (oi => oi.Position));

					di.Position = position;
					stat_items.Add (di);
				}
			}
			
			// potential leak if not all items in stat_items were ItemDockItems!!!
			foreach (ItemDockItem item in  old_items.Where (di => !stat_items.Contains (di))) {
				UnregisterDockItem (item);
				item.Dispose ();
			}
		}
		
		void UpdateTaskItems ()
		{
			foreach (ItemDockItem item in OrderedItems.Where (di => di is ItemDockItem))
				item.UpdateApplication ();
			
			List<ApplicationDockItem> out_items = new List<ApplicationDockItem> ();

			IEnumerable<Window> knownWindows = OrderedItems
				    .Where (di => di is ItemDockItem)
					.Cast<ItemDockItem> ()
					.SelectMany (di => di.Windows);
			
			var prunedWindows = WindowUtils.GetWindows ()
				    .Where (w => !w.IsSkipTasklist && !knownWindows.Contains (w))
					.GroupBy (w => SafeResClass (w));
			
			foreach (IEnumerable<Wnck.Window> windows in prunedWindows) {
				ApplicationDockItem api = new ApplicationDockItem (windows);

				if (task_items.Any (di => di.Equals (api))) {
					AbstractDockItem match = task_items.Where (di => di.Equals (api)).First ();
					api.DockAddItem = match.DockAddItem;
					api.Position = match.Position;
				} else {
					api.DockAddItem = DateTime.UtcNow;
				}
					
				out_items.Add (api);
				RegisterDockItem (api);
			}
			
			foreach (AbstractDockItem item in task_items) {
				UnregisterDockItem (item);
				item.Dispose ();
			}
			
			foreach (ApplicationDockItem api in out_items.OrderBy (di => di.DockAddItem)) {
				if (api.Position == 0)
					api.Position = LastPosition + 1;
			}
					
			task_items.Clear ();
			task_items.AddRange (out_items.Cast<AbstractDockItem> ());
		}
		
		string SafeResClass (Wnck.Window window)
		{
			if (window.ClassGroup != null && window.ClassGroup.ResClass != null)
				return window.ClassGroup.ResClass;
			return string.Empty;
		}
		
		void OnDockItemsChanged ()
		{
			output_items.Clear ();
			
			if (DockItemsChanged != null)
				DockItemsChanged (DockItems);
		}
		#endregion
		
		#region Item Management
		bool InternalAddItemToDock (Element item, int position)
		{
			if (!(item is Item)) {
				Log<ItemsService>.Error ("Could not add {0} to custom items for dock", item.Safe.Name);
				return false;
			}
			
			string id = item.UniqueId;
			if (custom_items.ContainsKey (id))
				return false;
			
			AbstractDockItem dockItem = new ItemDockItem (item as Item);
			RegisterDockItem (dockItem);
			
			MakeHoleAtPosition (position);
			
			dockItem.Position = position;
			custom_items [id] = dockItem;
			
			return true;
		}
		
		bool InternalAddItemToDock (string identifier, int position)
		{
			if (custom_items.ContainsKey (identifier)) return false;
			
			AbstractDockItem customItem = MaybeCreateCustomItem (identifier);
			
			if (customItem == null) return false;
			
			RegisterDockItem (customItem);
			
			MakeHoleAtPosition (position);
			
			customItem.Position = position;
			custom_items [identifier] = customItem;
			
			return true;
		}
		#endregion
		
		#region Disk Utilities
		void WriteData ()
		{
			WriteSortDictionary ();
			WriteCustomItems ();
		}
		
		void WriteSortDictionary ()
		{
			try {
				if (File.Exists (SortDictionaryPath))
					File.Delete (SortDictionaryPath);
				
				using (StreamWriter writer = new StreamWriter (SortDictionaryPath)) {
					foreach (ItemDockItem di in OrderedItems.Where (di => di is ItemDockItem)) {
						writer.WriteLine ("{0}|{1}", di.Element.UniqueId, di.Position);
					}
				}
			} catch (Exception e) {
				Log<ItemsService>.Error ("Could not write out sort items");
				Log<ItemsService>.Error (e.Message);
			}
		}
		
		Dictionary<string, int> ReadSortDictionary ()
		{
			Dictionary<string, int> sortDictionary = new Dictionary<string, int> ();
			try {
				using (StreamReader reader = new StreamReader (SortDictionaryPath)) {
					string [] line;
					while (!reader.EndOfStream) {
						line = reader.ReadLine ().Split ('|');
						sortDictionary [line [0]] = Convert.ToInt32 (line [1]);
					}
				}
			} catch (FileNotFoundException e) {
				Log<ItemsService>.Debug ("Sort Dictionary file not present, nothing to add. " + e.Message);
			} catch (Exception e) {
				Log<ItemsService>.Error ("Could not deserialize sort dictionary");
			}
			return sortDictionary;
		}
		
		void WriteCustomItems ()
		{
			try {
				using (Stream s = File.OpenWrite (CustomItemsPath)) {
					BinaryFormatter f = new BinaryFormatter ();
					f.Serialize (s, custom_items.Keys.ToArray ());
				}
			} catch (Exception e) {
				Log<ItemsService>.Error ("Could not serialize custom items");
				Log<ItemsService>.Error (e.Message);
			}
		}
		
		IEnumerable<string> ReadCustomItems ()
		{
			string[] filenames;
			try {
				using (Stream s = File.OpenRead (CustomItemsPath)) {
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
		#endregion
		
		#region Random Useful Functions
		bool ItemCanInteractWithPosition (AbstractDockItem item, int position)
		{
			return DockItems.Contains (item) && 0 <= position && position < DockItems.Count;
		}
		
		int LastNonTaskItemPosition ()
		{
			if (!OrderedItems.Any (di => !(di is ApplicationDockItem)))
				return 0;
			return OrderedItems
				.Where (di => !(di is ApplicationDockItem))
				.Max ((Func<AbstractDockItem, int>) (di => di.Position));
		}
		
		/// <summary>
		/// Returns the most used items out of GNOME Do and does a tiny bit of filtering and sorting on them
		/// This is mostly to encourage a better first run experience, but overall this can be improved
		/// </summary>
		/// <returns>
		/// A <see cref="IEnumerable"/> of the most used items from Do's core universe
		/// </returns>
		IEnumerable<Item> MostUsedItems ()
		{
			return Services.Core
				.GetItemsOrderedByRelevance ()
				.Where (item => item.GetType ().Name != "SelectedTextItem" && 
					        item.GetType ().Name != "GNOMETrashFileItem")
				.Where (item => !DockPreferences.ItemBlacklist.Contains (item.UniqueId))
				.Take (DockPreferences.AutomaticIcons)
				.OrderByDescending (item => item is IApplicationItem)
				.ThenBy (item => item.GetType ().Name)
				.ThenBy (item => item.Safe.Name);
		}
		
		void MakeHoleAtPosition (int position)
		{
			foreach (AbstractDockItem di in OrderedItems) {
				if (di.Position >= position)
					di.Position++;
			}
		}
		#endregion
		
		#region IItemsService implementation
		public event DockItemsChangedHandler DockItemsChanged;
		public event UpdateRequestHandler ItemNeedsUpdate;
		
		public bool UpdatesEnabled { get; private set; }
		
		public ReadOnlyCollection<AbstractDockItem> DockItems {
			get {
				if (output_items.Count == 0) {
					output_items.Add (MenuItem);
					
					// Add our custom/task/statistical items in one shot
					output_items.AddRange (OrderedItems);
				
					// add a separator and any docklets that are active
					if (DockServices.DockletService.ActiveDocklets.Any ()) {
						output_items.Add (Separator);
						output_items.AddRange (DockServices.DockletService.ActiveDocklets.Cast<AbstractDockItem> ());
					}
				}
				return readonly_output_items;
			}
		}
		
		public void AddItemToDock (Element item)
		{
			AddItemToDock (item, LastPosition + 1);
		}
		
		public void AddItemToDock (string identifier)
		{
			AddItemToDock (identifier, LastPosition + 1);
		}
		
		public void AddItemToDock (Element item, int position)
		{
			position = DockItems [position].Position;
			if (InternalAddItemToDock (item, position)) {
				UpdateItems ();
				WriteData ();
				OnDockItemsChanged ();
			}
		}
		
		public void AddItemToDock (string identifier, int position)
		{
			position = DockItems [position].Position;
			if (InternalAddItemToDock (identifier, position)) {
				UpdateItems ();
				WriteData ();
				OnDockItemsChanged ();
			}
		}
		
		public bool ItemCanBeMoved (int item)
		{
			if (item < 0 || item > DockItems.Count)
				return false;
			
			return (OrderedItems.Contains (DockItems [item]));
		}
		
		public void DropItemOnPosition (AbstractDockItem item, int position)
		{
			do {
				if (!ItemCanInteractWithPosition (item, position)) continue;
			
				if (DockItems [position] is TrashDockItem) {
					RemoveItem (item);
					continue;
				}
				
				if (item is ApplicationDockItem && position <= LastNonTaskItemPosition ()) {
					ApplicationDockItem api = item as ApplicationDockItem;
					
					if (api.Launcher == null) continue;
					
					Item launcher = api.Launcher as Item;
					if (launcher == null)
						continue;
					
					AbstractDockItem newItem = new ItemDockItem (launcher);
					
					newItem.Position = item.Position;
					newItem.DockAddItem = item.DockAddItem;
					custom_items [launcher.UniqueId] = newItem;
					
					RegisterDockItem (newItem);
					UpdateItems ();
					WriteData ();
				}
			} while (false);
		}
		
		public void MoveItemToPosition (AbstractDockItem item, int position)
		{
			if (ItemCanInteractWithPosition (item, position))
				MoveItemToPosition (DockItems.IndexOf (item), position);
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
			
			if (itemSource == IconSource.Unknown || targetSource == IconSource.Unknown)
				return;
			
			AbstractDockItem primaryItem = DockItems [item];
			AbstractDockItem targetItem = DockItems [position];
			
			int startPosition = primaryItem.Position;
			int targetPosition = targetItem.Position;
			
			foreach (AbstractDockItem di in OrderedItems) {
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
			WriteData ();
		}
		
		public void ForceUpdate ()
		{
			UpdateItems ();
		}
		
		public IconSource GetIconSource (AbstractDockItem item)
		{
			if (task_items.Contains (item))
				return IconSource.Application;
			
			if (stat_items.Contains (item))
				return IconSource.Statistics;
			
			if (custom_items.Values.Contains (item))
				return IconSource.Custom;
			
			return IconSource.Unknown;
		}
		
		public bool RemoveItem (AbstractDockItem item)
		{
			if (!DockItems.Contains (item))
				return false;
			return RemoveItem (DockItems.IndexOf (item));
		}
		
		public bool RemoveItem (int item)
		{
			bool ret_val = false;
			
			if (DockItems [item].WindowCount == 0) {
				if (GetIconSource (DockItems [item]) == IconSource.Statistics && DockItems [item] is ItemDockItem) {
					DockPreferences.AddBlacklistItem ((DockItems [item] as ItemDockItem).Element.UniqueId);
					DockPreferences.AutomaticIcons = Math.Max (0, DockPreferences.AutomaticIcons - 1);
					UpdateItems ();
					ret_val = true;
				} else if (GetIconSource (DockItems [item]) == IconSource.Custom) {
					foreach (KeyValuePair<string, AbstractDockItem> kvp in custom_items) {
						if (kvp.Value.Equals (DockItems [item])) {
							custom_items.Remove (kvp.Key);
							
							UpdateItems ();
							ret_val = true;
							break;
						}
					}
				}
			}
			
			UpdateItems ();
			WriteData ();
			return ret_val;
		}
		
		public bool HotSeatItem (AbstractDockItem item, List<AbstractDockItem> seatedItems)
		{
			Log<ItemsService>.Error ("Items Service cannot currently handle hotseating");
			return false;
		}
		
		public bool ResetHotSeat (AbstractDockItem item)
		{
			Log<ItemsService>.Error ("Items Service cannot currently handle hotseating");
			return false;
		}
		#endregion

		#region IDisposable implementation
		public void Dispose ()
		{
			UnregisterEvents ();
			
			foreach (AbstractDockItem di in DockItems) {
				if (di.Disposed)
					continue;
				
				UnregisterDockItem (di);
				di.Dispose ();
			}
			
			if (!Separator.Disposed)
				Separator.Dispose ();
			
			custom_items.Clear ();
			stat_items.Clear ();
			output_items.Clear ();
			task_items.Clear ();
		}
		#endregion
		
		
	}
}
