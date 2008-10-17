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
		BezelDrawingArea bda;
		BezelGlassResults bgr;
		BezelGlassWindow bgw;
		IDoController controller;
		PositionWindow pw;
		HUDStyle style;
		
		public Pane CurrentPane {
			get { return bda.Focus; }
			set { bda.Focus = value; }
		}
		
		public Bezel(IDoController controller, HUDStyle style) : base (Gtk.WindowType.Toplevel)
		{
			this.style = style;
			this.controller = controller;
			Build ();
		}
		
		void Build ()
		{
			Decorated = false;
			AppPaintable = true;
			KeepAbove = true;
			
			TypeHint = WindowTypeHint.Splashscreen;
			SetColormap ();
			
			VBox vbox = new VBox ();
			
			bda = new BezelDrawingArea (style, false);
			vbox.PackStart (bda, true, true, 0);
			bda.Show ();
			
			bgr = bda.Results;
			bgw = new BezelGlassWindow (bgr);
	
			Add (vbox);
			vbox.Show ();
			
			pw = new PositionWindow (this, bgw);
		}
		
		protected override void OnDestroyed ()
		{
			base.OnDestroyed ();
			bda.Destroy ();
			bda = null;
			bgr.Destroy ();
			bgr = null;
		}

		protected override bool OnButtonPressEvent (EventButton evnt)
		{
			Gdk.Point global_point = new Gdk.Point ((int) evnt.XRoot, (int) evnt.YRoot);
			Gdk.Point local_point = new Gdk.Point ((int) evnt.X, (int) evnt.Y);
			
			switch (bda.GetPointLocation (local_point)) {
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
			
			pw.UpdatePosition (0, Pane.First, new Gdk.Rectangle (((int)(bda.WindowWidth-bgr.WidthRequest)/2), -10, 0, 0));
			Show ();
			bgw.Show ();
			Util.Appearance.PresentWindow (this);
		}

		public void Vanish ()
		{
			Hide ();
			bgw.Hide ();
		}

		public void Reset ()
		{
			bda.Clear ();
			bgr.Clear ();
		}

		public void Grow ()
		{
			bda.ThirdPaneVisible = true;
		}

		public void Shrink ()
		{
			bda.ThirdPaneVisible = false;
		}

		public void GrowResults ()
		{
			bgr.SlideIn ();
		}

		public void ShrinkResults ()
		{
			bgr.SlideOut ();
		}

		public void SetPaneContext (Pane pane, IUIContext context)
		{
			bda.BezelSetPaneObject (pane, context.Selection);
			bda.BezelSetQuery      (pane, context.Query);
			bda.BezelSetTextMode   (pane, context.LargeTextDisplay);
			bda.BezelSetEntryMode (pane, context.LargeTextModeType == TextModeType.Explicit);
			
//			bda.Draw ();
			
			if (CurrentPane == pane) {
				bgr.Context = context;
			}
		}

		public void ClearPane (Pane pane)
		{
			bda.BezelSetPaneObject (pane, null);
			bda.BezelSetQuery (pane, string.Empty);
			bda.BezelSetEntryMode (pane, false);
			
//			bda.Draw ();
			if (pane == CurrentPane) {
				bgr.Clear ();
			}
		}

		
		public new event DoEventKeyDelegate KeyPressEvent;
	}
}
