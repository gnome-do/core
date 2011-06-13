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
using System.Linq;

using Cairo;
using Gdk;
using Gtk;

using Do.Universe;
using Do.Platform;
using Do.Platform.Linux;
using Do.Interface;

namespace Do.Interface.AnimationBase
{
	
	
	public abstract class AbstractAnimatedInterface : Gtk.Window, IDoWindow, IConfigurable
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

		protected abstract IRenderTheme RenderTheme { get; }
		
		public AbstractAnimatedInterface () : base (Gtk.WindowType.Toplevel)
		{
		}
		
		public void Initialize (IDoController controller)
		{
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
			
			bezel_drawing_area = new BezelDrawingArea (controller, RenderTheme, false);
			bezel_drawing_area.Show ();
			
			bezel_glass_results = bezel_drawing_area.Results;
			bezel_glass_window = new BezelGlassWindow (bezel_glass_results);
	
			Add (bezel_drawing_area);
			
			pw = new PositionWindow (this, bezel_glass_window);
			
			Realized += delegate {
				GdkWindow.SetBackPixmap (null, false);
				GdkWindow.OverrideRedirect = true;
			};
			
			StyleSet += delegate {
				if (IsRealized)
					GdkWindow.SetBackPixmap (null, false);
			};
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
			Gdk.Point global_point = new Gdk.Point ((int)evnt.XRoot, (int)evnt.YRoot);
			Gdk.Point local_point = new Gdk.Point ((int)evnt.X, (int)evnt.Y);
			
			switch (bezel_drawing_area.GetPointLocation (local_point)) {
				case PointLocation.Close:
				case PointLocation.Outside:
					controller.ButtonPressOffWindow ();
					break;
				case PointLocation.Preferences:
					// We need to let the window manager handle the Do window before popping up the
					// preferences menu so that it can place the menu over the top.
					GdkWindow.OverrideRedirect = false;
					Services.Windowing.ShowMainMenu (global_point.X, global_point.Y);

					// Have to re-grab the pane from the menu.
					Interface.Windowing.PresentWindow (this);
					GdkWindow.OverrideRedirect = true;
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
			
			pw.UpdatePosition (0, Pane.First, new Gdk.Rectangle (((int) (bezel_drawing_area.WindowWidth - bezel_glass_results.WidthRequest) / 2), -10, 0, 0));
			Show ();
			bezel_glass_window.Show ();
			Interface.Windowing.PresentWindow (this);
		}

		public void Vanish ()
		{
			Interface.Windowing.UnpresentWindow (this);
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
			if (pane == Pane.Third && context.Selection == null && string.IsNullOrEmpty (context.Query) && !context.Results.Any ()) {
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
			bezel_drawing_area.BezelSetQuery (pane, "");
			bezel_drawing_area.BezelSetEntryMode (pane, false);
			
			if (pane == CurrentPane) {
				bezel_glass_results.Clear ();
			}
		}

		#region IConfigurable implementation
		public Bin GetConfiguration ()
		{
			return new AnimationBaseConfigurationWidget (bezel_drawing_area);
		}
		
		public string Description {
			get {
				return "Animated Interface Configuration";
			}
		}
		
		public new string Icon {
			get {
				return "preferences";
			}
		}

		// This must be an explicit interface method to disambiguate between
		// Widget.Name and IConfigurable.Name
		string IConfigurable.Name {
			get { return RenderTheme.Name; }
		}
		#endregion


		public bool ResultsCanHide { get { return true; } }
		
		public new event DoEventKeyDelegate KeyPressEvent;
	}
}
