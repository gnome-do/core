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
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Threading;

using Gdk;
using Mono.Unix;

using Do;
using Do.UI;
using Do.Addins;
using Do.Universe;
using Do.DBusLib;

namespace Do.Core {

	public class Controller : IController, IDoController {
		
		struct DoPerformState {
			public List<IItem> Items, ModItems;
			public IAction Action;
			
			public DoPerformState (IAction action, List<IItem> items, List<IItem> moditems) {
				Items = items;
				ModItems = moditems;
				Action = action;
			}
		}

		protected IDoWindow window;
		protected Gtk.Window addin_window;
		protected Gtk.AboutDialog about_window;
		protected PreferencesWindow prefs_window;
		protected ISearchController[] controllers;
		protected Thread th;
		
		const int SearchDelay = 250;
		
		IAction action;
		List<IItem> items;
		List<IItem> modItems;
		bool thirdPaneVisible;
		bool resultsGrown;
		Gtk.IMContext im;
		
		public Controller ()
		{
			im = new Gtk.IMMulticontext ();
			items = new List<IItem> ();
			modItems = new List<IItem> ();
			resultsGrown = false;
			
			controllers = new SimpleSearchController[3];
			
			// Each controller needs to be aware of the controllers before it.
			// Going down the line however is not needed at current.
			controllers[0] = new FirstSearchController  ();
			controllers[1] = new SecondSearchController (controllers[0]);
			controllers[2] = new ThirdSearchController  (controllers[0], controllers[1]);
			
			// We want to show a blank box during our searches
			// controllers[0].SearchStarted += (u) => { };
			controllers[1].SearchStarted += (u) => {
				if (u && !ControllerExplicitTextMode (Pane.Second))
					window.ClearPane (Pane.Second); 
			};

			controllers[2].SearchStarted += (u) => { 
				if (u && !ControllerExplicitTextMode (Pane.Third))
					window.ClearPane (Pane.Third); 
			};
			
			controllers[0].SearchFinished += (o, state) => SearchFinished (o, state, Pane.First);
			controllers[1].SearchFinished += (o, state) => SearchFinished (o, state, Pane.Second);
			controllers[2].SearchFinished += (o, state) => SearchFinished (o, state, Pane.Third);
			
			im.UsePreedit = false;
			im.Commit += OnIMCommit;
			im.FocusIn ();
		}
		
		private void OnIMCommit (object o, Gtk.CommitArgs args)
		{
			foreach (char c in args.Str.ToCharArray ())
				SearchController.AddChar (c);
			
			//Horrible hack:
			//The reason this exists and exists here is to update the clipboard in a place that
			//we know will always be safe for GTK.  Unfortunately due to the way we have designed
			//Do, this has proven extremely difficult to put some place more logical.  We NEED to
			//rethink how we handle Summon () and audit our usage of Gdk.Threads.Enter ()
			if (SearchController.Query.Length <= 1)
				SelectedTextItem.UpdateText ();
		}
		
		public void Initialize ()
		{
			ThemeChanged ();
			Do.Preferences.PreferenceChanged += (sender, args) => { if (args.Key == "Theme") ThemeChanged (); };
		}
		
		void ThemeChanged ()
		{
			if (null != window) Vanish ();
			
			if (window != null)
				window.KeyPressEvent -= KeyPressWrap;
			if (window is Gtk.Widget)
				(window as Gtk.Widget).Destroy ();
			
			window = null;
			
			if (!Gdk.Screen.Default.IsComposited) {
				window = new ClassicWindow (this);
				window.KeyPressEvent += KeyPressWrap;
				Reset ();
				return;
			}

			window = PluginManager.GetThemes ()
				.Where (theme => theme.Name == Do.Preferences.Theme)
				.Select (theme => new Bezel (this, theme))
				.FirstOrDefault ();

			if (window == null)
				window = new Bezel (this, new ClassicTheme ());
			
			if (window is Gtk.Window)
				(window as Gtk.Window).Title = "Do";
			
			// Get key press events from window since we want to control that here.
			window.KeyPressEvent += KeyPressWrap;
			Reset ();
		}

