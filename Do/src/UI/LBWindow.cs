// GLBWindow.cs created with MonoDevelop
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
	
	public class LBWindow : Gtk.Window
	{
		
		private static Pixbuf default_item_pixbuf, missing_item_pixbuf;
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
		
		protected Commander commander;
		protected Command current_command;
		protected Do.Core.Item current_item;
		protected int current_item_index;
		protected int current_command_index;
		protected WindowFocus focus;
		protected string searchString;
		protected string itemSearchString;
		
		LBFrame frame;
		LBDisplayText displayText;
	
		ScrolledWindow result_sw;
		HBox result_hbox;

		LBIconBox item_icon_box;
		LBIconBox command_icon_box;
		LBIconBox iitem_icon_box;

		TreeView result_treeview;

		bool          can_rgba;
		bool          transparent;
		
		static LBWindow ()
		{
			default_item_pixbuf = Util.PixbufFromIconName ("gtk-find", IconBoxIconSize);
            missing_item_pixbuf = Util.PixbufFromIconName ("gtk-dialog-question", IconBoxIconSize);
		}
		
		public LBWindow (Commander commander, bool transparent) : base ("GNOME Go")
		{
			Build ();
			
			this.commander = commander;
			SetDefaultState ();
			Transparent = transparent;
			
			commander.SetDefaultStateEvent += OnDefaultStateEvent;
			commander.SetSearchingItemsStateEvent += OnSearchingStateEvent;
			commander.SetSearchingCommandsStateEvent += OnSearchingStateEvent;
			commander.SetItemSearchCompleteStateEvent += OnSearchCompleteStateEvent;
			commander.SetCommandSearchCompleteStateEvent += OnSearchCompleteStateEvent;
		}
		
		public bool Transparent {
			get { return transparent; }
			set {
				if (value && !can_rgba) {
					Console.Error.WriteLine ("Cannot paint window transparent (no rgba).");
					return;
				}
				transparent = value;	
				if (transparent) {
					item_icon_box.Transparent = true;
					iitem_icon_box.Transparent = true;
					command_icon_box.Transparent = true;
					frame.Fill = true;
					result_sw.ShadowType = ShadowType.None;
				} else {
					result_sw.ShadowType = ShadowType.In;
				}
				if (IsDrawable) {
					QueueDraw ();
				}
			}
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
				
			try { SetIconFromFile ("/usr/share/icons/gnome/scalable/actions/system-run.svg"); } catch { }
			SetColormap ();
			
			focus = WindowFocus.ItemFocus;
			searchString = itemSearchString = "";
			
			frame = new LBFrame ();
			frame.FillColor = new Gdk.Color (0, 0, 0);
			frame.FillAlpha = (ushort) (ushort.MaxValue * 0.8);
			Add (frame);
			frame.Show ();
			
			vbox = new VBox (false, 12);
			frame.Add (vbox);
			vbox.BorderWidth = 16;
			vbox.Show ();		
			
			result_hbox = new HBox (false, 12);
			result_hbox.BorderWidth = 0;
			vbox.PackStart (result_hbox, false, false, 0);
			result_hbox.Show ();
			
			item_icon_box = new LBIconBox (IconBoxIconSize);
			item_icon_box.IsFocused = true;
			result_hbox.PackStart (item_icon_box, false, false, 0);
			item_icon_box.Show ();
			
			command_icon_box = new LBIconBox (IconBoxIconSize);
			command_icon_box.IsFocused = false;
			result_hbox.PackStart (command_icon_box, false, false, 0);
			command_icon_box.Show ();
			
			iitem_icon_box = new LBIconBox (IconBoxIconSize);
			iitem_icon_box.IsFocused = false;
			// result_hbox.PackStart (iitem_icon_box, false, false, 0);
			iitem_icon_box.Show ();
	
			align = new Alignment (0.5F, 0.5F, 1, 1);
			align.SetPadding (0, 0, 0, 0);
			displayText = new LBDisplayText ();
			align.Add (displayText);
			vbox.PackStart (align, false, false, 0);
			displayText.Show ();
			align.Show ();
			
			result_sw = new ScrolledWindow ();
			result_sw.SetSizeRequest (-1, (ResultsListIconSize + 2) * ResultsListLength + 2);
			result_sw.SetPolicy (PolicyType.Automatic, PolicyType.Automatic);
			result_sw.ShadowType = ShadowType.In;
			vbox.PackStart (result_sw, true, true, 0);
			result_sw.Show ();
			
			result_treeview = new TreeView ();
			result_treeview.EnableSearch = false;
			result_treeview.HeadersVisible = false;
			result_sw.Add (result_treeview);
			result_treeview.Show ();
			
			result_treeview.Model = new ListStore (new Type[] { typeof(GCObject), typeof(Pixbuf), typeof(string) }); 
			
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
			
			result_treeview.AppendColumn (column);
			
			result_treeview.Selection.Changed += OnWindowResultRowSelected;
			ScreenChanged += OnScreenChanged;
			
			SetPosition (WindowPosition.Center);
		}
	
		protected virtual void SetColormap ()
		{
			Gdk.Colormap  colormap;

			colormap = this.Screen.RgbaColormap;
			if (colormap != null) {
				can_rgba = true;
			} else {
				colormap = this.Screen.RgbColormap;
				System.Console.WriteLine ("No alpha support.");
			}
			this.Colormap = colormap;
		}
		
		protected virtual void ShowSearchResults ()
		{
			Requisition size;
			
			result_sw.Show ();
			// instruction.Hide ();
			size = SizeRequest ();
			Resize (size.Width, size.Height);
		}
		
		protected virtual void HideSearchResults ()
		{
			Requisition size;
			
			result_sw.Hide ();
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
				
				result_treeview.Selection.GetSelected (out model, out iter);
				path = model.GetPath (iter);
				
				if (key == Gdk.Key.Up) {
					if (!path.Prev ()) {
						HideSearchResults ();
					}
				}
				else if (key == Gdk.Key.Down && num_items > 0) {
					if (result_sw.Visible) {
						path.Next ();
					} else {
						ShowSearchResults ();
					}
				}					
				result_treeview.Selection.SelectPath (path);
				result_treeview.ScrollToCell (path, null, false, 0.0F, 0.0F);
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
				selectedRowIndex = result_treeview.Selection.GetSelectedRows()[0].Indices[0];
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
		// private void OnExposeEvent (object sender, ExposeEventArgs args)
		{
			//return;
			Cairo.Context cairo;
			
			if (!transparent) {
				Gtk.Style.PaintBox (Style,
				                    GdkWindow,
				                    StateType.Normal,
				                    ShadowType.In,
				                    evnt.Area,
				                    this,
				                    "base",
				                    0, 0, -1, -1);
			} else {
				cairo = Gdk.CairoHelper.Create (GdkWindow);
				cairo.Rectangle (evnt.Area.X, evnt.Area.Y, evnt.Area.Width, evnt.Area.Height);
				cairo.Color = new Cairo.Color (1.0, 1.0, 1.0, 0.0);
				cairo.Operator = Cairo.Operator.Source;
				cairo.Paint ();
			}
			
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
			
			displayText.DisplayObject = currentObject;
			displayText.Highlight = searchString;
			item_icon_box.IsFocused = (focus == WindowFocus.ItemFocus);
			command_icon_box.IsFocused = (focus == WindowFocus.CommandFocus);
			iitem_icon_box.IsFocused = (focus == WindowFocus.IndirectItemFocus);
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
			item_icon_box.Caption = commander.CurrentItem.Name;
			item_icon_box.Pixbuf = Util.PixbufFromIconName (commander.CurrentItem.Icon, IconBoxIconSize);
			if (focus == WindowFocus.ItemFocus) {
				displayText.DisplayObject = commander.CurrentItem;
				displayText.Highlight = searchString;
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

			command_icon_box.Caption = commander.CurrentCommand.Name;
			command_icon_box.Pixbuf = Util.PixbufFromIconName (commander.CurrentCommand.Icon, IconBoxIconSize);
			if (focus == WindowFocus.CommandFocus) {
				displayText.DisplayObject = commander.CurrentCommand;
				displayText.Highlight = match;
			}
		}
		
		protected virtual void SetDefaultState ()
		{
			searchString = itemSearchString = "";
			
			HideSearchResults ();
			(result_treeview.Model as ListStore).Clear ();
			SetWindowFocus (WindowFocus.ItemFocus);
			
			item_icon_box.Pixbuf = default_item_pixbuf;
			item_icon_box.Caption = "";
			command_icon_box.Clear ();
			
			displayText.SetDisplayText ("Type to begin searching", "press down arrow for more results");			
		}
		
		protected virtual void SetNoResultsFoundState ()
		{
			switch (focus) {
			case WindowFocus.CommandFocus:
				command_icon_box.Pixbuf = missing_item_pixbuf;
				command_icon_box.Caption = "No commands found";
				break;
			default:
			// case WindowFocus.ItemFocus:
				command_icon_box.Clear ();
				item_icon_box.Pixbuf = missing_item_pixbuf;
				item_icon_box.Caption = "No items found";
				break;
			}
		}
		
		protected void OnDefaultStateEvent ()
		{
			SetDefaultState ();
		}
		
		protected void OnSearchingStateEvent ()
		{
			displayText.Text = string.Format ("<u>{0}</u>", searchString);
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
			
			store = result_treeview.Model as ListStore;
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
				result_treeview.Selection.SelectIter (selected_iter);
				result_treeview.ScrollToCell (result_treeview.Model.GetPath (selected_iter),
				                              null, false, 0.0F, 0.0F);
				
			}
		}
		
	}

	
}
