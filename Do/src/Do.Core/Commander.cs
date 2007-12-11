/* Commander.cs
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
	
	public abstract class Commander : ICommander
	{
		const string kActivateKeybinding = "<Super>space";
		
		protected Tomboy.GConfXKeybinder keybinder;
		
		private CommanderState state;
		
		public event OnCommanderStateChange SetSearchingItemsStateEvent;
		public event OnCommanderStateChange SetFirstCompleteStateEvent;
		public event OnCommanderStateChange SetSearchingCommandsStateEvent;
		public event OnCommanderStateChange SetSecondCompleteStateEvent;
		public event OnCommanderStateChange SetDefaultStateEvent;
		
		public event VisibilityChangedHandler VisibilityChanged;
		
		public Commander ()
		{
			keybinder = new Tomboy.GConfXKeybinder ();
			
			SetSearchingItemsStateEvent = SetSearchingItemsState;
			SetFirstCompleteStateEvent = SetFirstSearchCompleteState;
			SetSearchingCommandsStateEvent = SetSearchingCommandsState;
			SetSecondCompleteStateEvent = SetSecondSearchCompleteState;
			SetDefaultStateEvent = SetDefaultState;
			VisibilityChanged = OnVisibilityChanged;
			
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
		
		protected virtual void SetupKeybindings ()
		{
			GConf.Client client;
			string binding;

			client = new GConf.Client();
			try {
				binding = client.Get ("/apps/gnome-do/preferences/key_binding") as string;
			} catch {
				binding = kActivateKeybinding;
				client.Set ("/apps/gnome-do/preferences/key_binding", binding);
			}
			keybinder.Bind ("/apps/gnome-do/preferences/key_binding", binding, OnActivate);
		}
		
		private void OnActivate (object sender, EventArgs args)
		{
			Show ();
		}
		
		protected abstract void OnVisibilityChanged (bool visible);
		
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