		bool IsSummonable {
			get {
				return prefs_window == null && about_window == null;
			}
		}

		/// <value>
		/// Convenience Method
		/// </value>
		ISearchController SearchController {
			get {
				return controllers[(int) CurrentPane];
			}
		}
		
		/// <value>
		/// The currently active pane, setting does not imply searching
		/// currently
		/// </value>
		public Pane CurrentPane {
			set {
				//If we have no results, we can't go to the second pane
				if (window.CurrentPane == Pane.First &&
					SearchController.Results.Length == 0)
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
			
			get {
				return window.CurrentPane;
			}
		}
		
		bool FirstControllerIsReset {
			get {
				return (string.IsNullOrEmpty(controllers[0].Query) && controllers[0].Results.Length == 0);
			}
		}
		
		bool ThirdPaneVisible {
			set {
				if (value == thirdPaneVisible)
					return;
				
				if (value)
					window.Grow ();
				else
					window.Shrink ();
				thirdPaneVisible = value;
			}
			
			get {
				return thirdPaneVisible;
			}
		}
		
		bool ThirdPaneCanClose {
			get {
				return (!ThirdPaneRequired &&
				        controllers[2].Cursor == 0 && 
				        string.IsNullOrEmpty (controllers[2].Query) && 
				        !ControllerExplicitTextMode (Pane.Third));
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
					controllers[1].Results.Length > 0;
			}
		}

		bool ThirdPaneRequired {
			get {
				IObject first, second;
				IAction action;
				IItem item;

				first = GetSelection (Pane.First);
				second = GetSelection (Pane.Second);
				action = (first as IAction) ?? (second as IAction);
				item = (first as IItem) ?? (second as IItem);
				return action != null && item != null &&
					action.SupportedModifierItemTypes.Length > 0 &&
					!action.ModifierItemsOptional &&
					controllers[1].Results.Length > 0;
			}
		}

		public bool IsSummoned {
			get {
				return null != window && window.Visible;
			}
		}

		internal PreferencesWindow PreferencesWindow {
			get { return prefs_window; }
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
			
			Summon ();
			
			//Someone is going to need to explain this to me -- Now with less stupid!
			controllers[0].Results = objects;

			// If there are multiple results, show results window after a short
			// delay.
			if (objects.Length > 1) {
				GLib.Timeout.Add (50,
					delegate {
//						Gdk.Threads.Enter ();
						GrowResults ();
//						Gdk.Threads.Leave ();
						return false;
					}
				);
			}
		}
		
		public bool ControllerExplicitTextMode (Pane pane) {
			return controllers[(int) pane].TextType == TextModeType.Explicit ||
				controllers[(int) pane].TextType == TextModeType.ExplicitFinalized;
		}
		
		/////////////////////////
		/// Key Handling ////////
		/////////////////////////

		private void KeyPressWrap (Gdk.EventKey evnt)
		{
			// User set keybindings
			if (KeyEventToString (evnt).Equals (Do.Preferences.SummonKeyBinding)) {
				OnSummonKeyPressEvent (evnt);
				return;
			} 
			
			if (KeyEventToString (evnt).Equals (Do.Preferences.TextModeKeyBinding)) {
				OnTextModePressEvent (evnt);
				return;
			}
			
			// Check for paste
			if ((evnt.State & ModifierType.ControlMask) != 0) {
				if (evnt.Key == Key.v) {
					OnPasteEvent ();
					return;
				}
				if (evnt.Key == Key.c) {
					OnCopyEvent ();
					return;
				}
			}

			switch ((Gdk.Key) evnt.KeyValue) {
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
			case Gdk.Key.Page_Up:
			case Gdk.Key.Page_Down:
				OnUpDownKeyPressEvent (evnt);
				break;
			case Gdk.Key.Right:
			case Gdk.Key.Left:
				OnRightLeftKeyPressEvent (evnt);
				break;
			case Gdk.Key.comma:
				OnSelectionKeyPressEvent (evnt);
				break;
			default:
				OnInputKeyPressEvent (evnt);
				break;
			}
			return;
		}
		
		void OnPasteEvent ()
		{
			Gtk.Clipboard clip = Gtk.Clipboard.Get (Gdk.Selection.Clipboard);
			if (!clip.WaitIsTextAvailable ()) {
				return;
			}
			string str = clip.WaitForText ();
			SearchController.SetString (SearchController.Query + str);
		}
		
		void OnCopyEvent ()
		{
			Gtk.Clipboard clip = Gtk.Clipboard.Get (Gdk.Selection.Clipboard);
			if (SearchController.Selection != null)
				clip.Text = SearchController.Selection.Name;
		}
		
		void OnActivateKeyPressEvent (EventKey evnt)
		{
			im.Reset ();
			if (SearchController.TextType == TextModeType.Explicit) {
				OnInputKeyPressEvent (evnt);
				return;
			}
			bool shift_pressed = (evnt.State & ModifierType.ShiftMask) != 0;
			PerformAction (!shift_pressed);
		}
		
		/// <summary>
		/// This will set a secondary cursor unless we are operating on a text item, in which case we
		/// pass the event to the input key handler
		/// </summary>
		/// <param name="evnt">
		/// A <see cref="EventKey"/>
		/// </param>
		void OnSelectionKeyPressEvent (EventKey evnt)
		{
			im.Reset ();
			if (SearchController.Selection is ITextItem || !resultsGrown)
				OnInputKeyPressEvent (evnt);
			else if (SearchController.ToggleSecondaryCursor (SearchController.Cursor))
				UpdatePane (CurrentPane);
		}
		
		void OnDeleteKeyPressEvent (EventKey evnt)
		{
			im.Reset ();
			SearchController.DeleteChar ();
		}
		
		void OnSummonKeyPressEvent (EventKey evnt)
		{
			Reset ();
			Vanish ();
		}
		
		void OnEscapeKeyPressEvent (EventKey evnt)
		{
			im.Reset ();
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
			results = SearchController.Results.Length > 0;
			
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
			if (im.FilterKeypress (evnt) || ((evnt.State & ModifierType.ControlMask) != 0))
				return;
//			im.Reset ();
			char c;
			if (evnt.Key == Key.Return) {
				c = '\n';
			} else {
				c = (char) Gdk.Keyval.ToUnicode (evnt.KeyValue);
			}
			if (char.IsLetterOrDigit (c)
					|| char.IsPunctuation (c)
					|| c == '\n'
					|| (c == ' ' && SearchController.Query.Length > 0)
					|| char.IsSymbol (c)) {
				SearchController.AddChar (c);
			}
		}
		
		void OnRightLeftKeyPressEvent (EventKey evnt)
		{
			im.Reset ();
			if (SearchController.Results.Length == 0) return;

			switch ((Gdk.Key) evnt.KeyValue) {
			case Gdk.Key.Right:
				// We're attempting to browse the contents of an item, so increase its
				// relevance.
				(SearchController.Selection as DoObject)
					.IncreaseRelevance (SearchController.Query, null);
				if (SearchController.ItemChildSearch ()) GrowResults ();
				break;
			case Gdk.Key.Left:
				// We're attempting to browse the parent of an item, so decrease its
				// relevance. This makes it so we can merely visit an item's children,
				// and navigate back out of the item, and leave that item's relevance
				// unchanged.
				(SearchController.Selection as DoObject)
					.DecreaseRelevance (SearchController.Query, null);
				if (SearchController.ItemParentSearch ()) GrowResults ();
				break;
			}
		}
		
		void OnTabKeyPressEvent (EventKey evnt)
		{
			im.Reset ();
			ShrinkResults ();

			if (SearchController.TextType == TextModeType.Explicit) {
				SearchController.FinalizeTextMode ();
				UpdatePane (CurrentPane);
			}
				
			if (evnt.Key == Key.Tab) {
				NextPane ();
			} else if (evnt.Key == Key.ISO_Left_Tab) {
				PrevPane ();
			}
		}
		
		void OnTextModePressEvent (EventKey evnt)
		{
			im.Reset ();

			// If this isn't the first keypress in text mode (we just entered text
			// mode) or if we're already in text mode, treat keypress as normal
			// input.
			bool pass_keypress = (0 < SearchController.Query.Length || SearchController.TextType == TextModeType.Explicit);
			SearchController.TextMode = true;
			
			if (pass_keypress || SearchController.TextMode == false)
				OnInputKeyPressEvent (evnt);
			UpdatePane (CurrentPane);
		}
		
		void OnUpDownKeyPressEvent (EventKey evnt)
		{
			im.Reset ();
			if (evnt.Key == Gdk.Key.Up) {
				if (!resultsGrown) {
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
			} else if (evnt.Key == Gdk.Key.Down) {
				if (!resultsGrown) {
					GrowResults ();
					return;
				}
				SearchController.Cursor++;
			} else if (evnt.Key == Gdk.Key.Home) {
				SearchController.Cursor = 0;
			} else if (evnt.Key == Gdk.Key.End) {
				SearchController.Cursor = SearchController.Results.Length - 1;
			} else if (evnt.Key == Gdk.Key.Page_Down) {
				SearchController.Cursor += 5;
			} else if (evnt.Key == Gdk.Key.Page_Up) {
				SearchController.Cursor -= 5;
			}
		}
		
		/// <summary>
		/// Converts a keypress into a human readable string for comparing
		/// against values in GConf.
		/// </summary>
		/// <param name="evnt">
		/// A <see cref="EventKey"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.String"/> in the form "<Modifier>key"
		/// </returns>
		string KeyEventToString (EventKey evnt) {
			string modifier = string.Empty;
			if ((evnt.State & ModifierType.ControlMask) != 0) {
				modifier += "<Control>";
			}
			if ((evnt.State & ModifierType.SuperMask) != 0) {
				modifier += "<Super>";
			}
			if ((evnt.State & ModifierType.Mod1Mask) != 0) {
				modifier += "<Alt>";
			}
			return modifier + evnt.Key.ToString ();
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
				controllers[1].Reset ();
				controllers[2].Reset ();
				break;
			case Pane.Third:
				controllers[2].Reset ();
				break;
			}
		}
		
		/////////////////////////
		// Pane Update Methods //
		/////////////////////////

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
			window.SetPaneContext (pane, controllers[(int) pane].UIContext);
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
			resultsGrown = true;	
		}
		
