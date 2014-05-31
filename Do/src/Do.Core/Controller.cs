// Controller.cs
//
// GNOME Do is the legal property of its developers. Please refer to the
// COPYRIGHT file distributed with this source distribution.
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
using System.Linq;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;

using Gdk;
using Mono.Unix;

using Do;
using Do.UI;
using Do.Universe;
using Do.Platform;
using Do.Platform.Linux;
using Do.Interface;

namespace Do.Core
{

	public class Controller : IDoController
	{
		
		const int SearchDelay = 250;

		IDoWindow window;
		bool results_grown;
		bool third_pane_visible;
		Gtk.IMContext im_context;
		ISearchController [] controllers;

		public event EventHandler Summoned;

		internal IDoWindow Window {
			get {
				return window;
			}
		}
		
		public Gtk.AboutDialog AboutDialog { get; private set; }
		public PreferencesWindow PreferencesWindow { get; private set; }
		
		public Controller ()
		{
			im_context = new Gtk.IMMulticontext ();
			results_grown = false;
			
			controllers = new SimpleSearchController [3];
			
			// Each controller needs to be aware of the controllers before it.
			// Going down the line however is not needed at current.
			controllers [0] = new FirstSearchController  ();
			controllers [1] = new SecondSearchController (controllers [0]);
			controllers [2] = new ThirdSearchController  (controllers [0], controllers [1]);
			
			// We want to show a blank box during our searches
			// controllers [0].SearchStarted += (u) => { };
			controllers [1].SearchStarted += (u) => {
				if (u && !ControllerExplicitTextMode (Pane.Second))
					window.ClearPane (Pane.Second); 
			};

			controllers [2].SearchStarted += (u) => { 
				if (u && !ControllerExplicitTextMode (Pane.Third))
					window.ClearPane (Pane.Third); 
			};
			
			controllers [0].SearchFinished +=
				(o, state) => SearchFinished (o, state, Pane.First);
			controllers [1].SearchFinished +=
				(o, state) => SearchFinished (o, state, Pane.Second);
			controllers [2].SearchFinished +=
				(o, state) => SearchFinished (o, state, Pane.Third);
			
			im_context.UsePreedit = false;
			im_context.Commit += OnIMCommit;
			im_context.FocusIn ();
			
		}
		
		void OnIMCommit (object sender, Gtk.CommitArgs e)
		{
			foreach (char c in e.Str)
				SearchController.AddChar (c);
		}

		public void Initialize ()
		{
			if (Do.Preferences.Theme == "Docky") {
				string message = Catalog.GetString (
					"<b>Docky is no longer a Do theme!</b>\n" + 
					"It is now available as a stand-alone application. " +
					"Your GNOME Do theme has been reset to Classic. " +
					"Please feel free to change it in Preferences."
				);

				Gtk.MessageDialog md = new Gtk.MessageDialog (null, Gtk.DialogFlags.DestroyWithParent, 
					Gtk.MessageType.Info, Gtk.ButtonsType.Ok, true, message);
				md.Response += (o, args) => md.Destroy ();
				md.ShowAll ();
				Do.Preferences.Theme = "Classic";
			}
			
			SetTheme (Do.Preferences.Theme);
			Do.Preferences.ThemeChanged += OnThemeChanged;
			Screen.Default.CompositedChanged += OnCompositingChanged;
			
			// Register Shortcuts
			SetupKeybindings ();
		}
		
