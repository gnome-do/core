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

using Do.Universe;
using Do.Platform;
using Do.Platform.Linux;
using Do.Interface;
using Do.Interface.Xlib;
using Do.Interface.CairoUtils;
using Do.Interface.AnimationBase;

namespace Docky.Interface
{
	
	
	public class DockWindow : Gtk.Window, IDoWindow, IConfigurable
	{
		public static Gtk.Window Window { get; private set; }
		
		BezelGlassResults results;
		BezelGlassWindow results_window;
		
		DockArea dock_area;
		Interface.DoInteropService interop_service;
		IDoController controller;
		Gdk.Rectangle current_mask;
		uint strut_timer;
		bool is_repositioned_hidden;
		bool presented;
		int buffer_x, buffer_y;
		int buffer_width, buffer_height;
		
		public new string Name {
			get { return "Docky"; }
		}

		public bool IsRepositionHidden {
			get { return is_repositioned_hidden; }
		}
		
		public IDoController Controller {
			get { return controller; }
		}
		
		public DockWindow () : base (Gtk.WindowType.Toplevel)
		{
			Window = this;
		}
		
		public void Initialize (IDoController controller)
		{
			this.controller = controller;
			controller.Orientation = ControlOrientation.Vertical;

			interop_service = new DoInteropService (controller);
			Core.DockServices.RegisterService (interop_service);
			
			Core.DockServices.PainterService.RegisterPainter (new Painters.SummonModeRenderer ());
			
			RegisterEvents ();
			Build ();
		}
		
		void Build ()
		{
			AppPaintable = true;
			Decorated = false;
			SkipPagerHint = true;
			SkipTaskbarHint = true;
			Resizable = false;
			CanFocus = false;
			TypeHint = WindowTypeHint.Dock;
			
			this.SetCompositeColormap ();
			
			dock_area = new DockArea (this);
			Add (dock_area);

			results = new BezelGlassResults (controller, 450, HUDStyle.Classic, new BezelColors (new Cairo.Color (.1, .1, .1, .8)));
			results.SlideFromBottom = DockPreferences.Orientation == DockOrientation.Bottom;
			results_window = new BezelGlassWindow (results);

			ShowAll ();
			Stick ();
		}

		void RegisterEvents ()
		{
			Realized += (o, a) => GdkWindow.SetBackPixmap (null, false);
	
			StyleSet += HandleStyleSet;
			
			DockPreferences.AllowOverlapChanged += DelaySetStruts;
			DockPreferences.AutohideChanged += DelaySetStruts;
			DockPreferences.MonitorChanged += HandleMonitorChanged;
		}

		void UnregisterEvents ()
		{
			StyleSet -= HandleStyleSet;
			
			DockPreferences.AllowOverlapChanged -= DelaySetStruts;
			DockPreferences.AutohideChanged -= DelaySetStruts;
			DockPreferences.MonitorChanged -= HandleMonitorChanged;

			if (strut_timer > 0)
				GLib.Source.Remove (strut_timer);
		}

		void HandleMonitorChanged()
		{
			// bring us back down to "minimum" size
			Resize (1, 1);
			DelaySetStruts ();
		}
		
		void HandleStyleSet(object o, StyleSetArgs args)
		{
			if (!IsRealized) return;
			
			GdkWindow.SetBackPixmap (null, false);
			
			Gdk.Rectangle tmp = current_mask;
			current_mask = Gdk.Rectangle.Zero;
			
			SetInputMask (tmp);
		}
		
		public void SetInputMask (Gdk.Rectangle area)
		{
			if (!IsRealized || current_mask == area)
				return;

			current_mask = area;
			if (area.Width == 0 || area.Height == 0) {
				InputShapeCombineMask (null, 0, 0);
				return;
			}

			Gdk.Pixmap pixmap = new Gdk.Pixmap (null, area.Width, area.Height, 1);
			Context cr = Gdk.CairoHelper.Create (pixmap);
			
			cr.Color = new Cairo.Color (0, 0, 0, 1);
			cr.Paint ();

			InputShapeCombineMask (pixmap, area.X, area.Y);
			
			(cr as IDisposable).Dispose ();
			pixmap.Dispose ();
			
			if (area.Height == 1) {
				GLib.Timeout.Add (500, () => {
					if (current_mask.Height == 1)
						HideReposition ();
					return false;
				});
			} else {
				if (is_repositioned_hidden)
					Reposition ();
			}
		}
		
		protected override bool OnButtonReleaseEvent (Gdk.EventButton evnt)
		{
			Gdk.Rectangle rect;
			GetSize (out rect.Width, out rect.Height);
			GetPosition (out rect.X, out rect.Y);
			
			if (!rect.Contains ((int) evnt.XRoot, (int) evnt.YRoot)) {
				dock_area.ProxyButtonReleaseEvent (evnt);
			}
			
			return base.OnButtonReleaseEvent (evnt);
		}
		
		protected override bool OnConfigureEvent (Gdk.EventConfigure evnt)
		{
			buffer_x = evnt.X;
			buffer_y = evnt.Y;
			return base.OnConfigureEvent (evnt);
		}

		
		protected override bool OnKeyPressEvent (Gdk.EventKey evnt)
		{
			if (Visible)
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
			buffer_width = allocation.Width;
			buffer_height = allocation.Height;
			
			base.OnSizeAllocated (allocation);
			Reposition ();
		}
		
