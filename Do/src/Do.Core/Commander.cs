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
		
		public static ItemSource [] BuiltinItemSources {
			get {
				return new ItemSource [] {
					new ItemSource (new ApplicationItemSource ()),
					new ItemSource (new FirefoxBookmarkItemSource ()),
					new ItemSource (new DirectoryFileItemSource ()),
					new ItemSource (new GNOMESpecialLocationsItemSource ()),
					
					new ItemSource (new EvolutionContactItemSource ()),
					new ItemSource (new PidginContactItemSource ()),	
				};
			}
		}
		
		public static Command [] BuiltinCommands {
			get {
				return new Command [] {
					new Command (new RunCommand ()),
					new Command (new OpenCommand ()),
					new Command (new OpenURLCommand ()),
					new Command (new RunInShellCommand ()),
					new Command (new DefineWordCommand ()),
					
					new Command (new MailtoCommand ()),
					new Command (new PidginChatCommand ()),
				};
			}
		}
		
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
			LoadAssemblies ();
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
			foreach (ItemSource source in BuiltinItemSources) {
				itemManager.AddItemSource (source);
			}
			foreach (Command command in BuiltinCommands) {
				commandManager.AddCommand (command);
			}
		}
		
		protected virtual void SetupKeybindings ()
		{
			keybinder.Bind ("/apps/do/bindings/activate",
						 "<Control>space",
						 OnActivate);
		}
		
		private void OnActivate (object sender, EventArgs args)
		{
			Show ();
		}
		
		protected void LoadAssemblies ()
		{
			/*
			Assembly currentAssembly;
			string appAssembly;
			
			appAssembly = "/home/dave/Current Documents/gnome-commander/gnome-commander-applications/bin/Debug/gnome-commander-applications.dll";
			currentAssembly = Assembly.LoadFile (appAssembly);
			
			foreach (Type type in currentAssembly.GetTypes ())
			foreach (Type iface in type.GetInterfaces ()) {
				if (iface == typeof (IItemSource)) {
					IItemSource source = currentAssembly.CreateInstance (type.ToString ()) as IItemSource;
					itemManager.AddItemSource (new ItemSource (source));
				}
				if (iface == typeof (ICommand)) {
					ICommand command = currentAssembly.CreateInstance (type.ToString ()) as ICommand;
					commandManager.AddCommand (new Command (command));
				}
			}
			*/
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