		void SetupKeybindings ()
		{
			Services.Keybinder.RegisterKeyBinding (new KeyBinding ("Summon_Do",
					Catalog.GetString ("Summon Do"), "<Super>space", OnSummonKeyPressEvent, true));
			
			// this keybinding is disabled by default - note the empty keybinding
			Services.Keybinder.RegisterKeyBinding (new KeyBinding ("Summon_in_Text_Mode",
					Catalog.GetString ("Summon in Text Mode"), "", OnTextModeSummonKeyPressEvent, true));
			
			Services.Keybinder.RegisterKeyBinding (new KeyBinding ("Enter_Text_Mode",
					Catalog.GetString ("Enter Text Mode"), "period", OnTextModePressEvent));
			
			Services.Keybinder.RegisterKeyBinding (new KeyBinding ("Clear",
					Catalog.GetString ("Clear"), "Escape", OnClearKeyPressEvent));
			
			Services.Keybinder.RegisterKeyBinding (new KeyBinding ("Copy_to_Clipboard",
					Catalog.GetString ("Copy to Clipboard"), "<Control>c", OnCopyEvent));
			Services.Keybinder.RegisterKeyBinding (new KeyBinding ("Paste_from_Clipboard",
					Catalog.GetString ("Paste from Clipboard"), "<Control>v", OnPasteEvent));
			
			Services.Keybinder.RegisterKeyBinding (new KeyBinding ("Previous_Pane",
					Catalog.GetString ("Previous Pane"), "<Shift>Tab", OnPreviousPanePressEvent));
			Services.Keybinder.RegisterKeyBinding (new KeyBinding ("Next_Pane",
					Catalog.GetString ("Next Pane"), "Tab", OnNextPanePressEvent));
			
			Services.Keybinder.RegisterKeyBinding (new KeyBinding ("Previous_Item",
					Catalog.GetString ("Previous Item"), "Up", OnPreviousItemPressEvent));
			Services.Keybinder.RegisterKeyBinding (new KeyBinding ("Next_Item",
					Catalog.GetString ("Next Item"), "Down", OnNextItemPressEvent));
			
			Services.Keybinder.RegisterKeyBinding (new KeyBinding ("First_Item",
					Catalog.GetString ("First Item"), "Home", OnFirstItemPressEvent));
			Services.Keybinder.RegisterKeyBinding (new KeyBinding ("Last_Item",
					Catalog.GetString ("Last Item"), "End", OnLastItemPressEvent));
			
			Services.Keybinder.RegisterKeyBinding (new KeyBinding ("Previous_5_Results",
					Catalog.GetString ("Previous 5 Results"), "Page_Up", OnNextItemPagePressEvent));
			Services.Keybinder.RegisterKeyBinding (new KeyBinding ("Next_5_Results",
					Catalog.GetString ("Next 5 Results"), "Page_Down", OnPreviousItemPagePressEvent));
			
			Services.Keybinder.RegisterKeyBinding (new KeyBinding ("Step_Out_of_Item",
					Catalog.GetString ("Step Out of Item"), "Left", OnStepOutItemPressEvent));
			Services.Keybinder.RegisterKeyBinding (new KeyBinding ("Step_Into_Item",
					Catalog.GetString ("Step Into Item"), "Right", OnStepInItemPressEvent));
			
			Services.Keybinder.RegisterKeyBinding (new KeyBinding ("Select_Multiple_Items",
					Catalog.GetString ("Select Multiple Items"), "comma", OnSelectionKeyPressEvent));
		}

		void OnSummoned ()
		{
			if (Summoned == null) return;
			Summoned (this, EventArgs.Empty);
		}
		
		void OnCompositingChanged (object sender, EventArgs args)
		{
			UnsetTheme ();
			SetTheme (Do.Preferences.Theme);
		}
		
		private string LastTheme { get; set; }
		
		void OnThemeChanged (object sender, PreferencesChangedEventArgs e)
		{
			string newTheme = e.Value as string;
			
			// Only change the theme of the old and new themes are different.
			if (LastTheme != newTheme && !string.IsNullOrEmpty (newTheme)) {
				UnsetTheme ();
				SetTheme (newTheme);
			}
		}

		void UnsetTheme ()
		{
			if (window == null) return;
			
			Vanish ();
			window.KeyPressEvent -= KeyPressWrap;
			window.Dispose ();
			window = null;
		}

		void SetTheme (string themeName)
		{
			Log<Controller>.Debug ("Setting theme {0}", themeName);
			LastTheme = themeName;
			
			Orientation = ControlOrientation.Vertical;

			if ((Screen.Default.IsComposited) && (!Do.Preferences.ForceClassicWindow)) {
				window = InterfaceManager.MaybeGetInterfaceNamed (themeName) ?? new ClassicWindow ();
			} else {
				window = new ClassicWindow ();
			}
			
			window.Initialize (this);
			window.KeyPressEvent += KeyPressWrap;
			if (window is Gtk.Window)
				(window as Gtk.Window).Title = "Do";
			
			Reset ();
		}

		bool IsSummonable {
			get { return PreferencesWindow == null && AboutDialog == null; }
		}

		/// <value>
		/// Convenience Method
		/// </value>
		ISearchController SearchController {
			get { return controllers [(int) CurrentPane]; }
		}
		
