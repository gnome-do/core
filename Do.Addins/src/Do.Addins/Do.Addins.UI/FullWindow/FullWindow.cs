// FullWindow.cs
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
using Gdk;
using Gtk;

namespace Do.Addins.UI
{
	
	public class FullWindow : Gtk.Window, IDoWindow
	{
		//===============================================//
		//------------------Class Members----------------//
		//===============================================//
		
		/// <value>
		/// Used to store a reference to our own controller
		/// </value>
		private IDoController controller;
		
		/// <value>
		/// store internal reference to our current pane.  Controller will query this
		/// </value>
		private Pane currentPane;
		
		/// <value>
		/// Our first Gtk.Bin object
		/// </value>
		private Frame frame;
		
		private bool summonable;
		
		//===============================================//
		//-----------------Properties--------------------//
		//===============================================//
		public bool IsSummonable {
			get {
				return summonable;
			}
		}

		public Pane CurrentPane {
			get {
				return currentPane;
			}
			set {
				currentPane = value;
			}
		}
		
		//===========================================//
		//------------------ctor---------------------//
		//===========================================//
		public FullWindow(IDoController controller) : base (Gtk.WindowType.Toplevel)
		{
			summonable = false;
			Build ();
		}
		
		//===========================================//
		//------------------methods------------------//
		//===========================================//
		private void Build ()
		{
			Decorated = false;
			AppPaintable = true;
			KeepAbove = true;
			
			TypeHint = WindowTypeHint.Splashscreen;
			
			frame = new Frame ();
			frame.Radius = 0;
			frame.FillAlpha = frame.FrameAlpha = .9;
			frame.FillColor = frame.FrameColor = new Color (0, 226, 242);
			frame.DrawFill = frame.DrawFrame = true;
			
			Label label = new Label ("It's About To Go Down...");
			
			Add (frame);
			frame.Add (label);
			
			frame.ShowAll ();
			
			SetColormap ();
			
			summonable = true;
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
		}
		
		///////////////////////////////////
		//////// IDoWindow Members ////////
		///////////////////////////////////

		public void Summon ()
		{
			Show ();
			Fullscreen ();
		}

		public void Vanish ()
		{
			Hide ();
			Unfullscreen ();
		}

		public void Reset ()
		{
		}

		public void Grow ()
		{
		}

		public void Shrink ()
		{
		}

		public void GrowResults ()
		{
		}

		public void ShrinkResults ()
		{
		}

		public void SetPaneContext (Pane pane, Do.Addins.SearchContext context)
		{
		}

		public void ClearPane (Pane pane)
		{
		}

		
		public new event DoEventKeyDelegate KeyPressEvent;
	}
}
