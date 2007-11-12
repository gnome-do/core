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
using System.Runtime.InteropServices;

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
		class DefaultIconBoxObject : IObject {
			public string Icon { get { return "gtk-find"; } }
			public string Name { get { return ""; } }
			public string Description { get { return ""; } }
		}
		
		class NoResultsFoundObject : IObject {
			public string Icon { get { return "gtk-dialog-question"; } }
			public string Name { get { return "No results found."; } }
			public string Description { get { return "Try searching for something else."; } }
		}

		const int IconBoxIconSize = 128;
		const double WindowTransparency = 0.91;
		
		protected enum WindowFocus {
			FirstFocus = 0,
			SecondFocus = 1,
			ThirdItemFocus = 2,
		}
		
		RoundedFrame frame;
		SymbolDisplayLabel displayLabel;
		ResultsWindow resultsWindow;
		HBox resultsHBox;
		IconBox itemBox;
		IconBox commandBox;
		IconBox modItemBox;
		
		protected Commander commander;
		protected int currentItemIndex;
		protected int currentCommandIndex;
		protected WindowFocus focus;
		protected string searchString;
		
		protected SearchContext currentContext;
		
		protected SearchContext[] paneContext;
		
		private IObject[][] paneObjects;
		
		
		public SymbolWindow (Commander commander) : base (Gtk.WindowType.Toplevel)
		{
			Build ();
			
			this.commander = commander;
			currentContext = new SearchContext ();
			
			commander.SetDefaultStateEvent += OnDefaultStateEvent;
			commander.SetSearchingItemsStateEvent += OnSearchingStateEvent;
			commander.SetSearchingCommandsStateEvent += OnSearchingStateEvent;
			commander.SetFirstCompleteStateEvent += OnSearchFirstCompleteStateEvent;
			commander.SetSecondCompleteStateEvent += OnSearchSecondCompleteStateEvent;
		}
		
		protected void Build ()
		{
			VBox         vbox;
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

			focus = WindowFocus.FirstFocus;
			searchString = "";
			
			paneObjects = new IObject[2][];
			
			frame = new RoundedFrame ();
			frame.DrawFill = true;
			frame.FillColor = new Gdk.Color (0x35, 0x30, 0x45);
			frame.FillAlpha = WindowTransparency;
			SetFrameRadius ();
			Add (frame);
			frame.Show ();
			
			vbox = new VBox (false, 0);
			frame.Add (vbox);
			vbox.BorderWidth = 6;
			vbox.Show ();		
			
			settings_icon = new Gtk.Image (GetType().Assembly, "settings-triangle.png");
			align = new Alignment (1.0F, 0.0F, 0, 0);
			align.SetPadding (0, 0, 0, 0);
			align.Add (settings_icon);
			vbox.PackStart (align, false, false, 0);
			settings_icon.Show ();
			align.Show ();
			
			resultsHBox = new HBox (false, 12);
			resultsHBox.BorderWidth = 6;
			vbox.PackStart (resultsHBox, false, false, 0);
			resultsHBox.Show ();
			
			itemBox = new IconBox (IconBoxIconSize);
			itemBox.IsFocused = true;
			resultsHBox.PackStart (itemBox, false, false, 0);
			itemBox.Show ();
			
			commandBox = new IconBox (IconBoxIconSize);
			commandBox.IsFocused = false;
			resultsHBox.PackStart (commandBox, false, false, 0);
			commandBox.Show ();
			
			modItemBox = new IconBox (IconBoxIconSize);
			modItemBox.IsFocused = false;
			// resultsHBox.PackStart (modItemBox, false, false, 0);
			modItemBox.Show ();
	
			align = new Alignment (0.5F, 0.5F, 1, 1);
			align.SetPadding (0, 0, 0, 0);
			displayLabel = new SymbolDisplayLabel ();
			align.Add (displayLabel);
			vbox.PackStart (align, false, false, 0);
			displayLabel.Show ();
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
				Console.WriteLine ("No alpha support.");
			}
			Colormap = colormap;
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
			click_near_settings_icon = (end_x - 25) <= click_x && click_x < end_x &&
				                       start_y <= click_y && click_y < (start_y + 25);
			if (click_near_settings_icon) {
				Addins.Util.Appearance.PopupMainMenuAtPosition (end_x - 18, start_y + 16);
				// Have to re-grab the focus from the menu.
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
			commander.State = CommanderState.Default;
		}
		
		public void Reposition ()
		{
			int monitor;
			Gdk.Rectangle geo, main, results;
			
			GetPosition (out main.X, out main.Y);
			GetSize (out main.Width, out main.Height);
			monitor = Screen.GetMonitorAtPoint (main.X, main.Y);
			geo = Screen.GetMonitorGeometry (monitor);
			main.X = (geo.Width - main.Width) / 2;
			main.Y =  (int) ((geo.Height - main.Height) / 2.5);
			Move (main.X, main.Y);
			
			resultsWindow.GetSize (out results.Width, out results.Height);
			results.Y = main.Y + main.Height;
			results.X = main.X + (IconBoxIconSize + 60) * (int) focus + 10;
			resultsWindow.Move (results.X, results.Y);			
		}
		
		protected override bool OnKeyPressEvent (EventKey evnt)
		{
			Gdk.Key key;
			
			key = (Gdk.Key) evnt.KeyValue;
			
			// Handle command keys (Quit, etc.)
			if (((int) evnt.State & (int) ModifierType.ControlMask) != 0) {
				switch (key) {
				case Gdk.Key.q:
					Application.Quit ();
					break;
				}
				return false;
			}
			
			switch (key) {
			// Throwaway keys
			case Gdk.Key.Shift_L:
			case Gdk.Key.Control_L:
				break;
			case Gdk.Key.Escape:
				if (focus == WindowFocus.FirstFocus) {
					if (searchString.Length == 0) {
						Hide ();
					}
					resultsWindow.Hide ();
					commander.State = CommanderState.Default;
				} else {
					if (searchString.Length == 0) {
						resultsWindow.Hide ();
						commander.State = CommanderState.Default;
					} else {
						searchString = "";
					}
				}
				ResetContext ();
				break;
			case Gdk.Key.Return:
			case Gdk.Key.ISO_Enter:
				ActivateCommand ();
				break;
			case Gdk.Key.Delete:
			case Gdk.Key.BackSpace:
				if (searchString == null) {
					searchString = "";
				}
				if (searchString.Length > 1) {
					searchString = searchString.Substring (0, searchString.Length-1);
					DeleteCharacter ();
				} else {
					searchString = "";
					ResetContext ();
				}
				break;
			case Gdk.Key.Tab:
				resultsWindow.Hide ();
				if (focus == WindowFocus.FirstFocus && currentContext.Results != null) {				
					SetWindowFocus (WindowFocus.SecondFocus);
				} else if (focus == WindowFocus.SecondFocus) {
					SetWindowFocus (WindowFocus.FirstFocus);
				}
				break;
			case Gdk.Key.Up:
			case Gdk.Key.Down:
			{	
				if (key == Gdk.Key.Up) {
					if (resultsWindow.SelectedIndex == 0) {
						resultsWindow.Hide ();
					} else {
						resultsWindow.SelectPrev ();
					}
				}
				else if (key == Gdk.Key.Down) {
					if (resultsWindow.Visible) {
						resultsWindow.SelectNext ();
					} else {				
						resultsWindow.Show ();
					}
				}
			}
				break;
			case Gdk.Key.Right:
			case Gdk.Key.Left:
				break;
			default:
				char c;
				
				c = (char) Gdk.Keyval.ToUnicode ((uint) key);	
				if (char.IsLetterOrDigit (c)
				    || char.IsPunctuation (c)
				    || c == ' '
				    || char.IsSymbol (c)) {
					searchString += c;

					QueueSearch ();
				}
				break;
			}
			return base.OnKeyPressEvent (evnt);
		}
		
		protected virtual void ActivateCommand ()
		{
			ICommand command;
			IItem[] items = new IItem[1];
			IItem[] modItems = new IItem[0];
			
			// This class will be re-written soon to take better care
			// of corner cases like ones that lead to NullReferenceExceptions here.
			try {
				if (currentContext.FirstObject is IItem) {
					items[0] = paneContext[1].FirstObject as IItem;
					command = paneContext[1].SecondObject as ICommand;
				} else {
					items[0] = paneContext[1].SecondObject as IItem;
					command = paneContext[1].FirstObject as ICommand;			
				}
				
				command.Perform (items, modItems);
			} catch { }
			Hide ();
		}
		
		private void OnResultsWindowSelectionChanged (object sender, ResultsWindowSelectionEventArgs args)
		{
			if (focus == WindowFocus.FirstFocus) {
				paneContext[0].ObjectIndex = args.SelectedIndex;
				SetFirstIndex (paneContext[0].SearchString);
			}
			else if (focus == WindowFocus.SecondFocus) {
				paneContext[1].ObjectIndex = args.SelectedIndex;
				SetSecondIndex (paneContext[1].SearchString);
			}			
		}

		private void OnScreenChanged (object sender, EventArgs args)
		{
			SetColormap ();
		}
		
		protected virtual void SetFrameRadius ()
		{
			if (Screen.IsComposited) {
				frame.Radius = 10;
			} else {
				frame.Radius = 0;
			}
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
		
		protected virtual void SetWindowFocus (WindowFocus focus)
		{	
			IObject[] results;
			int selectedIndex;
			
			this.focus = focus;
			results = null;
			selectedIndex = -1;

			//Set the current context to the appropriate paneContext
			switch (focus) {
			case WindowFocus.SecondFocus:
				currentContext = paneContext[1];
				searchString = currentContext.SearchString;
				results = currentContext.Results;
				selectedIndex = currentContext.ObjectIndex;
				break;
			case WindowFocus.FirstFocus:
				currentContext = paneContext[0];
				searchString = currentContext.SearchString;		
				results = currentContext.Results;
				selectedIndex = currentContext.ObjectIndex;
				break;
			}
			if (searchString == null) {
				searchString = "";
			}

			//Change window to match new paneContext	
			resultsWindow.Results = results;
			resultsWindow.SelectedIndex = selectedIndex;			
			
			displayLabel.DisplayObject = resultsWindow.SelectedObject;
			displayLabel.Highlight = searchString;
			itemBox.IsFocused = (focus == WindowFocus.FirstFocus);
			commandBox.IsFocused = (focus == WindowFocus.SecondFocus);
			modItemBox.IsFocused = (focus == WindowFocus.ThirdItemFocus);
			Reposition ();
		}
		
		protected virtual void DeleteCharacter () 
		{
			//In either case move two contexts back (the reason being is because before we search
			//Queue search gives us two identical copies at the end of the linked list, so that
			//one can be manipulated by Search() and one stays the same)
			switch (focus) {
			case WindowFocus.FirstFocus:
				commander.State = CommanderState.SearchingItems;
				currentContext = currentContext.LastContext.LastContext;
				SearchContext temp;
				temp = currentContext;
				currentContext = currentContext.Clone ();
				currentContext.LastContext = temp;
				paneObjects[0] =  (currentContext.Results);
				paneContext[0] = currentContext;
				paneContext[1] = null;
				commander.State = CommanderState.FirstSearchComplete;
				break;
			case WindowFocus.SecondFocus:
				commander.State = CommanderState.SearchingCommands;
				currentContext = currentContext.LastContext.LastContext;
				SearchContext tempContext;
				tempContext = currentContext;
				currentContext = currentContext.Clone ();
				currentContext.LastContext = tempContext;
				paneContext[1] = currentContext;
				paneObjects[1] =  (currentContext.Results);
				commander.State = CommanderState.SecondSearchComplete;
				break;
			}
		}
		
		protected virtual void QueueSearch ()
		{
			//Set up the current search context to the proper state information then
			//call Search() on universe manager. After it's done change CommanderState to
			//update the window
			switch (focus) {
			case WindowFocus.FirstFocus:
				commander.State = CommanderState.SearchingItems;
				currentContext.SearchString = searchString;
				currentContext.SearchTypes = new Type[] { typeof (IItem), typeof (ICommand) };
				currentContext.FirstObject = null;
				currentContext = Do.UniverseManager.Search (currentContext);
				
				// For now, only allow commands if they take only ITextItem:
				List<IObject> filtered = new List<IObject> ();
				foreach (IObject o in currentContext.Results) {
					if (o is ICommand) {
						ICommand cmd = o as ICommand;
						if (cmd == null ||
						    cmd.SupportedItemTypes.Length != 1 ||
						    cmd.SupportedItemTypes[0] != typeof (ITextItem))
							continue;
					}
					filtered.Add (o);
				}
				currentContext.Results = filtered.ToArray ();
				paneObjects[0] = currentContext.Results;
				paneContext[0] = currentContext;
				commander.State = CommanderState.FirstSearchComplete;
				break;
			case WindowFocus.SecondFocus:
				commander.State = CommanderState.SearchingCommands;
				currentContext.SearchString = searchString;
				currentContext = Do.UniverseManager.Search (currentContext);
				paneObjects[1] = currentContext.Results;
				paneContext[1] = currentContext;
				commander.State = CommanderState.SecondSearchComplete;
				break;
			}
		}
		
		protected virtual void ResetContext ()
		{
			SetDefaultState ();
		}
		
		protected virtual void SetFirstIndex (string match)
		{	
			paneContext[0].FirstObject = paneContext[0].Results[paneContext[0].ObjectIndex];
			itemBox.DisplayObject = paneContext[0].FirstObject;
			itemBox.Highlight = match;
			displayLabel.DisplayObject = paneObjects[0][currentContext.ObjectIndex];
			displayLabel.Highlight= searchString;

			//Set up the next pane based on what's in the first pane
			if (paneContext[0].FirstObject is IItem) {
				paneContext[1] = paneContext[0].Clone ();
				paneContext[1].SearchTypes = new Type[] { typeof (ICommand) };
				paneContext[1].SearchString = "";
				paneContext[1] = Do.UniverseManager.Search (paneContext[1]);
				paneObjects[1] = paneContext[1].Results;
				paneContext[1].ObjectIndex = 0;
			}
			else {
				paneContext[1] = paneContext[0].Clone ();
				paneContext[1].SearchTypes = new Type[] { typeof (IItem) };
				paneContext[1].SearchString = "";
				paneContext[1] = Do.UniverseManager.Search (paneContext[1]);
				paneObjects[1] = paneContext[1].Results;
				paneContext[1].ObjectIndex = 0;
			}
			SetSecondIndex ("");
		}
		
		protected virtual void SetSecondIndex (string match)
		{
			if (paneContext[1].Results.Length > 0) {
				paneContext[1].SecondObject = paneContext[1].Results[paneContext[1].ObjectIndex];
				commandBox.DisplayObject = paneContext[1].SecondObject;
				
				commandBox.Highlight = match;
				if (focus == WindowFocus.SecondFocus) {
					displayLabel.DisplayObject = currentContext.SecondObject;
					displayLabel.Highlight = match;
				}
			} else {
				commandBox.DisplayObject = new NoResultsFoundObject ();
			}
		}
		
		protected virtual void SetDefaultState ()
		{
			resultsWindow.Hide ();
			paneContext = new SearchContext[3];
			currentContext = new SearchContext ();
			currentContext.SearchTypes = new Type[] { typeof (ICommand), typeof (IItem) };
			paneContext[0] = currentContext;
			searchString = "";
			
			SetWindowFocus (WindowFocus.FirstFocus);
			
			itemBox.DisplayObject = new DefaultIconBoxObject ();
			commandBox.Clear ();
			
			displayLabel.SetDisplayLabel ("Type to begin searching", "Type to start searching.");			
		}
		
		protected virtual void SetNoResultsFirstFoundState ()
		{
			commandBox.Clear ();
			itemBox.DisplayObject = new NoResultsFoundObject ();
			displayLabel.Text = "";
		}
		
		protected virtual void SetNoResultsSecondFoundState ()
		{
			commandBox.DisplayObject = new NoResultsFoundObject ();
			displayLabel.Text = "";
		}		
		
		protected void OnDefaultStateEvent ()
		{
			SetDefaultState ();
		}
		
		protected void OnSearchingStateEvent ()
		{
		}
		
		protected void OnSearchFirstCompleteStateEvent ()
		{
			IObject[] results;
			
			results = paneContext[0].Results;		
			resultsWindow.Results = results;
			if (results.Length == 0) {
				SetNoResultsFirstFoundState ();
			} else {
				SetFirstIndex (paneContext[0].SearchString);
			}
		}
		
		protected void OnSearchSecondCompleteStateEvent ()
		{
			IObject[] results;
			
			results = paneContext[1].Results;
			resultsWindow.Results = results;
			if (results.Length == 0) {
				SetNoResultsSecondFoundState ();
			} else {
				SetSecondIndex (paneContext[1].SearchString);
			}
		}
		
	}

	
}
