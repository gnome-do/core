// GSymbolWindow.cs created with MonoDevelop
// User: dave at 11:15 AM 8/25/2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.Runtime.InteropServices;
using Gtk;
using Gdk;

using Do.Core;
using Do.PluginLib;

namespace Do.UI
{
	
	public class SymbolWindow : Gtk.Window
	{
		class DefaultIconBoxObject : IObject {
			public string Icon { get { return "gtk-find"; } }
			public string Name { get { return ""; } }
			public string Description { get { return ""; } }
		}
		
		class NoItemsFoundObject : IObject {
			public string Icon { get { return "gtk-dialog-question"; } }
			public string Name { get { return "No items found."; } }
			public string Description { get { return ""; } }
		}

		class NoCommandsFoundObject : IObject {
			public string Icon { get { return "gtk-dialog-question"; } }
			public string Name { get { return "No commands found."; } }
			public string Description { get { return ""; } }
		}
		
		const int IconBoxIconSize = 128;
		const double WindowTransparency = 0.91;
		
		protected enum WindowFocus {
			ItemFocus,
			CommandFocus,
			IndirectItemFocus,
		}
		
		RoundedFrame frame;
		SymbolDisplayLabel displayLabel;
		ResultsWindow resultsWindow;
		HBox resultsHBox;
		IconBox itemBox;
		IconBox commandBox;
		IconBox modItemBox;
		
		protected Commander commander;
		protected Command currentCommand;
		protected Do.Core.Item currentItem;
		protected int currentItemIndex;
		protected int currentCommandIndex;
		protected WindowFocus focus;
		protected string searchString;
		protected string itemSearchString;
		
		public SymbolWindow (Commander commander) : base ("GNOME Go")
		{
			Build ();
			
			this.commander = commander;
			SetDefaultState ();
			
			commander.SetDefaultStateEvent += OnDefaultStateEvent;
			commander.SetSearchingItemsStateEvent += OnSearchingStateEvent;
			commander.SetSearchingCommandsStateEvent += OnSearchingStateEvent;
			commander.SetItemSearchCompleteStateEvent += OnSearchCompleteStateEvent;
			commander.SetCommandSearchCompleteStateEvent += OnSearchCompleteStateEvent;
		}
		
