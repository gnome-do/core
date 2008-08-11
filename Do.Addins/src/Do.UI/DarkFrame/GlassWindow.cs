// DarkFrame.cs
//
//GNOME Do is the legal property of its developers. Please refer to the
//COPYRIGHT file distributed with this
//source distribution.
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
//
//

using System;
using Mono.Unix;

using Do.Universe;
using Do.Addins;
using Gdk;
using Gtk;

namespace Do.UI
{
	
	
	public class GlassWindow : Gtk.Window, IDoWindow
	{
		GlassFrame frame;
		SymbolDisplayLabel label;
		ResultsWindow resultsWindow;
		PositionWindow positionWindow;
		HBox resultsHBox;
		IDoController controller;
		GlassIconBox[] iconbox;
		
		const int IconBoxIconSize = 64;
		const uint IconBoxPadding = 2;
		const int IconBoxRadius = 5;
		const int NumberResultsDisplayed = 6;
		
		int frameoffset;
		const int MainRadius = 13;
		
		Pane currentPane;
		
		//-------------------Events-----------------------
		public new event DoEventKeyDelegate KeyPressEvent;
			
		//-------------------Properties-------------------
		/// <value>
		/// IDoWindow public pane member
		/// </value>
		public Pane CurrentPane {
			get {
				return currentPane;
			}
			set {
				if (currentPane == value) return;

				currentPane = value;
				iconbox[0].IsFocused = (value == Pane.First);
				iconbox[1].IsFocused = (value == Pane.Second);
				iconbox[2].IsFocused = (value == Pane.Third);

				Reposition ();
			}
		}
		
		
		public PositionWindow PositionWindow {
			get {
				return positionWindow ?? 
					positionWindow = new PositionWindow(this, resultsWindow);
			}
		}
		//---------------------ctor-----------------------
		
		/// <summary>
		/// ctor
		/// </summary>
		/// <param name="controller">
		/// A <see cref="IDoController"/>
		/// </param>
		public GlassWindow (IDoController controller) 
			: base (Gtk.WindowType.Toplevel)
		{
			this.controller = controller;
			
			Build ();
		}
		
		/// <summary>
		/// Provides primary building and packing of the IDoWindow.  
		/// Custom widgets are packed in
		/// here and most settings are selected.
		/// </summary>
		protected void Build ()
		{
			VBox      vbox;

			AppPaintable = true;
			KeepAbove = true;
			Decorated = false;
			// This typehint gets the window to raise all the way to top.
			TypeHint = WindowTypeHint.Splashscreen;

			try {
				SetIconFromFile ("/usr/share/icons/gnome/scalable/actions/system-run.svg");
			} catch { }
			SetColormap ();

			resultsWindow = new ResultsWindow (new Color(15, 15, 15), 
			                                   NumberResultsDisplayed);
			resultsWindow.SelectionChanged += OnResultsWindowSelectionChanged;

			currentPane = Pane.First;

			//determine our offset frame for the glassing on the window
			frameoffset = 10;
			frame = new GlassFrame (frameoffset);
			frame.DrawFill = frame.DrawFrame = true;
			frame.FillColor = new Color(45, 45, 45);
			frame.FrameColor = new Color(255, 255, 255);
			frame.FrameAlpha = 1;
			frame.Radius = Screen.IsComposited ? MainRadius : 0;
			Add (frame);
			frame.Show ();

			vbox = new VBox (false, 0);
			frame.Add (vbox);
			vbox.BorderWidth = (uint) (IconBoxPadding + frameoffset);
			vbox.Show ();
			
			label = new SymbolDisplayLabel ();

			resultsHBox = new HBox (false, (int) IconBoxPadding * 2);
			resultsHBox.BorderWidth = IconBoxPadding;
			vbox.PackStart (resultsHBox, false, false, 0);
			resultsHBox.Show ();

			iconbox = new GlassIconBox[3];

			iconbox[0] = new GlassIconBox (IconBoxIconSize);
			iconbox[0].IsFocused = true;
			iconbox[0].Radius = IconBoxRadius;
			resultsHBox.PackStart (iconbox[0], true, true, 0);
			iconbox[0].Show ();

			iconbox[1] = new GlassIconBox (IconBoxIconSize);
			iconbox[1].IsFocused = false;
			iconbox[1].Radius = IconBoxRadius;
			resultsHBox.PackStart (iconbox[1], true, true, 0);
			iconbox[1].Show ();

			iconbox[2] = new GlassIconBox (IconBoxIconSize);
			iconbox[2].IsFocused = false;
			iconbox[2].Radius = IconBoxRadius;
			resultsHBox.PackStart (iconbox[2], true, true, 0);
			
			ScreenChanged += OnScreenChanged;
			ConfigureEvent += OnConfigureEvent;
			SizeAllocated += delegate { Reposition (); };
			iconbox[0].LinesChanged += OnLineChangedEvent;
			iconbox[1].LinesChanged += OnLineChangedEvent;
			iconbox[2].LinesChanged += OnLineChangedEvent;
			Reposition ();
		}
		