		/// <value>
		/// The currently active pane, setting does not imply searching
		/// currently
		/// </value>
		public Pane CurrentPane {
			set {
				// If we have no results, we can't go to the second pane.
				if (window.CurrentPane == Pane.First &&
					!SearchController.Results.Any ())
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
				if (!ThirdPaneAllowed || ThirdPaneCanClose)
					ThirdPaneVisible = false;
				
				if ((ThirdPaneAllowed && window.CurrentPane == Pane.Third) || ThirdPaneRequired)
					ThirdPaneVisible = true;
			}
			
			get { return window.CurrentPane; }
		}
		
		/// <value>
		/// Check if First Controller is in a reset state
		/// </value>
		bool FirstControllerIsReset {
			get {
				return string.IsNullOrEmpty (controllers [0].Query) &&
					!controllers [0].Results.Any ();
			}
		}
		
		Pane WorkingActionPane {
			get {
				Item first, second;
				first = GetSelection (Pane.First);
				second = GetSelection (Pane.Second);
				
				if (first != null && second != null) {
					if (first.IsAction () && first.AsAction ().Safe.SupportsItem (second)) {
						return Pane.First;
					} else if (second.IsAction () && second.AsAction ().Safe.SupportsItem (first)) {
						return Pane.Second;
					}
				}
				return Pane.None;
			}
		}
		
		Pane WorkingItemPane {
			get {
				Item first, second;
				first = GetSelection (Pane.First);
				second = GetSelection (Pane.Second);
				
				if (first != null && second != null) {
					if (first.IsAction () && first.AsAction ().Safe.SupportsItem (second)) {
						return Pane.Second;
					} else if (second.IsAction () && second.AsAction ().Safe.SupportsItem (first)) {
						return Pane.First;
					}
				}
				return Pane.None;
			}
		}
		
		Act WorkingAction {
			get {
				return GetSelection (WorkingActionPane).AsAction ();
			}
		}
		
		Item WorkingItem {
			get {
				return GetSelection (WorkingItemPane);
			}
		}
		
		IEnumerable<Item> WorkingItems {
			get {
				Pane workingPane = WorkingItemPane;
				if (workingPane == Pane.None)
					return null;
				
				return controllers [(int) workingPane].FullSelection;
			}
		}
		
		IEnumerable<Item> WorkingModItems {
			get {
				return controllers [(int) Pane.Third].FullSelection;
			}
		}
		
		/// <summary>
		/// Sets/Unsets third pane visibility if possible.
		/// </summary>
		bool ThirdPaneVisible {
			set {
				if (value == third_pane_visible)
					return;
				
				if (value) window.Grow ();
				else window.Shrink ();
				third_pane_visible = value;
			}
			get {
				return third_pane_visible;
			}
		}
		
		/// <value>
		/// Check if the third pane is capable of closing.  When actions require
		/// the third pane, this will return false.
		/// </value>
		bool ThirdPaneCanClose {
			get {
				return !ThirdPaneRequired &&
					controllers [2].Cursor == 0 && 
					string.IsNullOrEmpty (controllers [2].Query) && 
					!ControllerExplicitTextMode (Pane.Third);
			}
		}
		
		/// <summary>
		/// Determine if the third pane is allowed.  If allowed, tabbing will
		/// result in third pane opening when tabbing from the second pane.
		/// </summary>
		bool ThirdPaneAllowed {
			get {
				Act action = WorkingAction;
				
				return action != null &&
					action.SupportedModifierItemTypes.Any () &&
					controllers [1].Results.Any ();
			}
		}

		/// <value>
		/// Third pane required states that the current controller state requires
		/// that the third pane be visible.
		/// </value>
		bool ThirdPaneRequired {
			get {
				Item item = WorkingItem;
				Act action = WorkingAction;
				
				return action != null && item != null &&
					action.SupportedModifierItemTypes.Any () &&
					!action.ModifierItemsOptional &&
					controllers [1].Results.Any ();
			}
		}
		
		bool AlwaysShowResults {
			get {
				return Do.Preferences.AlwaysShowResults || !window.ResultsCanHide;
			}
		}

		/// <value>
		/// Check if the interface is currently visible.
		/// </value>
		public bool IsSummoned {
			get { return null != window && window.Visible; }
		}

		/// <summary>
		/// Summons a window with elements in it... seems to work
		/// </summary>
		/// <param name="elements">
		/// A <see cref="Item"/>
		/// </param>
		public void SummonWithItems (IEnumerable<Item> elements)
		{
			if (!IsSummonable) return;
			
			Reset ();
			Summon ();
			
			controllers [0].Results = elements.ToList ();
			// If there are multiple results, show results list after a short delay.
			if (1 < elements.Count ()) {
				Services.Application.RunOnMainThread (GrowResults, 250);
			}
		}
		
