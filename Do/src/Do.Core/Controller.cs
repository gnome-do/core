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
using System.Reflection;
using System.Collections.Generic;

using Gdk;
using Mono.Unix;
using Mono.Addins.Gui;

using Do.UI;
using Do.Addins;
using Do.Universe;
using Do.DBusLib;

namespace Do.Core
{
	
	public class Controller : IController, IDoController
	{
		protected IDoWindow window;
		protected Gtk.Window addinWindow;
		protected Gtk.AboutDialog aboutWindow;
		protected SearchContext[] context;
		
		const int SearchDelay = 225;
		
		uint[] searchTimeout;
		IAction action;
		List<IItem> items;
		List<IItem> modItems;
		bool thirdPaneVisible;
		bool tabbing = false;
		bool resultsGrown;
		
		public Controller ()
		{
			aboutWindow = null;
			addinWindow = null;
			items = new List<IItem> ();
			modItems = new List<IItem> ();
			searchTimeout = new uint[3];
			context = new SearchContext[3];
			resultsGrown = false;
		}
		
		public void Initialize ()
		{
			if (Do.Preferences.UseMiniMode) {
				window = new MiniWindow (this);
			} else if (Do.Preferences.UseGlassFrame) {
				window = new GlassWindow (this);
			} else {
				window = new ClassicWindow (this);
			}
			// Get key press events from window since we want to control that
			// here.
			window.KeyPressEvent += KeyPressWrap;
			Reset ();
		}

		bool IsSummonable {
			get {
				return aboutWindow == null && 
					(addinWindow == null || !addinWindow.Visible);
			}
		}

		/// <value>
		/// Convenience Method
		/// </value>
		SearchContext CurrentContext {
			get {
				return context[(int) CurrentPane];
			}
			set {
				context[(int) CurrentPane] = value;
			}
		}
		
		/// <value>
		/// The currently active pane, setting does not imply searching
		/// currently
		/// </value>
		public Pane CurrentPane {
			set {
				if (window.CurrentPane == Pane.First &&
					CurrentContext.Results.Length == 0)
					return;
				
				switch (value) {
				case Pane.First:
					window.CurrentPane = Pane.First;
					break;
				case Pane.Second:
					window.CurrentPane = Pane.Second;
					break;
				case Pane.Third:
					if (ThirdPaneAllowed)
						window.CurrentPane = Pane.Third;
					break;
				}

				// Determine if third pane needed
				if (!ThirdPaneAllowed || 
				    (!ThirdPaneRequired &&
					 context[2].Query.Length == 0 &&
					 context[2].Cursor == 0))
					ThirdPaneVisible = false;
				
				if ((ThirdPaneAllowed && window.CurrentPane == Pane.Third) ||
						ThirdPaneRequired)
					ThirdPaneVisible = true;
			}
			
			get {
				return window.CurrentPane;
			}
		}
		