		public void Reposition ()
		{
			Gdk.Rectangle geo, main, res;
			
			GetSize (out main.Width, out main.Height);
			results_window.GetSize (out res.Width, out res.Height);
			geo = LayoutUtils.MonitorGemonetry ();

			switch (DockPreferences.Orientation) {
			case DockOrientation.Bottom:
				Move ((geo.X + geo.Width / 2) - main.Width / 2, geo.Y + geo.Height - main.Height);
				results_window.Move ((geo.X + geo.Width / 2) - res.Width / 2, geo.Y + geo.Height - dock_area.DockHeight - res.Height);
				break;
			case DockOrientation.Top:
				Move (geo.X, geo.Y);
				results_window.Move ((geo.X + geo.Width / 2) - res.Width / 2, geo.Y + dock_area.DockHeight);
				break;
			}
			
			if (Display != null)
				Display.Sync ();
			
			results.SlideFromBottom = DockPreferences.Orientation == DockOrientation.Bottom;
			is_repositioned_hidden = false;
		}
		
		void HideReposition ()
		{
			Gdk.Rectangle geo, main;
			
			GetSize (out main.Width, out main.Height);
			geo = LayoutUtils.MonitorGemonetry ();

			switch (DockPreferences.Orientation) {
			case DockOrientation.Bottom:
				Move ((geo.X + geo.Width / 2) - main.Width / 2, geo.Y + geo.Height);
				break;
			case DockOrientation.Top:
				Move (geo.X, geo.Y - main.Height);
				break;
			}

			if (Display != null)
				Display.Sync ();
			
			is_repositioned_hidden = true;
		}
		
		public void WindowHideOffset (out int x, out int y)
		{
			x = y = 0;

			Gdk.Rectangle main, geo;
			main.Width = dock_area.Width;
			main.Height = dock_area.Height;
			GetBufferedPosition (out main.X, out main.Y);
			geo = LayoutUtils.MonitorGemonetry ();
			
			
			switch (DockPreferences.Orientation) {
			case DockOrientation.Bottom:
				y = main.Y - ((geo.Y + geo.Height) - main.Height);
				return;
			case DockOrientation.Top:
				y = main.Y - geo.Y;
				return;
			}
		}
				
		public void GetBufferedPosition (out int x, out int y)
		{
			if (buffer_x == 0 && buffer_y == 0)
				GetPosition (out buffer_x, out buffer_y);
			x = buffer_x;
			y = buffer_y;
		}
		
		public void GetBufferedSize (out int width, out int height)
		{
			if (buffer_width == 0 && buffer_height == 0)
				GetSize (out buffer_width, out buffer_height);
			width = buffer_width;
			height = buffer_height;
		}
		
		public void DelaySetStruts ()
		{
			if (strut_timer > 0)
				return;
			
			strut_timer = GLib.Timeout.Add (250, SetStruts);
		}
		
		public bool SetStruts ()
		{
			X11Atoms atoms = X11Atoms.Instance;

			uint [] struts = dock_area.StrutRequest;
			uint [] first_struts = new [] { struts [0], struts [1], struts [2], struts [3] };

			strut_timer = 0;
			
			if (!IsRealized)
				return false;
			Xlib.XChangeProperty (GdkWindow, atoms._NET_WM_STRUT_PARTIAL, atoms.XA_CARDINAL,
			                      (int) PropertyMode.PropModeReplace, struts);
			
			Xlib.XChangeProperty (GdkWindow, atoms._NET_WM_STRUT, atoms.XA_CARDINAL, 
			                      (int) PropertyMode.PropModeReplace, first_struts);
				
			return false;
		}

		public void PresentWindow ()
		{
			if (!presented)
				Windowing.PresentWindow (this);
			
			presented = true;
		}
		
		public void UnpresentWindow ()
		{
			if (presented)
				Windowing.UnpresentWindow (this);
			
			presented = false;
		}
		
		#region IDoWindow implementation 
		
		public new event DoEventKeyDelegate KeyPressEvent;
		
		public void Summon ()
		{
			Visible = true;
			results_window.Show ();
			Reposition ();
			PresentWindow ();
			interop_service.SignalSummon ();
		}
		
		public void Vanish ()
		{
			Visible = false;
			UnpresentWindow ();
			results_window.Hide ();
			interop_service.SignalVanish ();
		}
		
		public void Reset ()
		{
			DockState.Instance.Clear ();
			interop_service.SignalReset ();
		}
		
		public void Grow ()
		{
			DockState.Instance.ThirdPaneVisible = true;
		}
		
		public void Shrink ()
		{
			DockState.Instance.ThirdPaneVisible = false;
		}
		
		public void GrowResults ()
		{
			results.SlideIn ();
			interop_service.SignalResultsGrow ();
		}
		
		public void ShrinkResults ()
		{
			results.SlideOut ();
			interop_service.SignalResultsShrink ();
			
		}
		
		public void SetPaneContext (Pane pane, IUIContext context)
		{
			DockState.Instance.SetContext (context, pane);
			if (CurrentPane == pane) {
				results.Context = context;
			}
		}
		
		public void ClearPane (Pane pane)
		{
			DockState.Instance.ClearPane (pane);
		}
		
		public new bool Visible {
			get; private set;
		}
		
		public Pane CurrentPane {
			get { return DockState.Instance.CurrentPane; }
			set { DockState.Instance.CurrentPane = value; }
		}
		
		public bool ResultsCanHide { 
			get { return true; } 
		}
		
		public override void Dispose ()
		{
			Window = null;
			UnregisterEvents ();

			Remove (dock_area);
			dock_area.Dispose ();
			dock_area.Destroy ();
			dock_area = null;
			
			Core.DockServices.Clean ();
			
			Destroy ();
			base.Dispose ();
		}


		#endregion 

		#region IConfigurable implementation
		public Bin GetConfiguration ()
		{
			return new DockyConfigurationWidget ();
		}
		
		public string Description {
			get {
				return "Docky configuration";
			}
		}
		
		public new string Icon {
			get {
				return "panel";
			}
		}
		#endregion
	}
}