		/// <summary>
		/// Determines if the user has requested Text Mode explicitly, even if he
		/// has finalized that input
		/// </summary>
		/// <param name="pane">
		/// The <see cref="Pane"/> for which you wish to check
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/>
		/// </returns>
		public bool ControllerExplicitTextMode (Pane pane)
		{
			return controllers [(int) pane].TextType == TextModeType.Explicit ||
				controllers [(int) pane].TextType == TextModeType.ExplicitFinalized;
		}	

#region KeyPress Handling

		private void KeyPressWrap (EventKey evnt)
		{
			Key key = (Key) evnt.KeyValue;
			
			// Currently - only hardcoded are enter keys and delete/backspace
			if (key == Key.Return ||
				   key == Key.ISO_Enter ||
				   key == Key.KP_Enter) {
				OnActivateKeyPressEvent (evnt);
			} else if (key == Key.Delete ||
				   key == Key.BackSpace) {
				OnDeleteKeyPressEvent (evnt);
			} else if (Services.Keybinder.Bindings.Any (k => k.KeyString == Services.Keybinder.KeyEventToString (evnt.KeyValue, (uint)evnt.State))) {
				// User set keybindings
				Services.Keybinder.Bindings.First (k => k.KeyString == Services.Keybinder.KeyEventToString (evnt.KeyValue, (uint)evnt.State)).Callback (evnt);
			} else {
				OnInputKeyPressEvent (evnt);
			}
		}
		
		void OnPasteEvent (EventKey evnt)
		{
			Gtk.Clipboard clip = Gtk.Clipboard.Get (Selection.Clipboard);
			if (!clip.WaitIsTextAvailable ()) {
				return;
			}
			string str = clip.WaitForText ();
			SearchController.SetString (SearchController.Query + str);
		}
		
		void OnCopyEvent (EventKey evnt)
		{
			if (SearchController.Selection is Item)
				Services.Environment.CopyToClipboard (SearchController.Selection);
		}
		
		void OnActivateKeyPressEvent (EventKey evnt)
		{
			im_context.Reset ();
			if (SearchController.TextType == TextModeType.Explicit) {
				OnInputKeyPressEvent (evnt);
				return;
			}
			bool shift_pressed = (evnt.State & ModifierType.ShiftMask) != 0;
			PerformAction (!shift_pressed);
		}
		
		/// <summary>
		/// This will set a secondary cursor unless we are operating on a text
		/// item, in which case we pass the event to the input key handler.
		/// </summary>
		/// <param name="evnt">
		/// A <see cref="EventKey"/>
		/// </param>
		void OnSelectionKeyPressEvent (EventKey evnt)
		{
			im_context.Reset ();
			if (SearchController.Selection is ITextItem || !results_grown)
				OnInputKeyPressEvent (evnt);
			else if (SearchController.ToggleSecondaryCursor (SearchController.Cursor))
				UpdatePane (CurrentPane);
		}
		
		void OnDeleteKeyPressEvent (EventKey evnt)
		{
			im_context.Reset ();
			SearchController.DeleteChar ();
		}
		
		void OnSummonKeyPressEvent (EventKey evnt)
		{
			if (IsSummoned) {
				Reset ();
				Vanish ();
			} else {
				Summon ();
			}
		}
		
		void OnTextModeSummonKeyPressEvent (EventKey evnt)
		{
			Summon (); 
			SearchController.TextMode = true; 
			UpdatePane (CurrentPane);
		}
		
		void OnClearKeyPressEvent (EventKey evnt)
		{
			im_context.Reset ();
			if (SearchController.TextType == TextModeType.Explicit) {
				if (SearchController.Query.Length > 0)
					SearchController.FinalizeTextMode ();
				else 
					SearchController.TextMode = false;
				UpdatePane (CurrentPane);
				return;
			}
			
			bool results, something_typed;

			something_typed = SearchController.Query.Length > 0;
			results = SearchController.Results.Any ();
			
			ClearSearchResults ();
			
			ShrinkResults ();
			
			if (CurrentPane == Pane.First && !results) Vanish ();
			else if (!something_typed) Reset ();
			
			if (GetSelection (Pane.Third) == null || !ThirdPaneAllowed) {
				ThirdPaneVisible = false;
			}
		}
		