		protected void Build ()
		{
			VBox         vbox;
			Alignment align;
			
			AppPaintable = true;
			KeepAbove = true;
			Decorated = false;
			// This typehint gets the window to raise all the way to top.
			TypeHint = WindowTypeHint.Splashscreen;
				
			try { SetIconFromFile ("/usr/share/icons/gnome/scalable/actions/system-run.svg"); } catch { }
			SetColormap ();

			resultsWindow = new ResultsWindow ();
			resultsWindow.SelectionChanged += OnResultsWindowSelectionChanged;

			focus = WindowFocus.ItemFocus;
			searchString = itemSearchString = "";
			
			frame = new RoundedFrame ();
			frame.DrawFill = true;
			frame.FillColor = new Gdk.Color (0x35, 0x30, 0x45);
			frame.FillAlpha = WindowTransparency;
			Add (frame);
			frame.Show ();
			
			vbox = new VBox (false, 12);
			frame.Add (vbox);
			vbox.BorderWidth = 16;
			vbox.Show ();		
			
			resultsHBox = new HBox (false, 12);
			resultsHBox.BorderWidth = 0;
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
			Hide ();
			return false;
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
			SetUposition (main.X, main.Y);
			
			resultsWindow.GetSize (out results.Width, out results.Height);
			results.Y = main.Y + main.Height;
			resultsWindow.SetUposition (main.X, results.Y);			
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
				if (focus == WindowFocus.ItemFocus) {
					if (searchString.Length == 0) {
						Hide ();
					}
					resultsWindow.Hide ();
					commander.State = CommanderState.Default;
				} else {
					if (searchString.Length == 0) {
						commander.State = CommanderState.Default;
						resultsWindow.Hide ();
					} else {
						searchString = "";
						QueueSearch ();
					}
				}
				break;
			case Gdk.Key.Return:
			case Gdk.Key.ISO_Enter:
				ActivateCommand ();
				break;
			case Gdk.Key.Delete:
			case Gdk.Key.BackSpace:
				if (searchString.Length > 1) {
					searchString = searchString.Substring (0, searchString.Length-1);
					QueueSearch ();
				} else {
					commander.State = CommanderState.Default;
				}
				break;
			case Gdk.Key.Tab:
				if (focus == WindowFocus.ItemFocus && commander.CurrentItem != null) {
					SetWindowFocus (WindowFocus.CommandFocus);
				} else if (focus == WindowFocus.CommandFocus) {
					SetWindowFocus (WindowFocus.ItemFocus);
				}
				break;
			case Gdk.Key.Up:
			case Gdk.Key.Down:
			{
				int num_items;
				
				num_items = 0;
				switch (focus) {
				case WindowFocus.ItemFocus:
					num_items = commander.CurrentItems.Length;
					break;
				case WindowFocus.CommandFocus:
					num_items = commander.CurrentCommands.Length;
					break;
				}
				if (num_items == 0) {
					return false;
				}
				
				if (key == Gdk.Key.Up) {
					resultsWindow.SelectPrev ();
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
			return false;
		}
		
		protected virtual void ActivateCommand ()
		{
			commander.Execute ();
			Hide ();
		}
		
		private void OnResultsWindowSelectionChanged (object sender, ResultsWindowSelectionEventArgs args)
		{
			if (focus == WindowFocus.ItemFocus) {
				SetItemIndex (args.SelectedIndex, searchString);
			} else if (focus == WindowFocus.CommandFocus) {
				SetCommandIndex (args.SelectedIndex, searchString);
			}			
		}

		private void OnScreenChanged (object sender, EventArgs args)
		{
			SetColormap ();
		}
		
		protected override bool OnExposeEvent (EventExpose evnt)
		{
			Cairo.Context cairo;
			
			cairo = Gdk.CairoHelper.Create (GdkWindow);
			cairo.Rectangle (evnt.Area.X, evnt.Area.Y, evnt.Area.Width, evnt.Area.Height);
			cairo.Color = new Cairo.Color (1.0, 1.0, 1.0, 0.0);
			cairo.Operator = Cairo.Operator.Source;
			cairo.Paint ();
			
			return base.OnExposeEvent (evnt);
		}
		
		protected virtual void SetWindowFocus (WindowFocus focus)
		{	
			IObject[] results;
			int selectedIndex;
			
			if (this.focus == focus) {
				return;
			}			
			this.focus = focus;

			results = null;
			selectedIndex = -1;
			switch (focus) {
			case WindowFocus.CommandFocus:
				searchString = commander.CommandSearchString;
				results = commander.CurrentCommands;
				selectedIndex = commander.CurrentCommandIndex;
				break;
			case WindowFocus.ItemFocus:
				searchString = commander.ItemSearchString;
				results = commander.CurrentItems;
				selectedIndex = commander.CurrentItemIndex;
				break;
			}

			resultsWindow.Results = results;
			resultsWindow.SelectedIndex = selectedIndex;
			
			displayLabel.DisplayObject = resultsWindow.SelectedObject;
			displayLabel.Highlight = searchString;
			itemBox.IsFocused = (focus == WindowFocus.ItemFocus);
			commandBox.IsFocused = (focus == WindowFocus.CommandFocus);
			modItemBox.IsFocused = (focus == WindowFocus.IndirectItemFocus);
		}
		
		protected virtual void QueueSearch ()
		{
			switch (focus) {
			case WindowFocus.ItemFocus:
				commander.SearchItems (searchString);
				break;
			case WindowFocus.CommandFocus:
				commander.SearchCommands (searchString);
				break;
			}
		}
		
		protected virtual void SetItemIndex (int itemIndex, string match)
		{	
			if (itemIndex >= commander.CurrentItems.Length) {
				SetItemIndex (commander.CurrentItems.Length-1, match);
				return;
			}
			try {
				commander.CurrentItemIndex = itemIndex;
			} catch (IndexOutOfRangeException) {
				return;
			}
			itemBox.DisplayObject = commander.CurrentItem;
			itemBox.Highlight = match;
			if (focus == WindowFocus.ItemFocus) {
				displayLabel.DisplayObject = commander.CurrentItem;
				displayLabel.Highlight = searchString;
			}
			SetCommandIndex (0, "");
		}
		
		protected virtual void SetCommandIndex (int commandIndex, string match)
		{
			try {
				commander.CurrentCommandIndex = commandIndex;
			} catch (IndexOutOfRangeException) {
				return;
			}

			commandBox.DisplayObject = commander.CurrentCommand;
			commandBox.Highlight = match;
			if (focus == WindowFocus.CommandFocus) {
				displayLabel.DisplayObject = commander.CurrentCommand;
				displayLabel.Highlight = match;
			}
		}
		
		protected virtual void SetDefaultState ()
		{
			searchString = itemSearchString = "";

			SetWindowFocus (WindowFocus.ItemFocus);
			
			itemBox.DisplayObject = new DefaultIconBoxObject ();
			commandBox.Clear ();
			
			displayLabel.SetdisplayLabel ("Type to begin searching", "Type to start searching.");			
		}
		
		protected virtual void SetNoResultsFoundState ()
		{
			switch (focus) {
			case WindowFocus.CommandFocus:
				commandBox.DisplayObject = new NoCommandsFoundObject ();
				break;
			default:
			// case WindowFocus.ItemFocus:
				commandBox.Clear ();
				itemBox.DisplayObject = new NoItemsFoundObject ();
				break;
			}
			displayLabel.Text = "";
		}
		
		protected void OnDefaultStateEvent ()
		{
			SetDefaultState ();
		}
		
		protected void OnSearchingStateEvent ()
		{
		}
		
		protected void OnSearchCompleteStateEvent ()
		{
			GCObject[] results;
			
			switch (focus) {
			case WindowFocus.ItemFocus:
				results = commander.CurrentItems;
				break;
			case WindowFocus.CommandFocus:
				results = commander.CurrentCommands;
				break;
			default:
				results = new GCObject [0];
				break;
			}
		
			resultsWindow.Results = results;
			if (results.Length == 0) {
				SetNoResultsFoundState ();
			} else {
				SetItemIndex (commander.CurrentItemIndex, commander.ItemSearchString);
				SetCommandIndex (commander.CurrentCommandIndex, commander.CommandSearchString);
			}
		}
		
	}

	
}