		/// <summary>
		/// Sets argb colormap if available
		/// </summary>
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
		
		/// <summary>
		/// The new screen may not be composited
		/// </summary>
		protected void OnScreenChanged (object sender, EventArgs args)
		{
			SetColormap ();
		}
		
		/// <summary>
		/// We need to position the windows when we first configure the UI
		/// </summary>
		protected void OnConfigureEvent (object sender, ConfigureEventArgs args)
		{
			Reposition ();
		}
		
		/// <summary>
		/// Used for mouse events on the ResultsWindow
		/// </summary>
		/// <param name="sender">
		/// A <see cref="System.Object"/>
		/// </param>
		/// <param name="args">
		/// A <see cref="ResultsWindowSelectionEventArgs"/>
		/// </param>
		protected  void OnResultsWindowSelectionChanged (object sender,
				ResultsWindowSelectionEventArgs args)
		{
			controller.NewContextSelection (CurrentPane, args.SelectedIndex);
		}
		
		/// <summary>
		/// Detect motion events in the area of the menu and show the menu button when hovered
		/// </summary>
		/// <param name="evnt">
		/// A <see cref="EventMotion"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/>
		/// </returns>
		protected override bool OnMotionNotifyEvent (EventMotion evnt)
		{
			int end_x, end_y, start_x, start_y;
			int point_x, point_y;

			GetPosition (out start_x, out start_y);
			GetSize (out end_x, out end_y);
			
			end_x += start_x;
			end_y += start_y;
			
			point_x = (int) evnt.XRoot;
			point_y = (int) evnt.YRoot;
			
			if ((end_x - 35 <= point_x) && 
				(point_x < end_x - 15) && 
			    (start_y+frameoffset <= point_y) && 
				(point_y < start_y + frameoffset + 15)) {
				if (!frame.HoverArrow)
					frame.HoverArrow = true;
			} else {
				if (frame.HoverArrow)
					frame.HoverArrow = false;
			}
			
			return base.OnMotionNotifyEvent (evnt);
		}
		
		/// <summary>
		/// Detect if we have clicked on or off the window and and alert the controller.
		/// </summary>
		/// <param name="evnt">
		/// A <see cref="EventButton"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/>
		/// </returns>
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
			click_near_settings_icon = (end_x - 35 <= click_x) && (click_x < end_x - 15) && 
			                            (start_y+frameoffset <= click_y) && (click_y < start_y + frameoffset + 15);
			if (click_near_settings_icon) {
				Addins.Util.Appearance.PopupMainMenuAtPosition (end_x - 35, start_y + 25);
				// Have to re-grab the pane from the menu.
				Addins.Util.Appearance.PresentWindow (this);
				frame.HoverArrow = false;
			} else if (!click_on_window) {
				controller.ButtonPressOffWindow ();
			}
			
			return base.OnButtonPressEvent (evnt);
		}
		
		/// <summary>
		/// Direct pass of key events to the controller
		/// </summary>
		/// <param name="evnt">
		/// A <see cref="EventKey"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/>
		/// </returns>
		protected override bool OnKeyPressEvent (EventKey evnt)
		{
			switch (evnt.Key) {
				/*case Gdk.Key.Page_Up:
					resultsWindow.SelectedIndex -= NumberResultsDisplayed;
					break;
				case Gdk.Key.Page_Down:
					resultsWindow.SelectedIndex += NumberResultsDisplayed;
					break;*/
				default:
					KeyPressEvent (evnt);
					break;
			}

			return base.OnKeyPressEvent (evnt);
		}
		
		/// <summary>
		/// Reposition the main window along with the results window.
		/// </summary>
		public void Reposition ()
		{
			Gtk.Application.Invoke (delegate {
				Gdk.Rectangle offset;
				int iconboxWidth;
				
				offset = new Rectangle (IconBoxRadius, 0, 0 ,0);
				iconboxWidth = IconBoxIconSize + 150;
				
				PositionWindow.UpdatePosition (iconboxWidth, currentPane, offset);
			});
		}
		