		void OnInputKeyPressEvent (EventKey evnt)
		{
			if (im_context.FilterKeypress (evnt) || ((evnt.State & ModifierType.ControlMask) != 0))
				return;
			
			char c;
			if (evnt.Key == Key.Return) {
				c = '\n';
			} else {
				c = (char) Keyval.ToUnicode (evnt.KeyValue);
			}
			if (char.IsLetterOrDigit (c)
					|| char.IsPunctuation (c)
					|| c == '\n'
					|| (c == ' ' && SearchController.Query.Length > 0)
					|| char.IsSymbol (c)) {
				SearchController.AddChar (c);
			}
		}
		
		void OnStepOutItemPressEvent (EventKey evnt)
		{
			im_context.Reset ();
			if (!SearchController.Results.Any ()) return;

			// We're attempting to browse the parent of an item, so decrease its
			// relevance. This makes it so we can merely visit an item's children,
			// and navigate back out of the item, and leave that item's relevance
			// unchanged.
			SearchController.Selection
				.DecreaseRelevance (SearchController.Query, null);
			if (SearchController.ItemParentSearch ()) GrowResults ();
		}

		// Hmm.
		void OnStepInItemPressEvent (EventKey evnt)
		{
			im_context.Reset ();
			if (!SearchController.Results.Any ()) return;

			// We're attempting to browse the contents of an item, so increase its
			// relevance.
			SearchController.Selection.IncreaseRelevance (SearchController.Query, null);
			if (SearchController.ItemChildSearch ()) GrowResults ();
		}

		void OnNextPanePressEvent (EventKey evnt)
		{
			im_context.Reset ();
			ShrinkResults ();

			if (SearchController.TextType == TextModeType.Explicit) {
				SearchController.FinalizeTextMode ();
			}
				
			NextPane ();
			UpdatePane (CurrentPane);
		}

		void OnPreviousPanePressEvent (EventKey evnt)
		{
			im_context.Reset ();
			ShrinkResults ();

			if (SearchController.TextType == TextModeType.Explicit) {
				SearchController.FinalizeTextMode ();
			}
			PrevPane ();
			UpdatePane (CurrentPane);
		}
			
		
		void OnTextModePressEvent (EventKey evnt)
		{
			im_context.Reset ();

			// If this isn't the first keypress in text mode (we just entered text
			// mode) or if we're already in text mode, treat keypress as normal
			// input.
			if (0 < SearchController.Query.Length || SearchController.TextType == TextModeType.Explicit) {
				OnInputKeyPressEvent (evnt);
			} else {
				SearchController.TextMode = true;
			}
			UpdatePane (CurrentPane);
		}
		
		void OnPreviousItemPressEvent (EventKey evnt)
		{
			im_context.Reset ();
			if (!results_grown) {
				if (SearchController.Cursor > 0)
					GrowResults ();
				return;
			} else {
				if (SearchController.Cursor <= 0) {
					ShrinkResults ();
					return;
				}
				SearchController.Cursor--;
			}
		}

		void OnNextItemPressEvent (EventKey evnt)
		{
			im_context.Reset ();
			if (!results_grown) {
				GrowResults ();
				return;
			}
			SearchController.Cursor++;
		}
		
		void OnFirstItemPressEvent (EventKey evnt)
		{
			im_context.Reset ();
			SearchController.Cursor = 0;
		}

		void OnLastItemPressEvent (EventKey evnt)
		{
			im_context.Reset ();
			SearchController.Cursor = SearchController.Results.Count - 1;
		}

		void OnNextItemPagePressEvent (EventKey evnt)
		{
			im_context.Reset ();
			SearchController.Cursor -= 5;
		}
		
		void OnPreviousItemPagePressEvent (EventKey evnt)
		{
			im_context.Reset ();
			SearchController.Cursor += 5;
		}
#endregion
		
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
		
		/// <summary>
		/// This method determines what to do when a search is completed and takes the appropriate action
		/// </summary>
		/// <param name="o">
		/// A <see cref="System.Item"/>
		/// </param>
		/// <param name="state">
		/// A <see cref="SearchFinishState"/>
		/// </param>
		/// <param name="pane">
		/// A <see cref="Pane"/>
		/// </param>
		void SearchFinished (object o, SearchFinishState state, Pane pane)
		{
			if (pane == Pane.First && FirstControllerIsReset) {
				Reset ();
				return;
			}
			
			if (state.QueryChanged || state.SelectionChanged)
				SmartUpdatePane (pane);
		}

