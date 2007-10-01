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
		const int ResultsListIconSize = 32;
		const int ResultsListLength = 7; 
		
		protected enum WindowFocus {
			ItemFocus,
			CommandFocus,
			IndirectItemFocus,
		}
		
		protected enum Column {
			ItemColumn = 0,
			PixbufColumn = 1,
			NameColumn = 2,
			NumberColumns = 3
		}
		
		ScrolledWindow resultsScrolledWindow;
		TreeView resultsTreeview;
		HBox resultsHBox;
		
		protected Commander commander;
		protected Command currentCommand;
		protected Do.Core.Item currentItem;
		protected int currentItemIndex;
		protected int currentCommandIndex;
		protected WindowFocus focus;
		protected string searchString;
		protected string itemSearchString;
		
		RoundedFrame frame;
		SymbolDisplayLabel displayLabel;

		IconBox itemBox;
		IconBox commandBox;
		IconBox modItemBox;
		
		static SymbolWindow ()
		{
		}
		
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
			TreeViewColumn column;
			CellRenderer   cell;
			Alignment align;
			
			AppPaintable = true;
			KeepAbove = true;
			Decorated = false;
			// This typehint gets the window to raise all the way to top.
			TypeHint = WindowTypeHint.Splashscreen;
				
			try { SetIconFromFile ("/usr/share/icons/gnome/scalable/actions/system-run.svg"); } catch { }
			SetColormap ();
			
			focus = WindowFocus.ItemFocus;
			searchString = itemSearchString = "";
			
			frame = new RoundedFrame ();
			frame.DrawFill = true;
			frame.FillColor = new Gdk.Color (0x35, 0x30, 0x45);
			frame.FillAlpha = 0.9;
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
			
			resultsScrolledWindow = new ScrolledWindow ();
			resultsScrolledWindow.SetSizeRequest (-1, (ResultsListIconSize + 2) * ResultsListLength + 2);
			resultsScrolledWindow.SetPolicy (PolicyType.Automatic, PolicyType.Automatic);
			resultsScrolledWindow.ShadowType = ShadowType.None;
			vbox.PackStart (resultsScrolledWindow, true, true, 0);
			resultsScrolledWindow.Show ();
			
			resultsTreeview = new TreeView ();
			resultsTreeview.EnableSearch = false;
			resultsTreeview.HeadersVisible = false;
			resultsScrolledWindow.Add (resultsTreeview);
			resultsTreeview.Show ();
			
			resultsTreeview.Model = new ListStore (new Type[] { typeof(GCObject), typeof(Pixbuf), typeof(string) }); 
			
			column = new TreeViewColumn ();
			cell = new CellRendererPixbuf ();
			column.PackStart (cell, false);
			
			cell.CellBackgroundGdk = new Color (byte.MaxValue, byte.MaxValue, byte.MaxValue);
			// This property is not available.
			// cell.CellBackgroundSet = true;
			// Maybe below?
			// cell.CellBackground = "white";
			cell.SetFixedSize (-1, 4 + ResultsListIconSize - (int) cell.Ypad);
			column.AddAttribute (cell, "pixbuf", (int) Column.PixbufColumn);
			
			cell = new CellRendererText ();
			(cell as CellRendererText).Ellipsize = Pango.EllipsizeMode.End;
			column.PackStart (cell, true);
			column.AddAttribute (cell, "markup", (int) Column.NameColumn);
			
			resultsTreeview.AppendColumn (column);

			resultsTreeview.Selection.Changed += OnWindowResultRowSelected;
			ScreenChanged += OnScreenChanged;
			
			SetPosition (WindowPosition.Center);
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
		
		protected virtual void ShowSearchResults ()
		{
			Requisition size;
			
			resultsScrolledWindow.Show ();
			// instruction.Hide ();
			size = SizeRequest ();
			Resize (size.Width, size.Height);
		}
		
		protected virtual void HideSearchResults ()
		{
			Requisition size;
			
			resultsScrolledWindow.Hide ();
			// instruction.Show ();
			size = SizeRequest();
			Resize (size.Width, size.Height);
		}	
		
		protected override bool OnButtonPressEvent (EventButton evnt)
		{
			Hide ();
			commander.State = CommanderState.Default;
			return false;
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
					commander.State = CommanderState.Default;
				} else {
					if (searchString.Length == 0) {
						commander.State = CommanderState.Default;
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
				TreeModel model;
				TreeIter iter;
				TreePath path;
				int num_items = 0;
				
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
				
				resultsTreeview.Selection.GetSelected (out model, out iter);
				path = model.GetPath (iter);
				
				if (key == Gdk.Key.Up) {
					if (!path.Prev ()) {
						HideSearchResults ();
					}
				}
				else if (key == Gdk.Key.Down && num_items > 0) {
					if (resultsScrolledWindow.Visible) {
						path.Next ();
					} else {
						ShowSearchResults ();
					}
				}					
				resultsTreeview.Selection.SelectPath (path);
				resultsTreeview.ScrollToCell (path, null, false, 0.0F, 0.0F);
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
			Hide ();
			commander.Execute ();
			commander.State = CommanderState.Default;
		}
		
		private void OnWindowResultRowSelected (object sender, EventArgs args)
		{
			int selectedRowIndex;
			
			try {
				selectedRowIndex = resultsTreeview.Selection.GetSelectedRows()[0].Indices[0];
			} catch (IndexOutOfRangeException) {
				return;
			}
			if (focus == WindowFocus.ItemFocus) {
				SetItemIndex (selectedRowIndex, searchString);
			} else if (focus == WindowFocus.CommandFocus) {
				SetCommandIndex (selectedRowIndex, searchString);
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
			IObject currentObject;
			
			currentObject = null;
			if (this.focus == focus) {
				return;
			}			
			this.focus = focus;

			switch (focus) {
			case WindowFocus.CommandFocus:
				searchString = commander.CommandSearchString;
				currentObject = commander.CurrentCommand;
				break;
			case WindowFocus.ItemFocus:
				searchString = commander.ItemSearchString;
				currentObject = commander.CurrentItem;
				break;
			}

			// Repopulate the results list.
			OnSearchCompleteStateEvent ();
			
			displayLabel.DisplayObject = currentObject;
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
			
			HideSearchResults ();
			(resultsTreeview.Model as ListStore).Clear ();
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
			ListStore store;
			TreeIter  iter, selected_iter;
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
			
			store = resultsTreeview.Model as ListStore;
			store.Clear ();
			if (results.Length == 0) {
				SetNoResultsFoundState ();
			} else {
				SetItemIndex (commander.CurrentItemIndex, commander.ItemSearchString);
				SetCommandIndex (commander.CurrentCommandIndex, commander.CommandSearchString);
				
				int current_index = 0;
				selected_iter = default (TreeIter);
				foreach (GCObject result in results) {
					Pixbuf small_icon = Util.PixbufFromIconName (result.Icon, ResultsListIconSize);
					string result_info = string.Format ("<b>{0}</b>\n<i><small>{1}</small></i>", result.Name, result.Description);
					iter = store.AppendValues (new object[] { result, small_icon, result_info });
					
					if ((focus == WindowFocus.ItemFocus && commander.CurrentItemIndex == current_index) ||
						(focus == WindowFocus.CommandFocus && commander.CurrentCommandIndex == current_index)) {
						selected_iter = iter;
					}
					current_index++;
				}
				resultsTreeview.Selection.SelectIter (selected_iter);
				resultsTreeview.ScrollToCell (resultsTreeview.Model.GetPath (selected_iter),
				                              null, false, 0.0F, 0.0F);
				
			}
		}
		
	}

	
}