		/// <summary>
		/// Should cause UI to display fewer results, 0 == no results displayed
		/// </summary>
		void ShrinkResults ()
		{
			if (Do.Preferences.AlwaysShowResults) return;
			window.ShrinkResults ();
			resultsGrown = false;
		}
		
		IObject GetSelection (Pane pane)
		{
			IObject o;

			try {
				o = controllers[(int) pane].Selection;
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

			first  = GetSelection (Pane.First);
			second = GetSelection (Pane.Second);
			third  = GetSelection (Pane.Third);

			if (first != null && second != null) {
				if (first is IItem) {
					foreach (IItem item in controllers[0].FullSelection)
						items.Add (item);
					action = second as IAction;
					itemQuery = controllers[0].Query;
					actionQuery = controllers[1].Query;
				} else {
					foreach (IItem item in controllers[1].FullSelection)
						items.Add (item);
					action = first as IAction;
					itemQuery = controllers[1].Query;
					actionQuery = controllers[0].Query;
				}
				if (third != null && ThirdPaneVisible) {
					foreach (IItem item in controllers[2].FullSelection)
						modItems.Add (item);
					modItemQuery = controllers[2].Query;
					(third as DoObject).IncreaseRelevance (modItemQuery, null);
				}

				/////////////////////////////////////////////////////////////
				/// Relevance accounting
				/////////////////////////////////////////////////////////////
				
				// Increase the relevance of the item.
				foreach (DoObject item in items) {
					item.IncreaseRelevance (itemQuery, null);
				}

				// Increase the relevance of the action alone:
				(action as DoAction).IncreaseRelevance (actionQuery, null);
				// Increase the relevance of the action /for each item/:
				foreach (DoObject item in items)
					(action as DoObject).IncreaseRelevance (actionQuery, item);

				DoPerformState state = new DoPerformState (action, items, modItems);
				th = new Thread (new ParameterizedThreadStart (DoPerformWork));
				th.Start (state);
				th.Join (100);
			}

			if (vanish) {
				Reset ();
			}
		}
				
		private void DoPerformWork (object o)
		{
			DoPerformState state = (DoPerformState) o;
			state.Action.Perform (state.Items.ToArray (), state.ModItems.ToArray ());
		}
					

		///////////////////////////
		/// IController Members ///
		///////////////////////////
		
		public void Summon ()
		{
			if (!IsSummonable) return;

			if (th != null && th.IsAlive) {
				Thread.Sleep (100);
			}
			
			if (th != null && th.IsAlive) {
				NotificationIcon.ShowKillNotification ((o, a) => System.Environment.Exit (20));
				return;
			}
			
			window.Summon ();
			if (Do.Preferences.AlwaysShowResults)
				GrowResults ();
			im.FocusIn ();
		}
		
		public void Vanish ()
		{
			window.ShrinkResults ();
			resultsGrown = false;
			window.Vanish ();
		}	

		public void ShowPreferences ()
		{
			Vanish ();
			Reset ();

			if (null == prefs_window) {
				prefs_window = new PreferencesWindow ();
				prefs_window.Destroyed += delegate {
					Do.UniverseManager.Reload ();
					prefs_window = null;
				};
			}
			prefs_window.Show ();
		}

		public void ShowAbout ()
		{
			string logo;

			Vanish ();
			Reset ();

			about_window = new Gtk.AboutDialog ();
			about_window.ProgramName = "GNOME Do";
			about_window.Modal = false;

			try {
				Assembly asm = Assembly.GetEntryAssembly ();
				ProgramVersion ver = asm.GetCustomAttributes (typeof (ProgramVersion), false)[0] as ProgramVersion;
				about_window.Version = ver.Version + "\n" + ver.Details;
			} catch {
				about_window.Version = Catalog.GetString ("Unknown");
			}

			logo = "gnome-do.svg";

			about_window.Logo = UI.IconProvider.PixbufFromIconName (logo, 140);
			about_window.Copyright = "Copyright \xa9 2008 GNOME Do Developers";
			about_window.Comments = "Do things as quickly as possible\n" +
				"(but no quicker) with your files, bookmarks,\n" +
				"applications, music, contacts, and more!";
			about_window.Website = "http://do.davebsd.com/";
			about_window.WebsiteLabel = "Visit Homepage";
			about_window.IconName = "gnome-do";

			if (null != about_window.Screen.RgbaColormap)
				Gtk.Widget.DefaultColormap = about_window.Screen.RgbaColormap;

			about_window.Run ();
			about_window.Destroy ();
			about_window = null;
		}
		
#region IDoController
		public void NewContextSelection (Pane pane, int index)
		{
			if (controllers[(int) pane].Results.Length == 0 || index == controllers[(int) pane].Cursor) return;
			
			controllers[(int) pane].Cursor = index;
			window.SetPaneContext (pane, controllers[(int) pane].UIContext);
		}

		public void ButtonPressOffWindow ()
		{
			Vanish ();
			Reset ();
		}
		
		public bool ObjectHasChildren (IObject o)
		{
			return Do.UniverseManager.ObjectHasChildren (o);
		}
#endregion
	}
}