		/// <summary>
		/// Intended to wrap UpdatePane with smart conditionals to avoid excess/unexpected updates
		/// </summary>
		/// <param name="pane">
		/// A <see cref="Pane"/>
		/// </param>
		void SmartUpdatePane (Pane pane)
		{
			if (!FirstControllerIsReset)
				UpdatePane (pane);
		}
		
		protected void ClearSearchResults ()
		{
			switch (CurrentPane) {
			case Pane.First:
				Reset ();
				break;
			case Pane.Second:
				controllers [1].Reset ();
				controllers [2].Reset ();
				break;
			case Pane.Third:
				controllers [2].Reset ();
				break;
			}
		}
		
		protected void UpdatePane (Pane pane)
		{
			if (!window.Visible) return;
			
			//Lets see if we need to play with Third pane visibility
			if (pane == Pane.Third) {
				if (ThirdPaneRequired) {
					ThirdPaneVisible = true;
				} else if (CurrentPane != Pane.Third && (!ThirdPaneAllowed || ThirdPaneCanClose)) {
					ThirdPaneVisible = false;
				}
			}
			window.SetPaneContext (pane, controllers [(int) pane].UIContext);
		}
		
		/// <summary>
		/// Call to fully reset Do.  This should return Do to its starting state
		/// </summary>
		void Reset ()
		{
			ThirdPaneVisible = false;
			
			foreach (ISearchController controller in controllers)
				controller.Reset ();
			
			CurrentPane = Pane.First;
			
			ShrinkResults ();
			window.Reset ();
		}
		
		/// <summary>
		/// Should cause UI to display more results
		/// </summary>
		void GrowResults ()
		{
			UpdatePane (CurrentPane);
			window.GrowResults ();
			results_grown = true;	
		}
		
		/// <summary>
		/// Should cause UI to display fewer results, 0 == no results displayed
		/// </summary>
		void ShrinkResults ()
		{
			if (AlwaysShowResults) return;
			window.ShrinkResults ();
			results_grown = false;
		}
		
		Item GetSelection (Pane pane)
		{
			Item o;

			try {
				o = controllers [(int) pane].Selection;
			} catch {
				o = null;
			}
			return o;
		}
		
		void PerformAction (bool vanish)
		{
			Act action;
			string actionQuery, itemQuery, modItemQuery;
			IEnumerable<Item> items, modItems;

			if (vanish) Vanish ();
			// Flush main thread queue to get Vanish to complete.
			Services.Application.FlushMainThreadQueue ();

			action = WorkingAction;
			items = WorkingItems;
			modItems = ThirdPaneVisible ? WorkingModItems : Enumerable.Empty<Item> ();
			
			// If the current state of the controller is invalid, warn and return
			// early.
			if (items == null || action == null) {
				Log<Controller>
					.Warn ("Controller state was not valid, so the action could not be performed.");
				if (vanish) Reset ();
				return;
			}
			
			actionQuery  = controllers [(int) WorkingActionPane].Query;
			itemQuery    = controllers [(int) WorkingItemPane].Query;
			modItemQuery = controllers [(int) Pane.Third].Query;
			
			if (ThirdPaneVisible)
				modItemQuery = controllers [(int) Pane.Third].Query;

			/////////////////////////////////////////////////////////////
			/// Relevance accounting
			/////////////////////////////////////////////////////////////
			
			if (WorkingActionPane == Pane.Second) {
				// Act is in second pane.
				// Increase the relevance of the items.
				foreach (Item item in items)
					item.IncreaseRelevance (itemQuery, null);

				// Increase the relevance of the action /for each item/:
				if (action != null) {
					foreach (Item item in items)
						action.IncreaseRelevance (actionQuery, item);
				} else {
					Log<Controller>.Debug ("Action is null in 2nd pane!");
				}
			} else {
				// Act is in first pane.
				// Increase the relevance of each item for the action.
				foreach (Item item in items)
					item.IncreaseRelevance (itemQuery, action);
				if (action != null)
					action.IncreaseRelevance (actionQuery, null);
				else
					Log<Controller>.Debug ("Action is null in 1st pane!");
			}

			if (ThirdPaneVisible) {
				foreach (Item item in modItems)
					item.IncreaseRelevance (modItemQuery, action);
			}

			if (vanish) Reset ();

			// Finally, we can perform the action.
			PerformAction (action, items, modItems);
		}

