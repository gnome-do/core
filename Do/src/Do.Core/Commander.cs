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
			FirstSearchComplete,
			SearchingCommands,
			SecondSearchComplete
	}
	
	public abstract class Commander : ICommander {
		
		protected Tomboy.GConfXKeybinder keybinder;
		
		private CommanderState state;
		
		public event OnCommanderStateChange SetSearchingItemsStateEvent;
		public event OnCommanderStateChange SetFirstCompleteStateEvent;
		public event OnCommanderStateChange SetSearchingCommandsStateEvent;
		public event OnCommanderStateChange SetSecondCompleteStateEvent;
		public event OnCommanderStateChange SetDefaultStateEvent;
		
		public event VisibilityChangedHandler VisibilityChanged;
				
		public static ItemSource [] BuiltinItemSources {
			get {
				return new ItemSource [] {
					new ItemSource (new ApplicationItemSource ()),
					new ItemSource (new FirefoxBookmarkItemSource ()),
					new ItemSource (new DirectoryFileItemSource ()),
					new ItemSource (new GNOMESpecialLocationsItemSource()),
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
				};
			}
		}
		
		public Commander () {
			keybinder = new Tomboy.GConfXKeybinder ();
			
			SetSearchingItemsStateEvent = SetSearchingItemsState;
			SetFirstCompleteStateEvent = SetFirstSearchCompleteState;
			SetSearchingCommandsStateEvent = SetSearchingCommandsState;
			SetSecondCompleteStateEvent = SetSecondSearchCompleteState;
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
				case CommanderState.FirstSearchComplete:
					SetFirstCompleteStateEvent ();
					break;
				case CommanderState.SearchingCommands:
					SetSearchingCommandsStateEvent ();
					break;
				case CommanderState.SecondSearchComplete:
					SetSecondCompleteStateEvent ();
					break;
				}
			}
		}
		
		protected virtual void SetDefaultState ()
		{
		}
		
		protected virtual void SetSearchingItemsState ()
		{
		}
		
		protected virtual void SetFirstSearchCompleteState ()
		{
		}
		
		protected virtual void SetSearchingCommandsState ()
		{
		}
		
		protected virtual void SetSecondSearchCompleteState ()
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
			GCObject firstResult = executeContext.FirstObject;
			GCObject secondResult = executeContext.SecondObject;

			Item o = null;
			Command c = null;
			
			if (firstResult != null) {
				if (firstResult.GetType ().Equals (typeof (Item))) {
					o = (Item) firstResult;
					c = (Command) secondResult;
				}
				else {
					o = (Item) secondResult;
					c = (Command) firstResult;
				}
			}

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
