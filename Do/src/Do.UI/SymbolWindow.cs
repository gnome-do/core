/* SymbolWindow.cs
 *
 * GNOME Do is the legal property of its developers. Please refer to the
 * COPYRIGHT file distributed with this source distribution.
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
using System.Collections.Generic;

using Gtk;
using Gdk;

using Do.Universe;
using Do.Addins;

// Do.Core dependency needs to be removed
using Do.Core;

namespace Do.UI
{
	public class SymbolWindow : Gtk.Window
	{
		class DefaultIconBoxObject : IObject
		{
			public string Icon { get { return "gtk-find"; } }
			public string Name { get { return ""; } }
			public string Description { get { return ""; } }
		}

		class NoResultsFoundObject : IObject
		{
			string query;

			public NoResultsFoundObject (string query)
			{
				this.query = query;
			}

			public string Icon { get { return "gtk-dialog-question"; } }
			public string Name { get { return "No results found."; } }

			public string Description
			{
				get {
					return string.Format ("No results found for \"{0}\".", query);
				}
			}
		}

		const int IconBoxIconSize = 128;
		const int IconBoxPadding = 6;
		const int IconBoxRadius = 20;
		const double WindowTransparency = 0.91;

		protected enum Pane {
			First = 0,
			Second = 1,
			Third = 2,
		}

		RoundedFrame frame;
		SymbolDisplayLabel label;
		ResultsWindow resultsWindow;
		HBox resultsHBox;
		IconBox[] iconbox;

		protected Pane currentPane;
		protected SearchContext[] context;

		ICommand command;
		List<IItem> items;
		List<IItem> modItems;

		bool tabbing;

		public SymbolWindow () : base (Gtk.WindowType.Toplevel)
		{
			Build ();
			items = new List<IItem> ();
			modItems = new List<IItem> ();
			context = new SearchContext[3];
			SetDefaultState ();
		}

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

		Pane CurrentPane
		{
			get {
				return currentPane;
			}
			set {
				currentPane = value;
				SetPane (currentPane);
			}
		}

		SearchContext CurrentContext
		{
			get {
				return context[(int) currentPane];
			}
			set {
				context[(int) currentPane] = value;
			}
		}

		IconBox CurrentIconBox { get { return iconbox[(int) currentPane]; } }

		int CurrentCursor
		{
			get {
				return context[(int) currentPane].Cursor;
			}
			set {
				context[(int) currentPane].Cursor = value;
			}
		}

		protected virtual void SetDefaultState ()
		{
			tabbing = false;

			context[0] = new SearchContext ();
			context[1] = new SearchContext ();
			context[2] = new SearchContext ();
			context[0].SearchTypes = new Type[] { typeof (ICommand), typeof (IItem) };

			CurrentPane = Pane.First;
			iconbox[0].DisplayObject = new DefaultIconBoxObject ();
			iconbox[1].Clear ();

			label.SetDisplayLabel ("Type to begin searching", "Type to start searching.");
		}

		protected virtual void SetNoResultsFoundState (Pane pane)
		{
			NoResultsFoundObject none_found;

			if (currentPane == Pane.First) {
				iconbox[1].Clear ();
			}
			iconbox[2].Clear ();

			none_found = new NoResultsFoundObject (context[(int) pane].Query);
			iconbox[(int) pane].DisplayObject = none_found;
			if (currentPane == pane) {
				label.SetDisplayLabel ("", none_found.Description);
				resultsWindow.Results = new IObject[0];
			}
		}

		protected void ClearSearchResults ()
		{
			switch (currentPane) {
				case Pane.First:
					SetDefaultState ();
					break;
				case Pane.Second:
					context[1] = new SearchContext ();
					context[2] = new SearchContext ();
					SearchSecondPane ("");
					break;
			}
		}

		protected override bool OnButtonPressEvent (EventButton evnt)
		{
			int start_x, start_y, end_x, end_y;
			int click_x, click_y;
			bool click_on_window, click_near_settings_icon;

			GetPosition (out start_x, out start_y);
			GetSize (out end_x, out end_y);
			end_x += start_x;
			end_y += start_y;
			click_x = (int) evnt.XRoot;
			click_y = (int) evnt.YRoot;
			click_on_window = start_x <= click_x && click_x < end_x &&
			                  start_y <= click_y && click_y < end_y;
			click_near_settings_icon = (end_x - 27) <= click_x && click_x < end_x &&
			                            start_y <= click_y && click_y < (start_y + 27);
			if (click_near_settings_icon) {
				Addins.Util.Appearance.PopupMainMenuAtPosition (end_x - 21, start_y + 16);
				// Have to re-grab the pane from the menu.
				Addins.Util.Appearance.PresentWindow (this);
			} else if (!click_on_window) {
				Hide ();
			}
			return base.OnButtonPressEvent (evnt);
		}

		public virtual new void Hide ()
		{
			base.Hide ();
			resultsWindow.Hide ();
		}

		public void Reposition ()
		{
			int monitor;
			int extraGap = IconBoxRadius-(IconBoxPadding*2);
			Gdk.Rectangle geo, main, results;

			// Do special things for the first Pane
			// if (currentPane == Pane.First)
			// 	extraGap = IconBoxRadius-(IconBoxPadding*2);

			GetPosition (out main.X, out main.Y);
			GetSize (out main.Width, out main.Height);
			monitor = Screen.GetMonitorAtPoint (main.X, main.Y);
			geo = Screen.GetMonitorGeometry (monitor);
			main.X = (geo.Width - main.Width) / 2;
			main.Y = (int)((geo.Height - main.Height) / 2.5);
			Move (main.X, main.Y);

			resultsWindow.GetSize (out results.Width, out results.Height);
			results.Y = main.Y + main.Height;
			results.X = main.X + (IconBoxIconSize + 60) * (int) currentPane + IconBoxPadding*2 + extraGap;
			resultsWindow.Move (results.X, results.Y);
		}

		void OnControlKeyPressEvent (EventKey evnt)
		{
			switch ((Gdk.Key) evnt.KeyValue) {
				case Gdk.Key.q:
					Application.Quit ();
					break;
				default:
					break;
			}
		}

		void OnEscapeKeyPressEvent (EventKey evnt)
		{
			bool something_typed, results_showing;

			results_showing = resultsWindow.Visible;
			something_typed = CurrentContext.Query.Length > 0;

			if (currentPane == Pane.First && results_showing) resultsWindow.Hide ();
			ClearSearchResults ();

			if (!something_typed && results_showing) resultsWindow.Hide ();
			if (results_showing || something_typed) return;
			if (currentPane == Pane.First) Hide ();
			if (resultsWindow.Visible) {
				resultsWindow.Hide ();
			} else {
				SetDefaultState ();
			}
		}

		void OnActivateKeyPressEvent (EventKey evnt)
		{
			ActivateCommand ();
		}

		void OnDeleteKeyPressEvent (EventKey evnt)
		{
			if (CurrentContext.Query.Length == 0) return;

			if (CurrentContext.Query.Length > 1 || CurrentContext.ParentContext != null) {
				CurrentContext.Query = CurrentContext.Query.Substring (0, CurrentContext.Query.Length-1);
				QueueSearch ();
			} else {
				if (CurrentContext.ParentContext == null)
					ClearSearchResults ();
			}
		}

		void OnTabKeyPressEvent (EventKey evnt)
		{
			tabbing = true;
			if (CurrentPane == Pane.First &&
			    context[0].Results != null &&
			    context[0].Results.Length != 0) {
				resultsWindow.Hide ();
				CurrentPane = Pane.Second;
			} else if (currentPane == Pane.Second) {
				resultsWindow.Hide ();
				CurrentPane = Pane.First;
			}
			tabbing = false;
		}

		void OnUpDownKeyPressEvent (EventKey evnt)
		{
			if (!resultsWindow.Visible) {
				resultsWindow.Show ();
				return;
			}
			if ((Gdk.Key) evnt.KeyValue == Gdk.Key.Up) {
				resultsWindow.SelectPrev ();
			} else {
				resultsWindow.SelectNext ();
			}
		}

		void OnLeftRightKeyPressEvent (EventKey evnt)
		{
			if (CurrentContext.Results != null) {
				if ((Gdk.Key) evnt.KeyValue == Gdk.Key.Right) {
					SearchContext parentContext = CurrentContext;
					CurrentContext.FindingChildren = true;
					QueueSearch ();
					if (parentContext.FindingChildren)
						resultsWindow.Show ();
				} else if ((Gdk.Key) evnt.KeyValue == Gdk.Key.Left) {
					CurrentContext.FindingParent = true;
					QueueSearch ();
				}
			}
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
				QueueSearch ();
			}
		}

		protected override bool OnKeyPressEvent (EventKey evnt)
		{
			// Handle command keys (Quit, etc.)
			if (((int) evnt.State & (int) ModifierType.ControlMask) != 0) {
					OnControlKeyPressEvent (evnt);
					return false;
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
					OnUpDownKeyPressEvent (evnt);
					break;
				case Gdk.Key.Right:
				case Gdk.Key.Left:
					OnLeftRightKeyPressEvent (evnt);
					break;
				default:
					OnInputKeyPressEvent (evnt);
					break;
			}
			return base.OnKeyPressEvent (evnt);
		}

		protected virtual void ActivateCommand ()
		{
			IObject first, second;

			Hide ();

			items.Clear ();
			modItems.Clear ();

			first = GetCurrentObject (Pane.First);
			second = GetCurrentObject (Pane.Second);
			if (first != null && second != null) {
				if (first is IItem) {
					items.Add (first as IItem);
					command = second as ICommand;
				} else {
					items.Add (second as IItem);
					command = first as ICommand;
				}
				command.Perform (items.ToArray (), modItems.ToArray ());
			}
			SetDefaultState ();
		}

		private void OnResultsWindowSelectionChanged (object sender, ResultsWindowSelectionEventArgs args)
		{
			CurrentCursor = args.SelectedIndex;

			label.DisplayObject = CurrentContext.Results[CurrentCursor];
			label.Highlight = CurrentContext.Query;
			CurrentIconBox.DisplayObject = CurrentContext.Results[CurrentCursor];
			CurrentIconBox.Highlight = CurrentContext.Query;

			// If we're just tabbing, no need to search.
			if (!tabbing) {
				switch (currentPane) {
					case Pane.First:
						SearchSecondPane ("");
						break;
					case Pane.Second:
						break;
				}
			}
		}

		private void OnScreenChanged (object sender, EventArgs args)
		{
			SetColormap ();
		}

		protected virtual void SetFrameRadius ()
		{
			// Nothing
		}

		protected virtual void SetPane (Pane pane)
		{
			currentPane = pane;
			iconbox[0].IsFocused = (pane == Pane.First);
			iconbox[1].IsFocused = (pane == Pane.Second);
			iconbox[2].IsFocused = (pane == Pane.Third);

			resultsWindow.Results = CurrentContext.Results;
			resultsWindow.SelectedIndex = CurrentCursor;

			label.DisplayObject = GetCurrentObject (pane) ??
				new NoResultsFoundObject (CurrentContext.Query);

			Reposition ();
		}

		void QueueSearch ()
		{
			switch (currentPane) {
				case Pane.First:
					SearchFirstPane (CurrentContext.Query);
					break;
				case Pane.Second:
					SearchSecondPane (CurrentContext.Query);
					break;
			}
		}

		protected virtual void SearchFirstPane (string match)
		{
			context[1].Cursor = 0;
			context[2].Cursor = 0;

			context[0].Query = match;
			context[0].SearchTypes = new Type[] { typeof (IItem), typeof (ICommand) };
			context[0] = Do.UniverseManager.Search (context[0]);

			context[0].GenericObject = context[0].Results[context[0].Cursor];
			UpdatePane (Pane.First);

			context[1] = new SearchContext ();
			SearchSecondPane ("");
		}

		protected virtual void SearchSecondPane (string match)
		{
			IObject first;

			context[2].Cursor = 0;

			first = GetCurrentObject (Pane.First);
			// Set up the next pane based on what's in the first pane:
			context[1].GenericObject = first;
			if (first is IItem) {
				context[1].SearchTypes = new Type[] { typeof (ICommand) };
			} else {
				context[1].SearchTypes = new Type[] { typeof (IItem) };
			}

			context[1].Query = match;
			context[1] = Do.UniverseManager.Search (context[1]);

			if (context[1].Results.Length > 0) {
				UpdatePane (Pane.Second);
			} else {
				SetNoResultsFoundState (Pane.Second);
			}
		}

		protected void UpdatePane (Pane pane)
		{
			IObject currentObject;

			currentObject = GetCurrentObject (pane);
			if (currentObject != null) {
				iconbox[(int) pane].DisplayObject = currentObject;
				iconbox[(int) pane].Highlight = context[(int) pane].Query;
			} else {
				iconbox[(int) pane].DisplayObject = new NoResultsFoundObject (context[(int) pane].Query);
			}

			if (pane == currentPane) {
				label.DisplayObject = GetCurrentObject (pane);
				resultsWindow.Results = CurrentContext.Results;
				resultsWindow.SelectedIndex = CurrentCursor;
			}
		}

		protected void Build ()
		{
			VBox      vbox;
			Alignment align;
			Gtk.Image settings_icon;

			AppPaintable = true;
			KeepAbove = true;
			Decorated = false;
			// This typehint gets the window to raise all the way to top.
			TypeHint = WindowTypeHint.Splashscreen;

			try { SetIconFromFile ("/usr/share/icons/gnome/scalable/actions/system-run.svg"); } catch { }
			SetColormap ();

			resultsWindow = new ResultsWindow ();
			resultsWindow.SelectionChanged += OnResultsWindowSelectionChanged;

			currentPane = Pane.First;

			frame = new RoundedFrame ();
			frame.DrawFill = true;
			frame.FillColor = new Gdk.Color (0x35, 0x30, 0x45);
			frame.FillAlpha = WindowTransparency;
			frame.Radius = Screen.IsComposited ? IconBoxRadius : 0;
			Add (frame);
			frame.Show ();

			vbox = new VBox (false, 0);
			frame.Add (vbox);
			vbox.BorderWidth = IconBoxPadding;
			vbox.Show ();

			settings_icon = new Gtk.Image (GetType().Assembly, "settings-triangle.png");
			align = new Alignment (1.0F, 0.0F, 0, 0);
			align.SetPadding (3, 0, 0, IconBoxPadding);
			align.Add (settings_icon);
			vbox.PackStart (align, false, false, 0);
			settings_icon.Show ();
			align.Show ();

			resultsHBox = new HBox (false, IconBoxPadding*2);
			resultsHBox.BorderWidth = IconBoxPadding;
			vbox.PackStart (resultsHBox, false, false, 0);
			resultsHBox.Show ();

			iconbox = new IconBox[3];

			iconbox[0] = new IconBox (IconBoxIconSize);
			iconbox[0].IsFocused = true;
			iconbox[0].Radius = IconBoxRadius;
			resultsHBox.PackStart (iconbox[0], false, false, 0);
			iconbox[0].Show ();

			iconbox[1] = new IconBox (IconBoxIconSize);
			iconbox[1].IsFocused = false;
			iconbox[1].Radius = IconBoxRadius;
			resultsHBox.PackStart (iconbox[1], false, false, 0);
			iconbox[1].Show ();

			iconbox[2] = new IconBox (IconBoxIconSize);
			iconbox[2].IsFocused = false;
			iconbox[2].Radius = IconBoxRadius;
			// resultsHBox.PackStart (iconbox[2], false, false, 0);
			iconbox[2].Show ();

			align = new Alignment (0.5F, 0.5F, 1, 1);
			align.SetPadding (0, 2, 0, 0);
			label = new SymbolDisplayLabel ();
			align.Add (label);
			vbox.PackStart (align, false, false, 0);
			label.Show ();
			align.Show ();

			ScreenChanged += OnScreenChanged;

			Reposition ();
		}

		protected virtual void SetColormap ()
		{
			Gdk.Colormap  colormap;

			colormap = Screen.RgbaColormap;
			if (colormap == null) {
				colormap = Screen.RgbColormap;
				Console.Error.WriteLine ("No alpha support.");
			}
			Colormap = colormap;
		}

		protected override bool OnExposeEvent (EventExpose evnt)
		{
			Cairo.Context cairo;

			using (cairo = Gdk.CairoHelper.Create (GdkWindow)) {
				cairo.Rectangle (evnt.Area.X, evnt.Area.Y, evnt.Area.Width, evnt.Area.Height);
				cairo.Color = new Cairo.Color (1.0, 1.0, 1.0, 0.0);
				cairo.Operator = Cairo.Operator.Source;
				cairo.Paint ();
			}
			return base.OnExposeEvent (evnt);
		}
	}
}