		void PerformAction (Act action, IEnumerable<Item> items, IEnumerable<Item> modItems)
		{
			if (action == null)   throw new ArgumentNullException ("action");
			if (items == null)    throw new ArgumentNullException ("items");
			if (modItems == null) throw new ArgumentNullException ("modItems");

			IEnumerable<Item> results = action.Safe.Perform (items, modItems);
			if (results.Any ()) {
				SummonWithItems (results);
			}
		}

		#region IController Implementation
		public void Summon ()
		{
			if (!IsSummonable) return;
			OnSummoned ();
			
			window.Summon ();
			if (AlwaysShowResults) GrowResults ();
			im_context.FocusIn ();
		}
		
		public void Vanish ()
		{
			window.ShrinkResults ();
			results_grown = false;
			window.Vanish ();
		}

		public void ShowPreferences ()
		{
			Vanish ();
			Reset ();

			if (PreferencesWindow == null) {
				PreferencesWindow = new PreferencesWindow ();
				PreferencesWindow.Hidden += delegate {
					// Release the window.
					PreferencesWindow.Dispose ();
					PreferencesWindow.Destroy ();
					PreferencesWindow = null;
					// Reload universe.
					Do.UniverseManager.Reload ();
				};
			}
			if (Gtk.Grab.Current != null)
				Gtk.Grab.Remove (Gtk.Grab.Current);	
			PreferencesWindow.Show ();
		}

		public void ShowAbout ()
		{
			if (AboutDialog != null) {
				AboutDialog.GdkWindow.Raise ();
				return;
			}
			
			string logo;

			Vanish ();
			Reset ();

			AboutDialog = new Gtk.AboutDialog ();
			AboutDialog.ProgramName = Catalog.GetString ("GNOME Do");
			AboutDialog.Modal = false;

			AboutDialog.Version = AssemblyInfo.DisplayVersion + "\n" + AssemblyInfo.VersionDetails;

			logo = "gnome-do.svg";

			AboutDialog.Logo = IconProvider.PixbufFromIconName (logo, 140);
			AboutDialog.Copyright = Catalog.GetString ("Copyright \xa9 2014 GNOME Do Developers");
			AboutDialog.Comments = Catalog.GetString ("Do things as quickly as possible\n" +
				"(but no quicker) with your files, bookmarks,\n" +
				"applications, music, contacts, and more!");
			AboutDialog.Website = "http://do.cooperteam.net/";
			AboutDialog.WebsiteLabel = Catalog.GetString ("Visit Homepage");
			Gtk.AboutDialog.SetUrlHook((dialog, link) => Services.Environment.OpenUrl (link));
			AboutDialog.IconName = "gnome-do";

			if (AboutDialog.Screen.RgbaColormap != null)
				Gtk.Widget.DefaultColormap = AboutDialog.Screen.RgbaColormap;
			
			AboutDialog.Response += delegate {
				AboutDialog.Hide ();
			};
			
			AboutDialog.Hidden += delegate {
				AboutDialog.Destroy ();
				AboutDialog = null;
			};
			
			AboutDialog.Show ();
		}
		#endregion
		
		#region IDoController implementation
		
		public void NewContextSelection (Pane pane, int index)
		{
			if (!controllers [(int) pane].Results.Any () || index == controllers [(int) pane].Cursor) return;
			
			controllers [(int) pane].Cursor = index;
			window.SetPaneContext (pane, controllers [(int) pane].UIContext);
		}

		public void ButtonPressOffWindow ()
		{
			Vanish ();
			Reset ();
		}
		
		public ControlOrientation Orientation { get; set; }
		
		public bool ItemHasChildren (Item item)
		{
			return item.HasChildren ();
		}
		
		#endregion
		
		public void PerformDefaultAction (Item item, IEnumerable<Type> filter) 
		{
			Act action =
				Do.UniverseManager.Search ("", typeof (Act).Cons (filter), item)
					.OfType<Act> ()
					.Where (act => act.Safe.SupportsItem (item))
					.FirstOrDefault ();

			if (action == null) return;
			
			item.IncreaseRelevance ("", null);
			PerformActionOnItem (action, item);
		}

		public void PerformActionOnItem (Act action, Item item)
		{
			PerformAction (action, new [] { item }, new Item [0]);
		}

	}
		
}
