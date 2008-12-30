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

using Docky.Utilities;
using Docky.XLib;

using Do.Universe;
using Do.Platform;
using Do.Interface;
using Do.Interface.CairoUtils;

namespace Docky.Interface
{
	
	
	public class DockWindow : Gtk.Window, IDoWindow
	{
		DockArea dock_area;
		EventBox eb;
		IDoController controller;
		int current_offset;
		
		public new string Name {
			get { return "Docky"; }
		}
		
		public int CurrentOffsetMask {
			get { return current_offset; }
		}
		
		public IDoController Controller {
			get { return controller; }
		}
		
		public DockWindow () : base (Gtk.WindowType.Toplevel)
		{
		}
		
		public void Initialize (IDoController controller)
		{
			this.controller = controller;
			controller.Orientation = ControlOrientation.Horizontal;
			
			AppPaintable = true;
			Decorated = false;
			SkipPagerHint = true;
			SkipTaskbarHint = true;
			Resizable = false;
			CanFocus = false;
			TypeHint = WindowTypeHint.Dock;
			
			this.SetCompositeColormap ();
			
			Realized += (o, a) => GdkWindow.SetBackPixmap (null, false);
			
			StyleSet += (o, a) => {
				if (IsRealized)
					GdkWindow.SetBackPixmap (null, false);
			};
			
			Build ();
		}
		
		void Build ()
		{
			eb = new EventBox ();
			eb.HeightRequest = 1;
			eb.AddEvents ((int) Gdk.EventMask.PointerMotionMask);
			
			eb.MotionNotifyEvent += (o, a) => OnEventBoxMotion ();
			eb.DragMotion += (o, a) => OnEventBoxMotion ();
			
			TargetEntry dest_te = new TargetEntry ("text/uri-list", 0, 0);
			Gtk.Drag.DestSet (eb, DestDefaults.Motion | DestDefaults.Drop, new [] {dest_te}, Gdk.DragAction.Copy);
			
			eb.ExposeEvent += delegate(object o, ExposeEventArgs args) {
				using (Context cr = CairoHelper.Create (eb.GdkWindow)) {
					cr.AlphaFill ();
				}
			};
			
			dock_area = new DockArea (this);
			
			VBox vbox = new VBox ();
			vbox.PackStart (eb, false, false, 0);
			vbox.PackStart (dock_area, false, true, 0);
			Add (vbox);
			ShowAll ();
		}
		
		void OnEventBoxMotion ()
		{
			Reposition ();
			SetInputMask (2);
			Gtk.Application.Invoke ((o, a) => dock_area.ManualCursorUpdate ());
		}
		
		public void SetInputMask (int heightOffset)
		{
			if (!IsRealized || current_offset == heightOffset)
				return;
			
			current_offset = heightOffset;
			int width = Math.Max (Math.Min (800, dock_area.Width), dock_area.DockWidth);
			Gdk.Pixmap pixmap = new Gdk.Pixmap (null, width, heightOffset, 1);
			Context cr = Gdk.CairoHelper.Create (pixmap);
			
			cr.Color = new Cairo.Color (0, 0, 0, 1);
			cr.Paint ();
			
			InputShapeCombineMask (pixmap, (dock_area.Width - width) / 2, eb.HeightRequest + dock_area.Height - heightOffset);
			
			(cr as IDisposable).Dispose ();
			pixmap.Dispose ();
			
			if (heightOffset == 1) {
				GLib.Timeout.Add (1000, () => {
					if (current_offset == 1)
						HideReposition ();
					return false;
				});
			}
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
			if (dock_area.InputInterfaceVisible)
				KeyPressEvent (evnt);
			return base.OnKeyPressEvent (evnt);
		}

		
		protected override void OnShown ()
		{
			base.OnShown ();
			Reposition ();
			
			SetStruts ();
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
		
		void HideReposition ()
		{
			Gdk.Rectangle geo, main;
			
			GetSize (out main.Width, out main.Height);
			geo = Screen.GetMonitorGeometry (0);
			Move (((geo.X+geo.Width)/2) - main.Width/2, geo.Y+geo.Height-eb.HeightRequest);
			
			InputShapeCombineMask (null, 0, 0);
		}
		
		public void RequestClickOff ()
		{
			Controller.ButtonPressOffWindow ();
		}
		
		public void SetStruts ()
		{
			IntPtr display = Xlib.gdk_x11_drawable_get_xdisplay (GdkWindow.Handle);
			X11Atoms atoms = new X11Atoms (display);
			uint[] struts = new uint[12];
			
			struts[(int) XLib.Struts.Bottom] = (uint) dock_area.DockHeight;
			
			if (!IsRealized)
				return;
			
			Xlib.XChangeProperty (display, Xlib.gdk_x11_drawable_get_xid (GdkWindow.Handle), atoms._NET_WM_STRUT, 
			                      atoms.XA_CARDINAL, 32, (int) XLib.PropertyMode.PropModeReplace, struts, 4);
		}

		#region IDoWindow implementation 
		
		public new event Do.Interface.DoEventKeyDelegate KeyPressEvent;
		
		public void Summon ()
		{
			Reposition ();
			Do.Interface.Windowing.PresentWindow (this);
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
		}
		
		public void ShrinkResults ()
		{
		}
		
		public void SetPaneContext (Pane pane, IUIContext context)
		{
			dock_area.SetPaneContext (context, pane);
		}
		
		public void ClearPane (Pane pane)
		{
			dock_area.ClearPane (pane);
		}
		
		public new bool Visible {
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
		
		public bool ResultsCanHide { 
			get { return false; } 
		}
		
		#endregion 
		

	}
}
