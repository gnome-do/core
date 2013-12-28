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
using System.Linq;
using Mono.Unix;

using Gdk;
using Gtk;

using Do.Universe;
using Do.Universe.Common;
using Do.Platform;
using Do.Interface.Widgets;

namespace Do.Interface {
	
	public class ClassicWindow : Gtk.Window, IDoWindow {
		
		//-------------------Class Members------------------
		Do.Interface.Widgets.Frame frame;
		SymbolDisplayLabel label;
		ResultsWindow resultsWindow;
		PositionWindow positionWindow;
		HBox resultsHBox;
		IconBox[] iconbox;
		IDoController controller;
		
		const int IconBoxIconSize = 128;
		const uint IconBoxPadding = 6;
		const int IconBoxRadius = 20;
		const int NumberResultsDisplayed = 6;

		const double WindowTransparency = 0.91;
		
		Pane currentPane;
		
		//-------------------Events-----------------------
		public new event DoEventKeyDelegate KeyPressEvent;
			
		//-------------------Properties-------------------

		public new string Name {
			get { return "Simple"; }
		}
		
		public Pane CurrentPane {
			get {
				return currentPane;
			}
			set {
				if (currentPane == value) return;

				currentPane = value;
				iconbox[0].IsFocused = value == Pane.First;
				iconbox[1].IsFocused = value == Pane.Second;
				iconbox[2].IsFocused = value == Pane.Third;

				Reposition ();
			}
		}
		
		public PositionWindow PositionWindow {
			get {
				if (positionWindow == null)
					positionWindow = new PositionWindow (this, resultsWindow);
				return positionWindow;
			}
		}
		
		//-------------------ctor----------------------
		public ClassicWindow () : base (Gtk.WindowType.Toplevel)
		{
			
		}
		
		public void Initialize (IDoController controller)
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

			resultsWindow = new ResultsWindow (BackgroundColor, 
			                                   NumberResultsDisplayed);
			resultsWindow.SelectionChanged += OnResultsWindowSelectionChanged;

			currentPane = Pane.First;

			frame = new GlossyRoundedFrame ();
			frame.DrawFill = frame.DrawFrame = true;
			frame.FillColor = frame.FrameColor = BackgroundColor;
			frame.FillAlpha = frame.FrameAlpha = WindowTransparency;
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

			resultsHBox = new HBox (false, (int) IconBoxPadding * 2);
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
			resultsHBox.PackStart (iconbox[2], false, false, 0);
			// iconbox[2].Show ();

			align = new Alignment (0.5F, 0.5F, 1, 1);
			align.SetPadding (0, 2, 0, 0);
			label = new SymbolDisplayLabel ();
			align.Add (label);
			vbox.PackStart (align, false, false, 0);
			label.Show ();
			align.Show ();

			ScreenChanged += OnScreenChanged;
			ConfigureEvent += OnConfigureEvent;
			SizeAllocated += delegate { Reposition (); };
			iconbox[0].LinesChanged += OnLineChangedEvent;
			iconbox[1].LinesChanged += OnLineChangedEvent;
			iconbox[2].LinesChanged += OnLineChangedEvent;
			Reposition ();
		}
		
		protected override bool OnKeyPressEvent (EventKey evnt)
		{
			KeyPressEvent (evnt);

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
			colormap.Dispose ();
		}
		
		private Gdk.Color BackgroundColor
		{
			get {
				byte r, g, b;
				Gdk.Color bgColor;

				using (Gtk.Style style = Gtk.Rc.GetStyle (this)) {
					bgColor = style.Backgrounds[(int) StateType.Selected];
				}
				r = (byte) ((bgColor.Red) >> 8);
				g = (byte) ((bgColor.Green) >> 8);
				b = (byte) ((bgColor.Blue) >> 8);
				
				// Useful for making overbright themes less ugly. Still trying
				// to find a happy balance between 50 and 90...
				byte maxLum = 60;
				double hue, sat, val;
				Interface.Util.Appearance.RGBToHSV(r, g, b, out hue, 
				                                out sat, out val);
				val = Math.Min (val, maxLum);
				
				Interface.Util.Appearance.HSVToRGB(hue, sat, val, out r,
				                                out g, out b);
				
				return new Gdk.Color (r, g, b);
			}
		}
		