		protected void OnLineChangedEvent (object o, EventArgs a)
		{
			if ((int) o <= 2) return;
			this.QueueDraw ();
			this.Resize (1, 1);
			Reposition ();
		}
		
		  ///////////////////////
		 //     IDoWindow     //
		///////////////////////
		
		public void Summon ()
		{
			//needed to know where our monitor sits...
			PositionWindow.GetMonitor ();
			Resize (1, 1);
			Reposition ();
			Show ();
			Util.Appearance.PresentWindow (this);
		}

		/// <summary>
		/// Hide the window
		/// </summary>
		public void Vanish ()
		{
			Hide ();
		}

		/// <summary>
		/// Reset window back to default state.  Does not imply resetting window focus.
		/// </summary>
		public void Reset ()
		{
			resultsWindow.Clear ();
			
			iconbox[0].Clear ();
			iconbox[1].Clear ();
			iconbox[2].Clear ();
			
			QueueDraw ();
			Resize (1, 1);
			Reposition ();
			
			iconbox[0].DisplayObject = new Do.Addins.DefaultIconBoxObject ();
			label.SetDisplayLabel (Catalog.GetString ("Type to begin searching"), 
			                       Catalog.GetString ("Type to start searching."));
		}

		/// <summary>
		/// Shows third pane.
		/// </summary>
		public void Grow ()
		{
			iconbox[2].Show ();
			Resize (1, 1);
		}

		/// <summary>
		/// Hides third pane
		/// </summary>
		public void Shrink ()
		{
			iconbox[2].Hide ();
			Resize (1, 1);
		}
		
		/// <summary>
		/// Makes results window visible if not already
		/// </summary>
		public void GrowResults ()
		{
			if (!resultsWindow.Visible)
				resultsWindow.Show ();
		}
		
		/// <summary>
		/// Hide results window if not already
		/// </summary>
		public void ShrinkResults ()
		{
			if (resultsWindow.Visible)
				resultsWindow.Hide ();
		}
		
		/// <summary>
		/// Allow for controller to set the search context of a specific pane.  Sets a no results
		/// found view if there are no results, and properly sets the display label if the context
		/// set is also the current pane.
		/// </summary>
		/// <param name="pane">
		/// A <see cref="Pane"/>
		/// </param>
		/// <param name="context">
		/// A <see cref="SearchContext"/>
		/// </param>
		public void SetPaneContext (Pane pane, IUIContext context)
		{
			if (context.Results.Length == 0 && !context.LargeTextDisplay) {
				if (pane == Pane.First && context.ParentContext == null) {
					iconbox[0].TextOverlay = context.LargeTextDisplay;
					iconbox[0].DisplayObject = new Do.Addins.DefaultIconBoxObject ();
					label.SetDisplayLabel (Catalog.GetString ("Type to begin searching"), 
					                       Catalog.GetString ("Type to start searching."));
				} else {
					NoResultsFoundObject noRes = new NoResultsFoundObject (context.Query);
					for (int i = (int) pane; i < 3; i++) {
						iconbox[i].Clear ();
						iconbox[i].DisplayObject = noRes;
						if (i == (int) CurrentPane) {
							label.SetDisplayLabel (noRes.Name, noRes.Description);
							resultsWindow.Clear ();
						}
					}
				}
				return;
			}
			
			if (context.Query.Length == 0 && context.LargeTextDisplay) {
				iconbox[(int) pane].TextOverlay = context.LargeTextDisplay;
				iconbox[(int) pane].DisplayObject = new TextItem ("Enter Text");
				
				if (context.Results.Length == 0) return;
			} else {
				iconbox[(int) pane].TextOverlay = context.LargeTextDisplay;
				iconbox[(int) pane].DisplayObject = context.Selection;
				
				if (!context.LargeTextDisplay)
					iconbox[(int) pane].Highlight = context.Query;
			}
			
			if (context.Selection == null) return;
			
			if (pane == CurrentPane) {
				resultsWindow.Context = context;
				if (!context.LargeTextDisplay)
					label.SetDisplayLabel (context.Selection.Name, context.Selection.Description);
				else
					label.SetDisplayLabel ("", "Raw Text Mode");
			}
		}

		public void ClearPane (Pane pane)
		{
			iconbox[(int) pane].Clear ();
			
			if (pane == CurrentPane)
				resultsWindow.Clear ();
		}
		
		/// <summary>
		/// Draws an argb rectangle as the main window.s
		/// </summary>
		/// <param name="evnt">
		/// A <see cref="EventExpose"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/>
		/// </returns>
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
