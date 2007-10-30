// Main.cs created with MonoDevelop
// User: dave at 8:25 PM 8/16/2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//
// project created on 8/16/2007 at 8:25 PM

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
		
		private string itemSearchString, commandSearchString;
				
		public static ItemSource [] BuiltinItemSources {
			get {
				return new ItemSource [] {
					new ItemSource (new ApplicationItemSource ()),
					new ItemSource (new FirefoxBookmarkItemSource ()),
					// Index contents of Home (~) directory to 1 level
					new ItemSource (new DirectoryFileItemSource ("~", 1)),
					// Index contents of ~/Documents to 3 levels
					new ItemSource (new DirectoryFileItemSource ("~/Documents", 3)),
					// Index contents of ~/Desktop to 1 levels
					new ItemSource (new DirectoryFileItemSource ("~/Desktop", 2)),
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
					// new Command (new VoidCommand ()),
				};
			}
		}
		
		public Commander () {
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
		
		protected void LoadBuiltins ()
		{
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
		
		public void Execute (SearchContext executeContext)
		{
			Item o;
			Command c;

			o = executeContext.Item;
			c = executeContext.Command;
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
