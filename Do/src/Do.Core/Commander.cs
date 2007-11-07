/* ${FileName}
 *
 * GNOME Do is the legal property of its developers. Please refer to the
 * COPYRIGHT file distributed with this
 * source distribution.
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using Do.DBusLib;
using Do.Universe;

namespace Do.Core
{
	
	public delegate void OnCommanderStateChange ();
	public delegate void VisibilityChangedHandler (bool visible);
	
	public enum CommanderState {
			Default,
			SearchingItems,
			ItemSearchComplete,
			SearchingCommands,
			CommandSearchComplete
	}
	
	public abstract class Commander : ICommander {
		
		const string kActivateShortcut = "<Super>space";
		
		protected Tomboy.GConfXKeybinder keybinder;
		
		private CommanderState state;
		
		public event OnCommanderStateChange SetSearchingItemsStateEvent;
		public event OnCommanderStateChange SetItemSearchCompleteStateEvent;
		public event OnCommanderStateChange SetSearchingCommandsStateEvent;
		public event OnCommanderStateChange SetCommandSearchCompleteStateEvent;
		public event OnCommanderStateChange SetDefaultStateEvent;
		
		public event VisibilityChangedHandler VisibilityChanged;
		
		private ItemManager itemManager;
		private CommandManager commandManager;
		private string itemSearchString, commandSearchString;
		
		private Item[] currentItems;
		private Command[] currentCommands;
		
		private int currentItemIndex;
		private int currentCommandIndex;
		
		public Commander () {
			itemManager = new ItemManager ();
			commandManager = new CommandManager ();
			
			keybinder = new Tomboy.GConfXKeybinder ();
			
			SetSearchingItemsStateEvent = SetSearchingItemsState;
			SetItemSearchCompleteStateEvent = SetItemSearchCompleteState;
			SetSearchingCommandsStateEvent = SetSearchingCommandsState;
			SetCommandSearchCompleteStateEvent = SetCommandSearchCompleteState;
			SetDefaultStateEvent = SetDefaultState;
			VisibilityChanged = OnVisibilityChanged;
			
			LoadBuiltins ();
			LoadAddins ();
			SetupKeybindings ();
			State = CommanderState.Default;
		}
		
		public CommanderState State
		{
			get { return state; }
			set {
				this.state = value;
				switch (state) {
				case CommanderState.Default:
					SetDefaultStateEvent ();
					break;
				case CommanderState.SearchingItems:
					SetSearchingItemsStateEvent ();
					break;
				case CommanderState.ItemSearchComplete:
					SetItemSearchCompleteStateEvent ();
					break;
				case CommanderState.SearchingCommands:
					SetSearchingCommandsStateEvent ();
					break;
				case CommanderState.CommandSearchComplete:
					SetCommandSearchCompleteStateEvent ();
					break;
				}
			}
		}
		
		public CommandManager CommandManager
		{
			get { return commandManager; }
		}
		
		public ItemManager ItemManager
		{
			get { return itemManager; }
		}

		public string ItemSearchString
		{
			get { return itemSearchString; }
		}
		
		public string CommandSearchString
		{
			get { return commandSearchString; }
		}
		
		protected virtual void SetDefaultState ()
		{
			currentItems = new Item [0];
			currentCommands = new Command [0];
			currentItemIndex = -1;
			currentCommandIndex = -1;
			itemSearchString = "";
		}
		
		protected virtual void SetSearchingItemsState ()
		{
		}
		
		protected virtual void SetItemSearchCompleteState ()
		{
		}
		
		protected virtual void SetSearchingCommandsState ()
		{
		}
		
		protected virtual void SetCommandSearchCompleteState ()
		{
		}
		
		public Item [] CurrentItems
		{
			get { return currentItems; }
		}
		
		public Item CurrentItem
		{
			get {
				if (currentItemIndex >= 0)
					return currentItems [currentItemIndex];
				else
					return null;
			}
		}
		
		public Command [] CurrentCommands
		{
			get { return currentCommands; }
		}
		
		public Command CurrentCommand
		{
			get {
				if (this.currentCommandIndex >= 0) {
					return currentCommands [currentCommandIndex];
				} else {
					return null;
				}
			}
		}
		
		public int CurrentItemIndex
		{
			get { return currentItemIndex; }
			set {
				if (value < 0 || value >= currentItems.Length) {
					throw new IndexOutOfRangeException ();
				}
				currentItemIndex = value;
				currentCommands = commandManager.CommandsForItem (CurrentItem, "");
				if (currentCommands.Length == 0) {
					currentCommands = new Command[] { 
						new Command (new VoidCommand ()),
					};
				}
				currentCommandIndex = 0;
			}
		}
		
		public int CurrentCommandIndex
		{
			get { return currentCommandIndex; }
			set {
				if (value < 0 || value >= currentCommands.Length) {
					throw new IndexOutOfRangeException ();
				}
				currentCommandIndex = value;
			}
		}
		
		protected void LoadBuiltins ()
		{
			LoadAssembly (typeof (IItem).Assembly);
		}
		
		protected virtual void SetupKeybindings ()
		{
			keybinder.Bind ("/apps/do/bindings/activate",
						 kActivateShortcut,
						 OnActivate);
		}
		
		private void OnActivate (object sender, EventArgs args)
		{
			Show ();
		}
		
		protected void LoadAddins ()
		{
			List<string> addin_dirs;
			
			addin_dirs = new List<string> ();
			
			addin_dirs.Add ("~/.do/addins".Replace ("~",
				   System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal)));
			
			foreach (string addin_dir in addin_dirs) {
				string[] files;
				
				files = System.IO.Directory.GetFiles (addin_dir);
				foreach (string file in files) {
					Assembly addin;
					
					if (!file.EndsWith (".dll")) continue;
					try {
						addin = Assembly.LoadFile (file);
						LoadAssembly (addin);
					} catch (Exception e) {
						Log.Error ("Do encountered and error while trying to load addin {0}: {1}", file, e.Message);
						continue;
					}
				}
			}
		}
		
		private void LoadAssembly (Assembly addin)
		{
			if (addin == null) return;
			
			foreach (Type type in addin.GetTypes ()) {
				
				if (type.IsAbstract) continue;
				if (type == typeof(VoidCommand)) continue;
				
				foreach (Type iface in type.GetInterfaces ()) {
					if (iface == typeof (IItemSource)) {
						IItemSource source;
						
						source = System.Activator.CreateInstance (type) as IItemSource;
						itemManager.AddItemSource (new ItemSource (source));
						Log.Info ("Successfully loaded \"{0}\" Item Source.", source.Name);
					}
					if (iface == typeof (ICommand)) {
						ICommand command;
						
						command = System.Activator.CreateInstance (type) as ICommand;
						commandManager.AddCommand (new Command (command));
						Log.Info ("Successfully loaded \"{0}\" Command.", command.Name);
					}
				}
			}
		}
		
		protected abstract void OnVisibilityChanged (bool visible);
			
		public void SearchItems (string itemSearchString)
		{
			State = CommanderState.SearchingItems;
			
			this.itemSearchString = itemSearchString;
			commandSearchString = "";
			currentItems = itemManager.ItemsForAbbreviation (itemSearchString);
			if (currentItems.Length == 0) {
				currentItems = new Item[] { new Item (new TextItem (itemSearchString)) };
			}
			
			// Update items and commands state.
			CurrentItemIndex = 0;
			
			State = CommanderState.ItemSearchComplete;
		}
		
		public void SearchCommands (string commandSearchString)
		{
			State = CommanderState.SearchingCommands;
			
			this.commandSearchString = commandSearchString;
			currentCommands = commandManager.CommandsForItem (CurrentItem, commandSearchString);
			
			// Update items and commands state.
			if (currentCommands.Length >  0) {
				CurrentCommandIndex = 0;
			} else {
				currentCommandIndex = -1;
			}
			
			State = CommanderState.CommandSearchComplete;
		}
		
		public void Execute ()
		{
			Item o;
			Command c;

			o = this.CurrentItem;
			c = this.CurrentCommand;
			if (o != null && c != null) {
				c.Perform (new IItem[] {o}, new IItem[] {});
			}
		}
		
		// ICommand members
		
		public void Show ()
		{
			VisibilityChanged (true);
		}
		
		public void Hide ()
		{
			VisibilityChanged (false);
		}
		
	}
}
