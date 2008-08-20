// ShowCaseResultsWindow.cs
// 
// Copyright (C) 2008 GNOME-Do
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
using System.Runtime.InteropServices;
using System.Text;

using Gtk;
using Gdk;

using Do.Addins;
using Do.Universe;

namespace Do.UI
{
	
	
	public class ShowCaseResultsWindow : ResultsWindow
	{
		
		public ShowCaseResultsWindow(int fullWidth) : base (new Gdk.Color (0x00, 0x00, 0x00), 32, fullWidth, 5)
		{
		}
		
		protected override void Build ()
		{
			Alignment align;
			TreeViewColumn column;
			CellRenderer   cell;
			HBox hbox;

			KeepAbove = true;
			AppPaintable = true;
			AcceptFocus = false;
			// This typehint gets the window to raise all the way to top.
			TypeHint = WindowTypeHint.Splashscreen;
			
			SetColormap ();
			
			frame = new HalfRoundedFrame ();
			frame.DrawFrame = frame.DrawFill = true;
			frame.FillColor = frame.FrameColor = backgroundColor;
			frame.FillAlpha = 1;
			frame.FrameAlpha = 1;
			frame.Radius = 7;
			frame.Show ();
			
			vbox = new VBox (false, 0);
			Add (frame);
			frame.Add (vbox);
			vbox.BorderWidth = 3;
			vbox.Show ();
			
			//---------The breadcrum bar---------
			hbox = new HBox ();
			toolbar = new Toolbar ();
			align = new Alignment (0, .5f, 0, 0);
			resultsLabel = new Label ();
			queryLabel = new Label ();
			align.Add (queryLabel);
			hbox.PackStart (align, true, true, 4);
			hbox.PackStart (resultsLabel, false, false, 0);
			hbox.WidthRequest = DefaultWindowWidth - 10;
			toolbar.Add (hbox);
			toolbar.ShowAll ();
			vbox.PackStart (toolbar, false, false, 0);
			
			
			//---------Results Window
			resultsScrolledWindow = new ScrolledWindow ();
			resultsScrolledWindow.SetPolicy (PolicyType.Never, PolicyType.Never);
			resultsScrolledWindow.ShadowType = ShadowType.None;
			vbox.PackStart (resultsScrolledWindow, true, true, 0);
			resultsScrolledWindow.Show ();

			resultsTreeview = new TreeView ();
			resultsTreeview.EnableSearch = false;
			resultsTreeview.HeadersVisible = false;
			// If this is not set the tree will call IconDataFunc for all rows to 
			// determine the total height of the tree
			resultsTreeview.FixedHeightMode = true;
			
			resultsScrolledWindow.Add (resultsTreeview);
			resultsTreeview.Show ();

			resultsTreeview.Model = new ListStore (new Type[] {
				typeof (IObject),				
				typeof (string), 
			});

			column = new TreeViewColumn ();			
			column.Sizing = Gtk.TreeViewColumnSizing.Fixed; 
				// because resultsTreeview.FixedHeightMode = true:  
				
			cell = new CellRendererPixbuf ();				
			cell.SetFixedSize (-1, 4 + DefaultResultIconSize - (int) cell.Ypad);

			int width, height;
			cell.GetFixedSize (out width, out height);
				
			column.PackStart (cell, false);
			column.SetCellDataFunc (cell, new TreeCellDataFunc (IconDataFunc));
				
			vbox.SetSizeRequest (DefaultWindowWidth, 
			                     (height + 2) * NumberResultsDisplayed + 
			                     (int) (vbox.BorderWidth * 2) + 20);
			
			cell = new CellRendererText ();
			(cell as CellRendererText).Ellipsize = Pango.EllipsizeMode.End;
			column.PackStart (cell, true);
			column.AddAttribute (cell, "markup", (int) Column.NameColumn);
			
			resultsTreeview.AppendColumn (column);

			resultsTreeview.Selection.Changed += OnResultRowSelected;
			Shown += OnShown;
			
			resultsTreeview.ModifyText (StateType.Normal, new Gdk.Color (0xff, 0xff, 0xff));
			resultsTreeview.ModifyBase (StateType.Normal, new Gdk.Color (0x00, 0x00, 0x00));
		}
	}
}
