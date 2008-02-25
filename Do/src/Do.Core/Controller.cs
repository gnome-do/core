/* Controller.cs
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

using Gdk;
using Mono.Unix;

using Do.DBusLib;
using Do.Universe;
using Do.Addins;
using Do.Addins.UI;

namespace Do.Core
{
	
	public class Controller : IController
	{
		//-------------------- Class Members--------------------//
		public event EventHandler Vanished;

		protected IDoWindow window;
		protected SearchContext[] context;
		
		const int SearchDelay = 225;
		
		uint[] searchTimeout;
		IAction action;
		List<IItem> items;
		List<IItem> modItems;
		bool thirdPaneVisible;
		bool tabbing = false;
		
		//-------------------- Class Properties-----------------//
		
		SearchContext CurrentContext
		{
			get {
				return context[(int) window.CurrentPane];
			}
			set {
				context[(int) window.CurrentPane] = value;
			}
		}
		
		bool ThirdPaneVisible
		{
			set {
				if (value == thirdPaneVisible)
					return;
				
				if (value == true)
					window.Grow ();
				else
					window.Shrink ();
				thirdPaneVisible = value;
			}
			
			get {
				return thirdPaneVisible;
			}
		}
		
		bool ThirdPaneAllowed
		{
			get {
				IObject first, second;
				IAction action;

				first = GetCurrentObject (Pane.First);
				second = GetCurrentObject (Pane.Second);
				action = (first as IAction) ?? (second as IAction);
				return action != null &&
					action.SupportedModifierItemTypes.Length > 0 &&
					context[1].Results.Length > 0;
			}
		}

		bool ThirdPaneRequired
		{
			get {
				IObject first, second;
				IAction action;

				first = GetCurrentObject (Pane.First);
				second = GetCurrentObject (Pane.Second);
				action = (first as IAction) ?? (second as IAction);
				return action != null &&
					action.SupportedModifierItemTypes.Length > 0 &&
					!action.ModifierItemsOptional &&
					context[1].Results.Length > 0;
			}
		}
		
		//-------------------- NESTED CLASS --------------------//
		
		class NoResultsFoundObject : IObject
		{
			string query;

			public NoResultsFoundObject (string query)
			{
				this.query = query;
			}

			public string Icon { get { return "gtk-dialog-question"; } }
			public string Name { get { return Catalog.GetString ("No results found."); } }

			public string Description
			{
				get {
					return string.Format (Catalog.GetString ("No results found for \"{0}\"."), query);
				}
			}
		}
		
		//-------------------- CONSTRUCTOR ---------------------//
		public Controller ()
		{
			items = new List<IItem> ();
			modItems = new List<IItem> ();
			searchTimeout = new uint[3];
			context = new SearchContext[3];
		}
		
		//-------------------- METHODS -------------------------//

		public void Initialize ()
		{
			//window = new SymbolWindow ();
			window.KeyPressEvent += KeyPressWrap;
		}

		protected void NotifyVanished ()
		{
			if (Vanished != null) {
				Vanished (this, new EventArgs ());
			}
		}

		public bool IsSummoned {
			get {
				return null != window && window.Visible;
			}
		}
		
		public void SummonWithObjects (IObject[] objects)
		{
			if (!window.IsSummonable) return;
			SearchContext search = new SearchContext ();
			search.Results = objects;
			
			window.DisplayObjects (search);
			Summon ();
		}
		
		/************************************************
		 * -------------KEYPRESS HANDLING----------------
		 * **********************************************/
		private void KeyPressWrap (object o, Gtk.KeyPressEventArgs args)
		{
			Gdk.EventKey evnt = args.Event;
			
			if ((evnt.State & ModifierType.ControlMask) != 0) {
					//sOnControlKeyPressEvent (evnt);
					return; //base.OnKeyPressEvent (evnt);
			}

			switch ((Gdk.Key) evnt.KeyValue) {
				// Throwaway keys
				case Gdk.Key.Shift_L:
				case Gdk.Key.Control_L:
					break;
				case Gdk.Key.Escape:
					OnEscapeKeyPressEvent (evnt);
					break;
				case Gdk.Key.Return:
				case Gdk.Key.ISO_Enter:
					OnActivateKeyPressEvent (evnt);
					break;
				case Gdk.Key.Delete:
				case Gdk.Key.BackSpace:
					OnDeleteKeyPressEvent (evnt);
					break;
				case Gdk.Key.Tab:
					OnTabKeyPressEvent (evnt);
					break;
				case Gdk.Key.Up:
				case Gdk.Key.Down:
					//OnUpDownKeyPressEvent (evnt);
					break;
				case Gdk.Key.Right:
				case Gdk.Key.Left:
					//OnRightLeftKeyPressEvent (evnt);
					break;
				default:
					OnInputKeyPressEvent (evnt);
					break;
			}
			return; //base.OnKeyPressEvent (evnt);
		}
		
		void OnActivateKeyPressEvent (EventKey evnt)
		{
			bool shift_pressed = (evnt.State & ModifierType.ShiftMask) != 0;
			PerformAction (!shift_pressed);
		}
		
		void OnDeleteKeyPressEvent (EventKey evnt)
		{
			string query;

			query = CurrentContext.Query;
			if (query.Length == 0) return;
			CurrentContext.Query = query.Substring (0, query.Length-1);
			QueueSearch (false);
		}
		
		void OnEscapeKeyPressEvent (EventKey evnt)
		{
			bool results;

			results = CurrentContext.Results.Length > 0;
			
			ClearSearchResults ();

			window.Reset ();
			if (window.CurrentPane == Pane.First && !results) 
				window.Vanish ();
		}
		
		void OnInputKeyPressEvent (EventKey evnt)
		{
			char c;

			c = (char) Gdk.Keyval.ToUnicode (evnt.KeyValue);
			if (char.IsLetterOrDigit (c)
					|| char.IsPunctuation (c)
					|| c == ' '
					|| char.IsSymbol (c)) {
				CurrentContext.Query += c;
				QueueSearch (false);
			}
		}
		
		void OnTabKeyPressEvent (EventKey evnt)
		{
			tabbing = true;
			//resultsWindow.Hide ();
			if (window.CurrentPane == Pane.First &&
					context[0].Results.Length != 0) {
				window.CurrentPane = Pane.Second;
			} else if (window.CurrentPane == Pane.Second && ThirdPaneAllowed) {
				window.CurrentPane = Pane.Third;
				ThirdPaneVisible = true;
			} else if (window.CurrentPane == Pane.Third && !ThirdPaneRequired) {
				// This is used for actions for which modifier items are optional.
				window.CurrentPane = Pane.First;
				ThirdPaneVisible = false;
			} else {
				window.CurrentPane = Pane.First;
			}
			tabbing = false;
		}
		
		/************************************************
		 * --------------Search Method-------------------
		 * **********************************************/
		
		void QueueSearch (bool delayed)
		{
			if (delayed) {
				SearchPaneDelayed (window.CurrentPane);
				return;
			}

			switch (window.CurrentPane) {
				case Pane.First:
					SearchFirstPane ();
					break;
				case Pane.Second:
					SearchSecondPane ();
					break;
				case Pane.Third:
					SearchThirdPane ();
					break;
			}
		}
		
		void SearchPaneDelayed (Pane pane)
		{
			for (int i = 0; i < 3; ++i) {
				if (searchTimeout[i] > 0) 
					GLib.Source.Remove (searchTimeout[i]);
				searchTimeout[i] = 0;
			}
			for (int i = (int) pane; i < 3; ++i) {
					window.ClearPane((Pane) i);
			}

			
			searchTimeout[(int) pane] = GLib.Timeout.Add (SearchDelay, delegate {
				Gdk.Threads.Enter ();
				switch (pane) {
					case Pane.First:
						SearchFirstPane ();
						break;
					case Pane.Second:
						SearchSecondPane ();
						break;
					case Pane.Third:
						SearchThirdPane ();
						break;
				}
				Gdk.Threads.Leave ();
				return false;
			});
		}
		
		protected void SearchFirstPane ()
		{
			IObject lastResult;

			lastResult = GetCurrentObject (Pane.First);

			// If we delete the entire query on a regular search (we are not
			// searching children) then set default state.
			if (context[0].Query == "" &&
					// DR, I could kill you right now.
					context[0].LastContext.LastContext.LastContext == null &&
					context[0].ParentContext == null) {
				window.Reset ();
				return;
			}

			context[0].SearchTypes = new Type[] { typeof (IItem), typeof (IAction) };
			Do.UniverseManager.Search (ref context[0]);
			UpdatePane (Pane.First, true);

			// Queue a search for the next pane unless the result of the most
			// recent search is the same as the last result - if this is the
			// case, we already have a valid search queued.
			if (GetCurrentObject (Pane.First) != lastResult) {
				context[1] = new SearchContext ();
				SearchPaneDelayed (Pane.Second);
			}
		}
		
		protected void SearchSecondPane ()
		{
			IObject first;
			IObject lastResult;

			lastResult = GetCurrentObject (Pane.Second);

			// Set up the next pane based on what's in the first pane:
			first = GetCurrentObject (Pane.First);
			if (first is IItem) {
				// Selection is an IItem
				context[1].Items.Clear ();
				context[1].Items.Add (first as IItem);
				context[1].SearchTypes = new Type[] { typeof (IAction) };
			} else {
				// Selection is an IAction
				context[1].Action = first as IAction;
				context[1].SearchTypes = new Type[] { typeof (IItem) };
			}

			Do.UniverseManager.Search (ref context[1]);
			UpdatePane (Pane.Second, true);

			// Queue a search for the next pane unless the result of the most
			// recent search is the same as the last result - if this is the
			// case, we already have a valid search queued.
			if (GetCurrentObject (Pane.Second) != lastResult) {
				context[2] = new SearchContext ();
				SearchPaneDelayed (Pane.Third);
			}
		}
		
		protected void SearchThirdPane ()
		{
			IObject first, second;

			context[2].SearchTypes = new Type[] { typeof (IItem) };

			first = GetCurrentObject (Pane.First);
			second = GetCurrentObject (Pane.Second);
			if (first == null || second == null) {
				SetNoResultsFoundState (Pane.Third);
				return;
			}

			if (first is IItem) {
				context[2].Items.Clear ();
				context[2].Items.Add (first as IItem);
				context[2].Action = second as IAction;
			} else {
				context[2].Items.Clear ();
				context[2].Items.Add (second as IItem);
				context[2].Action = first as IAction;
			}

			Do.UniverseManager.Search (ref context[2]);
			UpdatePane (Pane.Third, true);

			if (ThirdPaneRequired) {
				ThirdPaneVisible = true;
			} else if (!ThirdPaneAllowed) {
				ThirdPaneVisible = false;
			}
		}
		
		protected void ClearSearchResults ()
		{
			switch (window.CurrentPane) {
				case Pane.First:
					// Do this once we have "" in the first results list(?)
					// context[0] = new SearchContext ();
					// SearchFirstPane ();
					window.Reset ();
					break;
				case Pane.Second:
					context[1] = new SearchContext ();
					SearchSecondPane ();
					break;
				case Pane.Third:
					context[2] = new SearchContext ();
					SearchThirdPane ();
					break;
			}
		}
		
		/********************************************
		 * ---------- Pane Update Methods -----------
		 * ******************************************/
		
		protected void UpdatePane (Pane pane, bool updateResults)
		{
			IObject current;

			current = GetCurrentObject (pane);
			if (current != null) {
				//iconbox[(int) pane].DisplayObject = current;
				window.DisplayInPane (pane, current);
				
				//iconbox[(int) pane].Highlight = context[(int) pane].Query;
				window.SetPaneHighlight (pane, context[(int) pane].Query);
			} else {
				SetNoResultsFoundState (pane);
				return;
			}

			if (pane == window.CurrentPane) {
				window.DisplayInLabel (GetCurrentObject (pane));
				//FIXME
				if (updateResults) window.DisplayObjects (CurrentContext);
			}
		}
		
		protected void SetNoResultsFoundState (Pane pane)
		{
			NoResultsFoundObject none_found;

			if (pane == Pane.First) {
				window.ClearPane (Pane.First);
				window.ClearPane (Pane.Second);
			} else if (pane == Pane.Second) {
				window.ClearPane (Pane.Second);
			}

			none_found = new NoResultsFoundObject (context[(int) pane].Query);
			//iconbox[(int) pane].DisplayObject = none_found;
			window.DisplayInPane (pane, none_found);
			if (window.CurrentPane == pane) {
				window.DisplayInLabel (none_found);				
				window.DisplayObjects (new SearchContext ());
			}
		}
		
		/**************************************
		 * -------Object Related methods-------
		 * ************************************/
		
		IObject GetCurrentObject (Pane pane)
		{
			IObject o;

			try {
				o = context[(int) pane].Results[context[(int) pane].Cursor];
			} catch {
				o = null;
			}
			return o;
		}
		
		protected virtual void PerformAction (bool vanish)
		{
			IObject first, second, third;
			string actionQuery, itemQuery, modItemQuery;

			items.Clear ();
			modItems.Clear ();
			if (vanish) {
				Vanish ();
			}

			first = GetCurrentObject (Pane.First);
			second = GetCurrentObject (Pane.Second);
			third = GetCurrentObject (Pane.Third);
			// User may have pressed enter before delayed search completed.
			// We guess this is the case if there is nothing in the second pane,
			// so we immediately do a search and use the first result.
			if (first != null && second == null) {
				SearchSecondPane ();
				second = GetCurrentObject (Pane.Second);
			}

			if (first != null && second != null) {
				if (first is IItem) {
					items.Add (first as IItem);
					action = second as IAction;
					itemQuery = context[0].Query;
					actionQuery = context[1].Query;
				} else {
					items.Add (second as IItem);
					action = first as IAction;
					itemQuery = context[1].Query;
					actionQuery = context[0].Query;
				}
				if (third != null && ThirdPaneVisible) {
					modItems.Add (third as IItem);
					modItemQuery = context[2].Query;
					(third as DoObject).IncreaseRelevance (modItemQuery, null);
				}

				/////////////////////////////////////////////////////////////
				/// Relevance accounting
				/////////////////////////////////////////////////////////////
				
				// Increase the relevance of the item.
				// Someday this will need to be moved to allow for >1 item.
				(items[0] as DoObject).IncreaseRelevance (itemQuery, null);

				// Increase the relevance of the action alone:
				(action as DoAction).IncreaseRelevance (actionQuery, null);
				// Increase the relevance of the action /for each item/:
				foreach (DoObject item in items)
					(action as DoObject).IncreaseRelevance (actionQuery, item);

				action.Perform (items.ToArray (), modItems.ToArray ());
			}

			if (vanish) {
				window.Reset ();
			}
		}

		///////////////////////////
		/// IController Members ///
		///////////////////////////
		
		public void Summon ()
		{
			if (!window.IsSummonable) return;
			window.Summon ();
			
			//FIXME == This method requires a Gtk.Window... for desktop agnostic we can not do this...
			//Should we even do this?  The UI might not really want this...
			
			//Util.Appearance.PresentWindow (window);
		}
		
		public void Vanish ()
		{
			window.Vanish ();
			NotifyVanished ();
		}	
	}
}
