// ShowCase.cs
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

using Gtk;
using Gdk;
using Cairo;
using System;

using Do.Addins;
using Do.Universe;

namespace Do.UI
{
	
	
	public class ShowCase : Gtk.Window, IDoWindow
	{
		Pane currentPane;
		ShowCaseDrawingArea drawing_area;
		ShowCaseResultsWidget resultsWindow;
		PositionWindow positionWindow;
		Gtk.Label display_label;
		string[] queries;
		IDoController controller;

		public Pane CurrentPane {
			get {
				return currentPane;
			}
			set {
				if (currentPane == value)
					return;
				currentPane = value;
				drawing_area.Focus = value;
				
//				Reposition ();
			}
		}
		
		private ShowCaseDrawingContext DrawingContext {
			get; set;
		}
		
		private PositionWindow PositionWindow {
			get {
				return positionWindow ??
					positionWindow = new PositionWindow (this, null);
			}
		}
		
		public ShowCase(IDoController controller) : base (Gtk.WindowType.Toplevel)
		{
			this.controller = controller;
			Build ();
			DrawingContext = new ShowCaseDrawingContext (null, null, null, Pane.First);
			queries = new string[3];
		}

		private void Build ()
		{
			Decorated = false;
			AppPaintable = true;
			KeepAbove = true;
			
			TypeHint = WindowTypeHint.Splashscreen;
			
			SetColormap ();
			
			VBox vbox = new VBox ();
			this.BorderWidth = 15;
			
			drawing_area = new ShowCaseDrawingArea (500, 200);
			
			vbox.PackStart (drawing_area, false, false, 0);
			
			resultsWindow = new ShowCaseResultsWidget ();
			vbox.PackStart (resultsWindow, true, true, 0);
			
			vbox.BorderWidth = 0;
			vbox.ShowAll ();
			
			Add (vbox);
			
			Reposition ();
		}
		
		protected override bool OnKeyPressEvent (EventKey evnt)
		{
			KeyPressEvent (evnt);

			return base.OnKeyPressEvent (evnt);
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
		
		protected override bool OnExposeEvent (EventExpose evnt)
		{
			Cairo.Context cairo;
			int radius = 5;
			double x=.5, y=.5;
			
			using (cairo = Gdk.CairoHelper.Create (GdkWindow)) {
				cairo.Rectangle (evnt.Area.X, evnt.Area.Y, evnt.Area.Width, evnt.Area.Height);
				cairo.Color = new Cairo.Color (0, 0, 0, 0);
				cairo.Operator = Cairo.Operator.Source;
				cairo.Paint ();
				
				cairo.Operator = Cairo.Operator.Over;
				Gdk.Rectangle rect = new Gdk.Rectangle ();
				GetSize (out rect.Width, out rect.Height);
				rect.Width--;
				rect.Height--;
				
				cairo.NewPath ();
				cairo.MoveTo (x+radius, y);
				cairo.Arc (x+rect.Width-radius, y+radius, radius, (Math.PI*1.5), (Math.PI*2));
				cairo.Arc (x+rect.Width-radius, y+rect.Height-radius, radius, 0, (Math.PI*0.5));
				cairo.Arc (x+radius, y+rect.Height-radius, radius, (Math.PI*0.5), Math.PI);
				cairo.Arc (x+radius, y+radius, radius, Math.PI, (Math.PI*1.5));
				
				Cairo.LinearGradient pattern = new LinearGradient (0, 0, 0, 250);
				pattern.AddColorStop (0, new Cairo.Color (1, 1, 1, .85));
				pattern.AddColorStop (1, new Cairo.Color (.6, .6, .6, .85));
				cairo.Pattern = pattern;
				
				cairo.FillPreserve ();
				
				pattern = new LinearGradient (0, 0, rect.Width, rect.Height);
				pattern.AddColorStop (0.0, new Cairo.Color (0.0, 0.0, 0.0, .75));
				pattern.AddColorStop (0.2, new Cairo.Color (0.2, 0.2, 0.2, .20));
				pattern.AddColorStop (0.8, new Cairo.Color (0.2, 0.2, 0.2, .20));
				pattern.AddColorStop (1.0, new Cairo.Color (0.0, 0.0, 0.0, .75));
				
				cairo.Pattern = pattern;
				cairo.LineWidth = 1;
				cairo.Stroke ();
				
				cairo.NewPath ();
				cairo.MoveTo (rect.Width - 26, 5);
				cairo.LineTo (rect.Width - 14, 5);
				cairo.LineTo (rect.Width - 20, 11);
				cairo.ClosePath ();
				cairo.Color = new Cairo.Color (0.3, 0.3, 0.3);
				cairo.Fill ();
			}
			return base.OnExposeEvent (evnt);
		}
		
		private void Reposition ()
		{
			PositionWindow.UpdatePosition (0, Pane.First, new Gdk.Rectangle (11, -17, 0, 0));
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
		
		private void UpdateTopLabel ()
		{
//			if (queries[(int) CurrentPane] != null && queries[(int) CurrentPane].Length > 0)
//				display_label.Markup = "<b>Search: " + queries[(int) CurrentPane] + "</b>";
//			else
//				display_label.Markup = "";
		}
		
		//****************IDoWindow******************
		public void Summon ()
		{
			Show ();
			Reposition ();
			Util.Appearance.PresentWindow (this);
		}

		public void Vanish ()
		{
			Hide ();
		}

		public void Reset ()
		{
			resultsWindow.Clear ();
			
			DrawingContext = new ShowCaseDrawingContext (null, null, null, Pane.First);
			queries = new string [3];
			drawing_area.Clear ();
			return;
		}

		public void Grow ()
		{
			drawing_area.DrawTertiary = true;
			QueueDraw ();
			return;
		}

		public void Shrink ()
		{
			drawing_area.DrawTertiary = false;
			QueueDraw ();
			return;
		}

		public void GrowResults ()
		{
			resultsWindow.Show ();
			return;
		}

		public void ShrinkResults ()
		{
			resultsWindow.Hide ();
			Resize (1, 1);
			return;
		}

		public void SetPaneContext (Pane pane, IUIContext context)
		{
			drawing_area.SetPaneObject (pane, context.Selection);
			drawing_area.SetQuery (pane, context.Query);
			drawing_area.SetTextMode (pane, context.LargeTextDisplay);
			
			if (pane == CurrentPane) {
				resultsWindow.Context = context;
			}
		}

		public void ClearPane (Pane pane)
		{
			drawing_area.SetPaneObject (pane, null);
			drawing_area.SetQuery (pane, "");
		}
		
		public event DoEventKeyDelegate KeyPressEvent;
	}
}
