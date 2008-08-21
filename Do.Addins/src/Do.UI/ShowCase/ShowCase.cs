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
using System.Collections.Generic;

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
		IDoController controller;
		Dictionary<string, Surface> surface_buffer;
		int results_offset = 50;

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
			surface_buffer = new Dictionary<string,Surface> ();
			
			this.controller = controller;
			Build ();
			DrawingContext = new ShowCaseDrawingContext (null, null, null, Pane.First);
		}

		private void Build ()
		{
			Decorated = false;
			AppPaintable = true;
			KeepAbove = true;
			
			TypeHint = WindowTypeHint.Splashscreen;
			BorderWidth = 15;
			
			SetColormap ();
			
			VBox vbox = new VBox ();
			drawing_area = new ShowCaseDrawingArea (500, 200);
			vbox.PackStart (drawing_area, false, false, 0);
			
			HBox hbox = new HBox ();
			resultsWindow = new ShowCaseResultsWidget ();
			hbox.PackStart (resultsWindow, true, true, (uint) results_offset);
			vbox.PackStart (hbox, true, true, 0);
			
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
			
			using (cairo = Gdk.CairoHelper.Create (GdkWindow)) {
				Gdk.Rectangle rect = new Gdk.Rectangle ();
				GetSize (out rect.Width, out rect.Height);
				
				cairo.SetSource (GetSurfaceForSize (rect.Width, rect.Height));
				cairo.Operator = Cairo.Operator.Source;
				cairo.Paint ();
			}
			return base.OnExposeEvent (evnt);
		}
		
		private Surface GetSurfaceForSize (int width, int height)
		{
			string uid = width.ToString () + ":" + height.ToString ();
			if (!surface_buffer.ContainsKey (uid)) {
				int radius = 5;
				double x=.5, y=.5;
				
				Cairo.Context cr = CairoHelper.Create (GdkWindow);
				Cairo.Surface surface = cr.Target.CreateSimilar (cr.Target.Content, width, height);
				(cr as IDisposable).Dispose ();
				
				cr = new Context (surface);
				
				cr.Rectangle (0, 0, width, height);
				cr.Color = new Cairo.Color (0, 0, 0, 0);
				cr.Operator = Cairo.Operator.Source;
				cr.Paint ();
				
				cr.Operator = Cairo.Operator.Over;
				
				width--;
				height--;
				
				cr.NewPath ();
				if (!resultsWindow.Visible) {
					cr.MoveTo (x+radius, y);
					cr.Arc (x+width-radius, y+radius, radius, (Math.PI*1.5), (Math.PI*2));
					cr.Arc (x+width-radius, y+height-radius, radius, 0, (Math.PI*0.5));
					cr.Arc (x+radius, y+height-radius, radius, (Math.PI*0.5), Math.PI);
					cr.Arc (x+radius, y+radius, radius, Math.PI, (Math.PI*1.5));
				} else {
					int offset_line = 200 + 30 - 1;
					cr.MoveTo (x+radius, y);
					cr.Arc (x+width-radius, y+radius, radius, (Math.PI*1.5), (Math.PI*2));
					cr.Arc (x+width-radius, y+offset_line-radius, radius, 0, (Math.PI*0.5));
					cr.ArcNegative (x+width-results_offset+radius, y+offset_line+radius, radius, Math.PI*1.5, Math.PI);
					cr.Arc (x+width-results_offset-radius, y+height-radius, radius, 0, (Math.PI*0.5));
					cr.Arc (x+results_offset+radius, y+height-radius, radius, (Math.PI*0.5), Math.PI);
					cr.ArcNegative (x+results_offset-radius, y+offset_line+radius, radius, 0, Math.PI*1.5);
					cr.Arc (x+radius, y+offset_line-radius, radius, (Math.PI*0.5), Math.PI);
					cr.Arc (x+radius, y+radius, radius, Math.PI, (Math.PI*1.5));
					cr.ClosePath ();
				}
					
				Cairo.LinearGradient pattern = new LinearGradient (0, 0, 0, height);
				pattern.AddColorStop (0, new Cairo.Color (1, 1, 1, .85));
				pattern.AddColorStop (1, new Cairo.Color (.6, .6, .6, .85));
				cr.Pattern = pattern;
				
				cr.FillPreserve ();
				
				pattern = new LinearGradient (0, 0, width, 230);
				pattern.AddColorStop (0.0, new Cairo.Color (0.0, 0.0, 0.0, .75));
				pattern.AddColorStop (0.2, new Cairo.Color (0.2, 0.2, 0.2, .50));
				pattern.AddColorStop (0.8, new Cairo.Color (0.2, 0.2, 0.2, .50));
				pattern.AddColorStop (1.0, new Cairo.Color (0.0, 0.0, 0.0, .75));
				
				cr.Pattern = pattern;
				cr.LineWidth = 1;
				cr.Stroke ();
				
				cr.NewPath ();
				cr.MoveTo (width - 26, 5);
				cr.LineTo (width - 14, 5);
				cr.LineTo (width - 20, 11);
				cr.ClosePath ();
				cr.Color = new Cairo.Color (0.3, 0.3, 0.3);
				cr.Fill ();
				
				(cr as IDisposable).Dispose ();
				surface_buffer[uid] = surface;
			}
			return surface_buffer[uid];
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
			QueueDraw ();
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
		
		public new event DoEventKeyDelegate KeyPressEvent;
	}
}