		bool ThirdPaneVisible {
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
		
		bool ThirdPaneAllowed {
			get {
				IObject first, second;
				IAction action;

				first = GetSelection (Pane.First);
				second = GetSelection (Pane.Second);
				action = (first as IAction) ?? (second as IAction);
				return action != null &&
					action.SupportedModifierItemTypes.Length > 0 &&
					context[1].Results.Length > 0;
			}
		}

		bool ThirdPaneRequired {
			get {
				IObject first, second;
				IAction action;

				first = GetSelection (Pane.First);
				second = GetSelection (Pane.Second);
				action = (first as IAction) ?? (second as IAction);
				return action != null &&
					action.SupportedModifierItemTypes.Length > 0 &&
					!action.ModifierItemsOptional &&
					context[1].Results.Length > 0;
			}
		}

		public bool IsSummoned {
			get {
				return null != window && window.Visible;
			}
		}
		
		/// <summary>
		/// Summons a window with objects in it... seems to work
		/// </summary>
		/// <param name="objects">
		/// A <see cref="IObject"/>
		/// </param>
		public void SummonWithObjects (IObject[] objects)
		{
			if (!IsSummonable) return;
			
			Reset ();
			
			//Someone is going to need to explain this to me
			context[0].Results = objects;
			context[0].LastContext.LastContext = context[0].LastContext = context[0];
			
			SearchFirstPane ();
			SearchSecondPane ();

			Summon ();
			// If there are multiple results, show results window after a short
			// delay.
			if (objects.Length > 1) {
				GLib.Timeout.Add (50,
					delegate {
						Gdk.Threads.Enter ();
						GrowResults ();
						Gdk.Threads.Leave ();
						return false;
					}
				);
			}
		}
		
		/////////////////////////
		/// Key Handling ////////
		/////////////////////////

		private void KeyPressWrap (Gdk.EventKey evnt)
		{
			if ((evnt.State & ModifierType.ControlMask) != 0) {
					return;
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
			case Gdk.Key.KP_Enter:
				OnActivateKeyPressEvent (evnt);
				break;
			case Gdk.Key.Delete:
			case Gdk.Key.BackSpace:
				OnDeleteKeyPressEvent (evnt);
				break;
			case Gdk.Key.Tab:
			case Gdk.Key.ISO_Left_Tab:
				OnTabKeyPressEvent (evnt);
				break;
			case Gdk.Key.Up:
			case Gdk.Key.Down:
			case Gdk.Key.Home:
			case Gdk.Key.End:
				OnUpDownKeyPressEvent (evnt);
				break;
			case Gdk.Key.Right:
			case Gdk.Key.Left:
				OnRightLeftKeyPressEvent (evnt);
				break;
			default:
				OnInputKeyPressEvent (evnt);
				break;
			}
			return;
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
			bool results, something_typed;

			something_typed = CurrentContext.Query.Length > 0;
			results = CurrentContext.Results.Length > 0;
			
			ClearSearchResults ();
			
			if (!ThirdPaneAllowed)
				ThirdPaneVisible = false;
			
			ShrinkResults ();
			
			if (CurrentPane == Pane.First && !results) Vanish ();
			else if (!something_typed) Reset ();
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
		
		void OnRightLeftKeyPressEvent (EventKey evnt)
		{
			if (CurrentContext.Results.Length > 0) {
				if ((Gdk.Key) evnt.KeyValue == Gdk.Key.Right) {
					CurrentContext.ChildrenSearch = true;
					QueueSearch (false);
				} else if ((Gdk.Key) evnt.KeyValue == Gdk.Key.Left) {
					CurrentContext.ParentSearch = true;
					QueueSearch (false);
				}
				window.SetPaneContext(CurrentPane, CurrentContext);
			}
			GrowResults ();
		}
		
		void OnTabKeyPressEvent (EventKey evnt)
		{
			ShrinkResults ();
			
			tabbing = true;

			if (evnt.Key == Key.Tab) {
				NextPane ();
			} else if (evnt.Key == Key.ISO_Left_Tab) {
				PrevPane ();
			}
			if (!(CurrentPane == Pane.First && CurrentContext.Results.Length == 0))
				window.SetPaneContext (CurrentPane, CurrentContext);
			
			tabbing = false;
		}
		
		void OnUpDownKeyPressEvent (EventKey evnt)
		{
			
			if (evnt.Key == Gdk.Key.Up) {
				if (CurrentContext.Cursor <= 0) {
					ShrinkResults ();
					return;
				}
				CurrentContext.Cursor--;
			} else if (evnt.Key == Gdk.Key.Down) {
				if (!resultsGrown) {
					GrowResults ();
					return;
				}
				CurrentContext.Cursor++;
			} else if (evnt.Key == Gdk.Key.Home) {
				CurrentContext.Cursor = 0;
			} else if (evnt.Key == Gdk.Key.End) {
				CurrentContext.Cursor = CurrentContext.Results.Length - 1;
			}
			
			//We don't want to search the "default" state if the user presses down
			if (tabbing || CurrentContext.Results.Length == 0) return;
			UpdatePane (CurrentPane);
			
			switch (CurrentPane) {
			case Pane.First:
				context[1] = new SearchContext ();
				SearchPaneDelayed (Pane.Second);
				break;
			case Pane.Second:
				context[2] = new SearchContext ();
				SearchPaneDelayed (Pane.Third);
				break;
			}
		}
		
		/// <summary>
		/// Selects the logical next pane in the UI left to right
		/// </summary>
		void NextPane ()
		{
			switch (CurrentPane) {
			case Pane.First:
				CurrentPane = Pane.Second;
				break;
			case Pane.Second:
				if (ThirdPaneAllowed)
					CurrentPane = Pane.Third;
				else
					CurrentPane = Pane.First;
				break;
			case Pane.Third:
				CurrentPane = Pane.First;
				break;
			}
		}
		
		/// <summary>
		/// Selects the logical previous pane in the UI left to right
		/// </summary>
		void PrevPane ()
		{
			switch (CurrentPane) {
			case Pane.First:
				if (ThirdPaneAllowed)
					CurrentPane = Pane.Third;
				else
					CurrentPane = Pane.Second;
				break;
			case Pane.Second:
				CurrentPane = Pane.First;
				break;
			case Pane.Third:
				CurrentPane = Pane.Second;
				break;
			}
		}

		void QueueSearch (bool delayed)
		{
			if (delayed) {
				SearchPaneDelayed (CurrentPane);
				return;
			}

			switch (CurrentPane) {
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
		
		/// <summary>
		/// Searches panes with a delay for the sake of beauty.
		/// </summary>
		/// <param name="pane">
		/// A <see cref="Pane"/>
		/// </param>
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

			lastResult = GetSelection (Pane.First);

			// If we delete the entire query on a regular search (we are not
			// searching children) then set default state.
			if (context[0].Query == "" &&
					// DR, I could kill you right now.
					context[0].LastContext.LastContext.LastContext == null &&
					context[0].ParentContext == null) {
				Reset ();
				return;
			}

			context[0].SearchTypes = new Type[] { typeof (IItem), typeof (IAction) };
			Do.UniverseManager.Search (ref context[0]);
			UpdatePane (Pane.First);

			// Queue a search for the next pane unless the result of the most
			// recent search is the same as the last result - if this is the
			// case, we already have a valid search queued.
			if (GetSelection (Pane.First) != lastResult) {
				context[1] = new SearchContext ();
				SearchPaneDelayed (Pane.Second);
			}
		}
		
		protected void SearchSecondPane ()
		{
			IObject first;
			IObject lastResult;

			lastResult = GetSelection (Pane.Second);

			// Set up the next pane based on what's in the first pane:
			first = GetSelection (Pane.First);
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
			UpdatePane (Pane.Second);

			// Queue a search for the next pane unless the result of the most
			// recent search is the same as the last result - if this is the
			// case, we already have a valid search queued.
			if (GetSelection (Pane.Second) != lastResult) {
				context[2] = new SearchContext ();
				SearchPaneDelayed (Pane.Third);
			}
		}
		
		protected void SearchThirdPane ()
		{
			IObject first, second;

			context[2].SearchTypes = new Type[] { typeof (IItem) };

			first = GetSelection (Pane.First);
			second = GetSelection (Pane.Second);
			if (first == null || second == null) {
				window.SetPaneContext (Pane.Third, context[2]);
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
			UpdatePane (Pane.Third);

			if (ThirdPaneRequired) {
				ThirdPaneVisible = true;
			} else if (!ThirdPaneAllowed || 
			           (context[2].Query.Length == 0 && CurrentPane != Pane.Third)) {
				ThirdPaneVisible = false;
			}
		}
		
		protected void ClearSearchResults ()
		{
			switch (CurrentPane) {
			case Pane.First:
				Reset ();
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
		
		/////////////////////////
		// Pane Update Methods //
		/////////////////////////

		protected void UpdatePane (Pane pane)
		{
			window.SetPaneContext (pane, context[(int) pane]);
		}
		
		/// <summary>
		/// Call to fully reset Do.  This should return Do to its starting state
		/// </summary>
		void Reset ()
		{
			for (int i = 0; i < 3; ++i) {
				if (searchTimeout[i] > 0) 
					GLib.Source.Remove (searchTimeout[i]);
				searchTimeout[i] = 0;
			}
			
			ThirdPaneVisible = false;
			
			context[0] = new SearchContext ();
			context[1] = new SearchContext ();
			context[2] = new SearchContext ();
			
			//Must happen after new searchcontext's are set
			CurrentPane = Pane.First;
			
			ShrinkResults ();
			window.Reset ();
		}
		
		/// <summary>
		/// Should cause UI to display more results
		/// </summary>
		void GrowResults ()
		{
			window.GrowResults ();
			resultsGrown = true;	
		}
		
		/// <summary>
		/// Should cause UI to display fewer results, 0 == no results displayed
		/// </summary>
		void ShrinkResults ()
		{
			window.ShrinkResults ();
			resultsGrown = false;
		}
		
		IObject GetSelection (Pane pane)
		{
			IObject o;

			try {
				o = context[(int) pane].Selection;
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

			first = GetSelection (Pane.First);
			second = GetSelection (Pane.Second);
			third = GetSelection (Pane.Third);
			// User may have pressed enter before delayed search completed.
			// We guess this is the case if there is nothing in the second pane,
			// so we immediately do a search and use the first result.
			if (first != null && second == null) {
				SearchSecondPane ();
				second = GetSelection (Pane.Second);
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
				Reset ();
			}
		}

		///////////////////////////
		/// IController Members ///
		///////////////////////////
		
		public void Summon ()
		{
			if (!IsSummonable) return;
			window.Summon ();
		}
		
		public void Vanish ()
		{
			ShrinkResults ();
			window.Vanish ();
		}	

		public void ShowPluginManager ()
		{
			Vanish ();
			Reset ();

			addinWindow = AddinManagerWindow.Show (null);
			addinWindow.DestroyEvent += delegate {
				addinWindow = null;
			};
		}

		public void ShowAbout ()
		{
			string[] authors;
			string[] logos;
			string logo;

			Vanish ();
			Reset ();

			authors = new string[] {
				"Chris Halse Rogers <chalserogers@gmail.com>",
				"David Siegel <djsiegel@gmail.com>",
				"DR Colkitt <douglas.colkitt@gmail.com>",
				"James Walker",
				"Jason Smith",
				"Miguel de Icaza",
				"Rick Harding",
				"Thomsen Anders",
				"Volker Braun"
			};

			aboutWindow = new Gtk.AboutDialog ();
			aboutWindow.Name = "GNOME Do";

			try {
				AssemblyName name = Assembly.GetEntryAssembly ().GetName ();
				aboutWindow.Version = String.Format ("{0}.{1}.{2}",
					name.Version.Major, name.Version.Minor, name.Version.Build);
			} catch {
				aboutWindow.Version = Catalog.GetString ("Unknown");
			}
			
			logos = new string[] {
				"/usr/share/icons/gnome/scalable/actions/search.svg",
			};

			logo = "gnome-run";
			foreach (string l in logos) {
				if (!System.IO.File.Exists (l)) continue;
				logo = l;
			}

			aboutWindow.Logo = UI.IconProvider.PixbufFromIconName (logo, 140);
			aboutWindow.Copyright = "Copyright \xa9 2008 GNOME Do Developers";
			aboutWindow.Comments = "Do things as quickly as possible\n" +
				"(but no quicker) with your files, bookmarks,\n" +
				"applications, music, contacts, and more!";
			aboutWindow.Website = "http://do.davebsd.com/";
			aboutWindow.WebsiteLabel = "Visit Homepage";
			aboutWindow.Authors = authors;
			aboutWindow.IconName = "gnome-run";

			if (null != aboutWindow.Screen.RgbaColormap) {
				Gtk.Widget.DefaultColormap = aboutWindow.Screen.RgbaColormap;
			}

			aboutWindow.Run ();
			aboutWindow.Destroy ();
			aboutWindow = null;
		}
		
		/////////////////////////////
		/// IDoController Members ///
		/////////////////////////////
		
		public void NewContextSelection (Pane pane, int index)
		{
			if (context[(int) pane].Results.Length == 0) return;
			context[(int) pane].Cursor = index;
			window.SetPaneContext (pane, context[(int) pane]);
			
			if (pane != CurrentPane)
				return;
			
			switch (CurrentPane) {
			case Pane.First:
				context[1] = new SearchContext ();
				SearchPaneDelayed (Pane.Second);
				break;
			case Pane.Second:
				context[2] = new SearchContext ();
				SearchPaneDelayed (Pane.Third);
				break;
			}
		}

		public void ButtonPressOffWindow ()
		{
			Vanish ();
			Reset ();
		}
	}
}
