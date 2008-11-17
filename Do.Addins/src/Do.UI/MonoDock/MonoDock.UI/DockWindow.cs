// DockWindow.cs
// 
// Copyright (C) 2008 GNOME Do
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

using Gdk;
using Gtk;
using Cairo;

using MonoDock.Util;
using MonoDock.XLib;

using Do.Addins;
using Do.UI;
using Do.Universe;

namespace MonoDock.UI
{
	
	
	public class DockWindow : Gtk.Window, IDoWindow
	{
		DockArea dock_area;
		IDoController controller;
		
		public IDoController Controller {
			get { return controller; }
		}
		
		public DockWindow(IDoController controller) : base (Gtk.WindowType.Toplevel)
		{
			this.controller = controller;
			
			AppPaintable = true;
			KeepAbove = true;
			Decorated = false;
			SkipPagerHint = true;
			SkipTaskbarHint = true;
			Resizable = false;
			CanFocus = false;
			TypeHint = WindowTypeHint.Dock;
			
			this.SetCompositeColormap ();
			
			Realized += delegate {
				GdkWindow.SetBackPixmap (null, false);
			};
			
			StyleSet += delegate {
				if (IsRealized)
					GdkWindow.SetBackPixmap (null, false);
			};
			
			Build ();
		}
		
		void Build ()
		{
			dock_area = new DockArea (this, Controller.Statistics);
			
			Add (dock_area);
			ShowAll ();
		}
		
		public void SetInputMask (int heightOffset)
		{
			Gdk.Pixmap pixmap = new Gdk.Pixmap (null, dock_area.Width, dock_area.Height-heightOffset, 1);
			Context cr = Gdk.CairoHelper.Create (pixmap);
			
			cr.Color = new Cairo.Color (0, 0, 0, 1);
			cr.Paint ();
			
			InputShapeCombineMask (pixmap, 0, heightOffset);
			
			(cr as IDisposable).Dispose ();
			pixmap.Dispose ();
		}
		
		protected override bool OnButtonReleaseEvent (Gdk.EventButton evnt)
		{
			Gdk.Rectangle rect;
			GetSize (out rect.Width, out rect.Height);
			GetPosition (out rect.X, out rect.Y);
			
			if (!rect.Contains ((int) evnt.XRoot, (int) evnt.YRoot) && dock_area.InputInterfaceVisible) {
				controller.ButtonPressOffWindow ();
			}
			
			return base.OnButtonReleaseEvent (evnt);
		}
		
		protected override bool OnKeyPressEvent (Gdk.EventKey evnt)
		{
			KeyPressEvent (evnt);
			return base.OnKeyPressEvent (evnt);
		}

		
		protected override void OnShown ()
		{
			base.OnShown ();
			Reposition ();
			
			IntPtr display = Xlib.gdk_x11_drawable_get_xdisplay (GdkWindow.Handle);
			X11Atoms atoms = new X11Atoms (display);
			uint[] struts = new uint[12];
			
			struts[(int) XLib.Struts.Bottom] = (uint) dock_area.DockHeight;
			
			if (!IsRealized)
				return;
			
			Xlib.XChangeProperty (display, Xlib.gdk_x11_drawable_get_xid (GdkWindow.Handle), atoms._NET_WM_STRUT, 
			                      atoms.XA_CARDINAL, 32, (int) XLib.PropertyMode.PropModeAppend, struts, 4);
		}
		
		protected override void OnSizeAllocated (Gdk.Rectangle allocation)
		{
			base.OnSizeAllocated (allocation);
			
			Reposition ();
		}
		
		void Reposition ()
		{
			Gdk.Rectangle geo, main;
			
			GetSize (out main.Width, out main.Height);
			geo = Screen.GetMonitorGeometry (0);
			Move (((geo.X+geo.Width)/2) - main.Width/2, geo.Y+geo.Height-main.Height);
		}



		#region IDoWindow implementation 
		
		public event Do.Addins.DoEventKeyDelegate KeyPressEvent;
		
		public void Summon ()
		{
			Do.Addins.Util.Appearance.PresentWindow (this);
			if (!dock_area.InputInterfaceVisible)
				dock_area.ShowInputInterface ();
		}
		
		public void Vanish ()
		{
			uint current_time = Gtk.Global.CurrentEventTime;
			Gdk.Pointer.Ungrab (current_time);
			Gdk.Keyboard.Ungrab (current_time);
			if (dock_area.InputInterfaceVisible)
				dock_area.HideInputInterface ();
		}
		
		public void Reset ()
		{
			dock_area.Reset ();
		}
		
		public void Grow ()
		{
			dock_area.ThirdPaneVisible = true;
		}
		
		public void Shrink ()
		{
			dock_area.ThirdPaneVisible = false;
		}
		
		public void GrowResults ()
		{
//			throw new System.NotImplementedException();
		}
		
		public void ShrinkResults ()
		{
//			throw new System.NotImplementedException();
		}
		
		public void SetPaneContext (Pane pane, IUIContext context)
		{
			dock_area.SetPaneContext (context, pane);
		}
		
		public void ClearPane (Pane pane)
		{
//			throw new System.NotImplementedException();
		}
		
		public bool Visible {
			get {
				return dock_area.InputInterfaceVisible;
			}
		}
		
		public Pane CurrentPane {
			get {
				return dock_area.CurrentPane;
			}
			set {
				dock_area.CurrentPane = value;
			}
		}
		
		#endregion 
		

	}
}
