// Bezel.cs
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
using System.Collections.Generic;

using Cairo;
using Gdk;
using Gtk;

using Do.Addins;
using Do.Universe;

namespace Do.UI
{
	
	
	public class Bezel : Gtk.Window, IDoWindow
	{
		BezelDrawingArea bezel_drawing_area;
		BezelGlassResults bezel_glass_results;
		BezelGlassWindow bezel_glass_window;
		IDoController controller;
		PositionWindow pw;
		
		public Pane CurrentPane {
			get { return bezel_drawing_area.Focus; }
			set { bezel_drawing_area.Focus = value; }
		}
		
		public Bezel(IDoController controller, IRenderTheme theme) : base (Gtk.WindowType.Toplevel)
		{
			this.controller = controller;
			Build (theme);
		}
		
		void Build (IRenderTheme theme)
		{
			Decorated = false;
			AppPaintable = true;
			KeepAbove = true;
			
			TypeHint = WindowTypeHint.Splashscreen;
			SetColormap ();
			
			bezel_drawing_area = new BezelDrawingArea (controller, theme, false);
			bezel_drawing_area.Show ();
			
			bezel_glass_results = bezel_drawing_area.Results;
			bezel_glass_window = new BezelGlassWindow (bezel_glass_results);
	
			Add (bezel_drawing_area);
			
			pw = new PositionWindow (this, bezel_glass_window);
		}
		
		protected override void OnDestroyed ()
		{
			base.OnDestroyed ();
			bezel_drawing_area.Destroy ();
			bezel_drawing_area = null;
			bezel_glass_results.Destroy ();
			bezel_glass_results = null;
		}

		protected override bool OnButtonPressEvent (EventButton evnt)
		{
			Gdk.Point global_point = new Gdk.Point ((int) evnt.XRoot, (int) evnt.YRoot);
			Gdk.Point local_point = new Gdk.Point ((int) evnt.X, (int) evnt.Y);
			
			switch (bezel_drawing_area.GetPointLocation (local_point)) {
			case PointLocation.Close:
			case PointLocation.Outside:
				controller.ButtonPressOffWindow ();
				break;
			case PointLocation.Preferences:
				Addins.Util.Appearance.PopupMainMenuAtPosition (global_point.X, global_point.Y);
//				// Have to re-grab the pane from the menu.
				Addins.Util.Appearance.PresentWindow (this);
				break;
			}

			return base.OnButtonPressEvent (evnt);
		}
		
		protected override bool OnKeyPressEvent (EventKey evnt)
		{
			KeyPressEvent (evnt);

			return base.OnKeyPressEvent (evnt);
		}
		
		protected override bool OnExposeEvent (EventExpose evnt)
		{
			if (IsDrawable) {
				Cairo.Context cr = Gdk.CairoHelper.Create (GdkWindow);
				cr.Operator = Cairo.Operator.Source;
				cr.Paint ();
				(cr as IDisposable).Dispose ();
			}
			return base.OnExposeEvent (evnt);
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

		public void Summon ()
		{
			int width, height;
			GetSize (out width, out height);
			
			pw.UpdatePosition (0, Pane.First, new Gdk.Rectangle (((int)(bezel_drawing_area.WindowWidth-bezel_glass_results.WidthRequest)/2), -10, 0, 0));
			Show ();
			bezel_glass_window.Show ();
			Util.Appearance.PresentWindow (this);
		}

		public void Vanish ()
		{
			Hide ();
			bezel_glass_window.Hide ();
		}

		public void Reset ()
		{
			bezel_drawing_area.Clear ();
			bezel_glass_results.Clear ();
		}

		public void Grow ()
		{
			bezel_drawing_area.ThirdPaneVisible = true;
		}

		public void Shrink ()
		{
			bezel_drawing_area.ThirdPaneVisible = false;
		}

		public void GrowResults ()
		{
			bezel_glass_results.SlideIn ();
		}

		public void ShrinkResults ()
		{
			bezel_glass_results.SlideOut ();
		}

		public void SetPaneContext (Pane pane, IUIContext context)
		{
			// This prevents the odd situation of nothing drawing in the third pane.  Ultimately what has
			// happened is the universe has "nulled" the pane by fluke.  We detect this and replace the
			// query with an invisible space.
			string query;
			if (pane == Pane.Third && context.Selection == null && string.IsNullOrEmpty (context.Query) && context.Results.Length == 0) {
				query = " ";
			} else {
				query = context.Query;
			}
			bezel_drawing_area.BezelSetPaneObject (pane, context.Selection);
			bezel_drawing_area.BezelSetQuery      (pane, query);
			bezel_drawing_area.BezelSetTextMode   (pane, context.LargeTextDisplay);
			bezel_drawing_area.BezelSetEntryMode (pane, context.LargeTextModeType == TextModeType.Explicit);
			
			if (CurrentPane == pane) {
				bezel_glass_results.Context = context;
			}
		}

		public void ClearPane (Pane pane)
		{
			bezel_drawing_area.BezelSetPaneObject (pane, null);
			bezel_drawing_area.BezelSetQuery (pane, string.Empty);
			bezel_drawing_area.BezelSetEntryMode (pane, false);
			
			if (pane == CurrentPane) {
				bezel_glass_results.Clear ();
			}
		}

		
		public new event DoEventKeyDelegate KeyPressEvent;
	}
}
