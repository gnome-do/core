/* DoClassicWindow.cs
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
using Mono.Unix;

using Do.Addins;
using Do.Universe;
using Gdk;
using Gtk;

namespace Do.UI
{
	
	
	public class MiniWindow : Gtk.Window, IDoWindow
	{
		
		//-------------------Class Members------------------
		MiniWindowFrame frame;
		SymbolDisplayLabel label;
		ResultsWindow resultsWindow;
		PositionWindow positionWindow;
		HBox resultsHBox;
		MiniIconBox[] iconbox;
		IDoController controller;
		
		const int IconBoxIconSize = 48;
		const uint IconBoxPadding = 2;
		const int IconBoxRadius = 3;
		const int NumberResultsDisplayed = 4;
		
		const int MainRadius = 6;

		const double WindowTransparency = 0.95;
		
		Pane currentPane;
		
		//-------------------Events-----------------------
		public new event DoEventKeyDelegate KeyPressEvent;
			
		//-------------------Properties-------------------
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
					positionWindow = new PositionWindow (this, resultsWindow);
			}
		}
		
		//-------------------ctor----------------------
		public MiniWindow (IDoController controller) : base (Gtk.WindowType.Toplevel)
		{
			this.controller = controller;
			
			Build ();
		}
		
		//-------------------methods------------------
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

			try {
				SetIconFromFile ("/usr/share/icons/gnome/scalable/actions/system-run.svg");
			} catch { }
			SetColormap ();

			resultsWindow = new ResultsWindow (new Color(42, 45, 49), 24, 300, 
			                                   NumberResultsDisplayed);
			resultsWindow.SelectionChanged += OnResultsWindowSelectionChanged;
			resultsWindow.ResultInfoFormat = "<b>{0}</b>";

			currentPane = Pane.First;

			frame = new MiniWindowFrame ();
			frame.DrawFill = frame.DrawFrame = true;
			frame.FillColor = new Color(42, 45, 49);
			frame.FillAlpha = WindowTransparency;
			frame.FrameColor = new Color(0, 0, 0);
			frame.FrameAlpha = .35;
			frame.Radius = Screen.IsComposited ? MainRadius : 0;
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

			resultsHBox = new HBox (false, (int) IconBoxPadding * 2);
			resultsHBox.BorderWidth = IconBoxPadding;
			vbox.PackStart (resultsHBox, false, false, 0);
			resultsHBox.Show ();

			iconbox = new MiniIconBox[3];

			iconbox[0] = new MiniIconBox (IconBoxIconSize);
			iconbox[0].IsFocused = true;
			iconbox[0].Radius = IconBoxRadius;
			resultsHBox.PackStart (iconbox[0], false, false, 0);
			iconbox[0].Show ();

			iconbox[1] = new MiniIconBox (IconBoxIconSize);
			iconbox[1].IsFocused = false;
			iconbox[1].Radius = IconBoxRadius;
			resultsHBox.PackStart (iconbox[1], false, false, 0);
			iconbox[1].Show ();

			iconbox[2] = new MiniIconBox (IconBoxIconSize);
			iconbox[2].IsFocused = false;
			iconbox[2].Radius = IconBoxRadius;
			resultsHBox.PackStart (iconbox[2], false, false, 0);
			// iconbox[2].Show ();

			align = new Alignment (0.5F, 0.5F, 1, 1);
			align.SetPadding (0, 2, 0, 0);
			label = new SymbolDisplayLabel ();
			align.Add (label);
			vbox.PackStart (align, false, false, 0);
			label.Show ();
			//align.Show ();

			ScreenChanged += OnScreenChanged;
			ConfigureEvent += OnConfigureEvent;
			
			Reposition ();
		}
		
		protected override bool OnKeyPressEvent (EventKey evnt)
		{
			switch (evnt.Key) {
				case Gdk.Key.Page_Up:
					resultsWindow.SelectedIndex -= NumberResultsDisplayed;
					break;
				case Gdk.Key.Page_Down:
					resultsWindow.SelectedIndex += NumberResultsDisplayed;
					break;
				default:
					KeyPressEvent (evnt);
					break;
			}

			return base.OnKeyPressEvent (evnt);
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
		
		private void OnScreenChanged (object sender, EventArgs args)
		{
			SetColormap ();
		}
		
		private void OnConfigureEvent (object sender, ConfigureEventArgs args)
		{
			Reposition ();
		}
		
		public void Reposition ()
		{		
			Gdk.Rectangle offset;
			int iconboxWidth;

			offset = new Rectangle (MainRadius, 0, 0 ,0);
			iconboxWidth = (iconbox[0].Width + ((int) IconBoxPadding * 2));
			
			PositionWindow.UpdatePosition (iconboxWidth, currentPane, offset);
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
				controller.ButtonPressOffWindow ();
			}
			return base.OnButtonPressEvent (evnt);
		}
		
		protected  void OnResultsWindowSelectionChanged (object sender,
				ResultsWindowSelectionEventArgs args)
		{
			controller.NewContextSelection (CurrentPane, args.SelectedIndex);
		}

		  ///////////////////////
		 //     IDoWindow     //
		///////////////////////
		
		public void Summon ()
		{
			if (PositionWindow.GetMonitor ()) {
				Reposition ();
			}
			Show ();
			Util.Appearance.PresentWindow (this);
		}

		public void Vanish ()
		{
			Hide ();
		}

		public void Reset ()
		{
			resultsWindow.Clear ();
			
			iconbox[0].Clear ();
			iconbox[1].Clear ();
			iconbox[2].Clear ();
			
			iconbox[0].DisplayObject = new Do.Addins.DefaultIconBoxObject ();
			label.SetDisplayLabel (Catalog.GetString ("Type to begin searching"), 
			                       Catalog.GetString ("Type to start searching."));
		}

		public void Grow ()
		{
			iconbox[2].Show ();
			Resize (1, 1);
			GLib.Timeout.Add (70, delegate {
				Gdk.Threads.Enter ();
				Reposition ();
				Gdk.Threads.Leave ();
				return false;
			});
		}

		public void Shrink ()
		{
			iconbox[2].Hide ();
			Resize (1, 1);
			GLib.Timeout.Add (70, delegate {
				Gdk.Threads.Enter ();
				Reposition ();
				Gdk.Threads.Leave ();
				return false;
			});
		}
		
		public void GrowResults ()
		{
			if (!resultsWindow.Visible)
				resultsWindow.Show ();
		}
		
		public void ShrinkResults ()
		{
			if (resultsWindow.Visible)
				resultsWindow.Hide ();
		}
		
		public void SetPaneContext (Pane pane, SearchContext context)
		{
			if (context.Results.Length == 0) {
				NoResultsFoundObject noRes = new NoResultsFoundObject (context.Query);
				for (int i = (int) pane; i < 3; i++) {
					iconbox[i].Clear ();
					iconbox[i].DisplayObject = noRes;
					if (i == (int) CurrentPane) {
						label.SetDisplayLabel (noRes.Name, noRes.Description);
						resultsWindow.Context = context;
					}
				}
				return;
			}
			iconbox[(int) pane].DisplayObject = context.Selection;
			iconbox[(int) pane].Highlight = context.Query;
			
			if (pane == CurrentPane) {
				resultsWindow.Context = context;
				label.SetDisplayLabel (context.Selection.Name, context.Selection.Description);
			}
		}

		public void ClearPane (Pane pane)
		{
			iconbox[(int) pane].Clear ();
			
			if (pane == CurrentPane)
				resultsWindow.Clear ();
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