		private void OnScreenChanged (object sender, EventArgs args)
		{
			SetColormap ();
		}
		
		private void OnConfigureEvent (object sender, ConfigureEventArgs args)
		{
			Reposition ();
		}
		
		protected override void OnStyleSet (Gtk.Style previous_style)
		{
			frame.FrameColor = frame.FillColor = BackgroundColor;
			resultsWindow.UpdateColors (BackgroundColor);
			
			base.OnStyleSet (previous_style);
		}
		
		public virtual void Reposition ()
		{
			Gtk.Application.Invoke (delegate {
				Gdk.Rectangle offset;
				int iconboxWidth;
				
				offset = new Rectangle (IconBoxRadius, 0, 0 ,0);
				iconboxWidth = IconBoxIconSize + 60;
				
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
				Services.Windowing.ShowMainMenu (end_x - 21, start_y + 16);
				// Have to re-grab the pane from the menu.
				Interface.Windowing.PresentWindow (this);
			} else if (!click_on_window) {
				controller.ButtonPressOffWindow ();
			}
			return base.OnButtonPressEvent (evnt);
		}
		
		private void OnResultsWindowSelectionChanged (object sender,
				ResultsWindowSelectionEventArgs args)
		{
			controller.NewContextSelection (CurrentPane, args.SelectedIndex);
		}

		  ///////////////////////
		 //     IDoWindow     //
		///////////////////////
		
		public void Summon ()
		{
			frame.Radius = Screen.IsComposited ? IconBoxRadius : 0;

			Resize (1, 1);
			Reposition ();
			Show ();
			Interface.Windowing.PresentWindow (this);
		}

		public void Vanish ()
		{
			Interface.Windowing.UnpresentWindow (this);
			Hide ();
		}

		public void Reset ()
		{
			resultsWindow.Clear ();
			
			iconbox[0].Clear ();
			iconbox[1].Clear ();
			iconbox[2].Clear ();
			
			QueueDraw ();
			Resize (1, 1);
			Reposition ();
			
			iconbox[0].DisplayObject = new Do.Interface.Widgets.DefaultIconBoxItem ();
			label.SetDisplayLabel (Catalog.GetString ("Type to begin searching"), 
			                       Catalog.GetString ("Type to start searching."));
		}

		public void Grow ()
		{
			iconbox[2].Show ();
			Resize (1, 1);
		}

		public void Shrink ()
		{
			iconbox[2].Hide ();
			Resize (1, 1);
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
		
		public void SetPaneContext (Pane pane, IUIContext context)
		{
			if (!context.Results.Any () && !context.LargeTextDisplay) {
				if (pane == Pane.First && context.ParentContext == null) {
					iconbox[0].TextOverlay = context.LargeTextDisplay;
					iconbox[0].DisplayObject = new Do.Interface.Widgets.DefaultIconBoxItem ();
					label.SetDisplayLabel (Catalog.GetString ("Type to begin searching"), 
					                       Catalog.GetString ("Type to start searching."));
				} else {
					Do.Universe.Item noRes = new NoResultsFoundItem (context.Query);
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
			
			if (string.IsNullOrEmpty (context.Query) && context.LargeTextDisplay) {
				iconbox[(int) pane].TextOverlay = context.LargeTextDisplay;
				iconbox[(int) pane].DisplayObject = new TextItem ("Enter Text") as Do.Universe.Item;

				if (!context.Results.Any ()) return;
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
		
		public bool ResultsCanHide { get { return true; } }
		
		protected override bool OnExposeEvent (EventExpose evnt)
		{
			Cairo.Context cairo;
			
			using (cairo = Gdk.CairoHelper.Create (GdkWindow)) {
				cairo.Rectangle (evnt.Area.X, evnt.Area.Y, evnt.Area.Width, evnt.Area.Height);
				cairo.SetSourceRGBA (1.0, 1.0, 1.0, 0.0);
				cairo.Operator = Cairo.Operator.Source;
				cairo.Paint ();
			}
			return base.OnExposeEvent (evnt);
		}
	}
}
