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
			searchTimeout = new uint[3];
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

		bool IsSummonable {
			get {
				return MainMenu.Instance.AboutDialog == null;
			}
		}
		
		public void SummonWithObjects (IObject[] objects)
		{
			if (!IsSummonable) return;
			window.DisplayObjects (objects);
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
					//OnEscapeKeyPressEvent (evnt);
					break;
				case Gdk.Key.Return:
				case Gdk.Key.ISO_Enter:
					//OnActivateKeyPressEvent (evnt);
					break;
				case Gdk.Key.Delete:
				case Gdk.Key.BackSpace:
					//OnDeleteKeyPressEvent (evnt);
					break;
				case Gdk.Key.Tab:
					//OnTabKeyPressEvent (evnt);
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
					//SearchFirstPane ();
					break;
				case Pane.Second:
					//SearchSecondPane ();
					break;
				case Pane.Third:
					//SearchThirdPane ();
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
						//SearchFirstPane ();
						break;
					case Pane.Second:
						//SearchSecondPane ();
						break;
					case Pane.Third:
						//SearchThirdPane ();
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
				//if (updateResults) resultsWindow.Context = CurrentContext;
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
				window.DisplayObjects (new IObject[0]);
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

		///////////////////////////
		/// IController Members ///
		///////////////////////////
		
		public void Summon ()
		{
			if (!IsSummonable) return;
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
