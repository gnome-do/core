// DockArea.cs
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

using Cairo;
using Gdk;
using Gtk;

using Do.Universe;

using MonoDock.Util;

namespace MonoDock.UI
{
	
	
	public class DockArea : Gtk.DrawingArea
	{
		const int BounceTime = 700;
		const int YBuffer = 5;
		const int XBuffer = 5;
		
		IList<DockItem> dock_items;
		Gdk.Point cursor;
		DateTime enter_time = DateTime.Now;
		DateTime last_render = DateTime.Now;
		Gdk.Rectangle minimum_dock_size = new Gdk.Rectangle (-1, -1, -1, -1);
		
		Surface backbuffer;
		DockWindow window;
		
		#region Public properties
		public int Width {
			get {
				return (dock_items.Count * IconSize) + Preferences.ZoomSize + 2*XBuffer;
			}
		}
		
		public int Height {
			get {
				return 2*IconSize + 40;
			}
		}
		
		public int DockHeight {
			get {
				Console.WriteLine (MinimumDockArea.Height);
				return MinimumDockArea.Height;
			}
		}
		#endregion
		
		double ZoomIn {
			get {
				if (!CursorIsOverDockArea)
					return Math.Max (0, 1-DateTime.Now.Subtract (enter_time).TotalMilliseconds/150.0);
				return Math.Min (1, DateTime.Now.Subtract (enter_time).TotalMilliseconds/150.0);
			}
		}
		
		int ZoomPixels {
			get {
				return (int) (ZoomSize*ZoomIn);
			}
		}
		
		int ZoomSize {
			get {
				return Preferences.ZoomSize;
			}
		}
		
		int IconBorderWidth {get{ return 4; }}
		
		int IconSize { get { return DockItem.IconSize + IconBorderWidth; } }
		
		Gdk.Point Cursor {
			get {
				return cursor;
			}
			set {
				bool tmp = CursorIsOverDockArea;
				cursor = value;
				if (CursorIsOverDockArea != tmp) {
					if (CursorIsOverDockArea) {
						window.SetInputMask (0);
					} else {
						window.SetInputMask (Height-IconSize);
					}
					enter_time = DateTime.Now;
					AnimatedDraw ();
				}
			}
		}
		
		Gdk.Rectangle MinimumDockArea {
			get {
				if (minimum_dock_size.X == -1)
					minimum_dock_size = new Gdk.Rectangle ((ZoomSize/2)-XBuffer, Height-IconSize-2*YBuffer, Width - ZoomSize, IconSize+2*YBuffer);
				return minimum_dock_size;
			}
		}
		
		bool CursorIsOverDockArea {
			get {
				Gdk.Rectangle rect = MinimumDockArea;
				rect.Inflate (0, 55);
				return rect.Contains (Cursor); 
			}
		}
		
		bool ZoomAnimationNeeded {
			get { return !((CursorIsOverDockArea && ZoomIn == 1) || (!CursorIsOverDockArea && ZoomIn == 0)); }
		}
		
		DateTime last_click = DateTime.Now;
		bool BounceAnimationNeeded {
			get { return (DateTime.Now - last_click).TotalMilliseconds < BounceTime; }
		}
		
		public DockArea(DockWindow window, IEnumerable<DockItem> baseItems) : base ()
		{
			this.window = window;
			dock_items = new List<DockItem> (baseItems);
			cursor = new Gdk.Point (-1, -1);
			
			SetSizeRequest (Width, Height);
			this.SetCompositeColormap ();
			
			AddEvents ((int) Gdk.EventMask.PointerMotionMask | (int) Gdk.EventMask.LeaveNotifyMask |
			           (int) Gdk.EventMask.ButtonPressMask | (int) Gdk.EventMask.ButtonReleaseMask);
			
			DoubleBuffered = false;
		}
		
		uint timer = 0;
		void AnimatedDraw ()
		{
			if (timer > 0)
				return;
			
			QueueDraw ();
			
			timer = GLib.Timeout.Add (20, delegate {
				QueueDraw ();
				if (ZoomAnimationNeeded || BounceAnimationNeeded)
					return true;
				
				timer = 0;
				return false;
			});
		}
		
		void DrawDrock (Context cr)
		{
			Gdk.Rectangle dock_area = GetDockArea ();
			cr.Rectangle (dock_area.X+.5, dock_area.Y+.5, dock_area.Width-1, dock_area.Height);
			cr.Color = new Cairo.Color (.1, .1, .1, .7);
			cr.FillPreserve ();
			
			cr.Color = new Cairo.Color (1, 1, 1, .4);
			cr.LineWidth = 1;
			cr.Stroke ();
			
			DrawIcons (cr);
		}
		
		void DrawIcons (Context cr)
		{
			for (int i=0; i<dock_items.Count; i++) {
				int center;
				double zoom;
				
				IconPositionedCenterX (i, out center, out zoom);
				
				double x = (1/zoom)*(center - zoom*IconSize/2);
				double y = (1/zoom)*(Height-(zoom*IconSize)) + IconBorderWidth/2 - YBuffer;
				
				int total_ms = (int) (DateTime.Now - dock_items[i].LastClick).TotalMilliseconds;
				if (total_ms < BounceTime) {
					y -= Math.Abs (20*Math.Sin (total_ms*Math.PI/(BounceTime/2)));
				}
				
				cr.Scale (zoom/DockItem.IconQuality, zoom/DockItem.IconQuality);
				cr.Rectangle (x*DockItem.IconQuality, y*DockItem.IconQuality, IconSize*DockItem.IconQuality, IconSize*DockItem.IconQuality);
				Gdk.CairoHelper.SetSourcePixbuf (cr, dock_items[i].Pixbuf, x*DockItem.IconQuality, y*DockItem.IconQuality);
				cr.Fill ();
				cr.Scale (1/(zoom/DockItem.IconQuality), 1/(zoom/DockItem.IconQuality));
			}
		}
		
		int IconNormalCenterX (int icon)
		{
			return MinimumDockArea.X + XBuffer + (IconSize/2) + (IconSize * icon);
		}
		
		int DockItemForX (int x)
		{
			return (x-MinimumDockArea.X)/IconSize;
		}
		
		void IconPositionedCenterX (int icon, out int x, out double zoom)
		{
			int center = IconNormalCenterX (icon);
			int offset = Math.Min (Math.Abs (Cursor.X - center), ZoomPixels/2);
			
			if (ZoomPixels/2 == 0)
				zoom = 1;
			else {
				zoom = 2 - (offset/(double)(ZoomPixels/2));
				zoom = (zoom-1)*ZoomIn+1;
			}
			
			offset = (int) (offset*Math.Sin ((Math.PI/4)*zoom));
			
			if (Cursor.X > center) {
				center -= offset;
			} else {
				center += offset;
			}
			x = center;
		}
		
		Gdk.Rectangle GetDockArea ()
		{
			if (!CursorIsOverDockArea && ZoomIn == 0)
				return MinimumDockArea;

			int start_x, end_x;
			double start_zoom, end_zoom;
			IconPositionedCenterX (0, out start_x, out start_zoom);
			IconPositionedCenterX (dock_items.Count - 1, out end_x, out end_zoom);
			
			int x = start_x - (int)(start_zoom*(IconSize/2)) - XBuffer;
			int end = end_x + (int)(end_zoom*(IconSize/2)) + XBuffer;
			
			return new Gdk.Rectangle (x, Height-IconSize-10, end-x, IconSize+10);
		}
		
		protected override bool OnExposeEvent(EventExpose evnt)
		{
			bool ret_val = base.OnExposeEvent (evnt);
			if (!IsDrawable)
				return ret_val;
			last_render = DateTime.Now;
			
			if (backbuffer == null) {
				Context tmp = Gdk.CairoHelper.Create (GdkWindow);
				backbuffer = tmp.Target.CreateSimilar (tmp.Target.Content, Width, Height);
				(tmp as IDisposable).Dispose ();
			}
			
			Context cr = new Cairo.Context (backbuffer);
			cr.Color = new Cairo.Color (0, 0, 0, 0);
			cr.Operator = Operator.Source;
			cr.Paint ();
			cr.Operator = Operator.Over;
			
			DrawDrock (cr);
			(cr as IDisposable).Dispose ();
			
			Context cr2 = Gdk.CairoHelper.Create (GdkWindow);
			cr2.SetSource (backbuffer, 0, 0);
			cr2.Operator = Operator.Source;
			cr2.Paint ();
			(cr2 as IDisposable).Dispose ();
			
			return ret_val;
		}
 
		protected override bool OnMotionNotifyEvent(EventMotion evnt)
		{
			bool tmp = CursorIsOverDockArea;
			Cursor = new Gdk.Point ((int) evnt.X, (int) evnt.Y);
			
			if (tmp != CursorIsOverDockArea || CursorIsOverDockArea && DateTime.Now.Subtract (last_render).TotalMilliseconds > 20) 
				AnimatedDraw ();
			return base.OnMotionNotifyEvent (evnt);
		}
		
		protected override bool OnButtonReleaseEvent (Gdk.EventButton evnt)
		{
			int item = DockItemForX ((int) evnt.X);
			if ((DateTime.Now - dock_items[item].LastClick).TotalMilliseconds > BounceTime) {
				last_click = dock_items[item].LastClick = DateTime.Now;
				IItem doItem = dock_items[item].IObject as IItem;
				if (doItem != null)
					window.Controller.PerformDefaultAction (dock_items[item].IObject as IItem);
				AnimatedDraw ();
			}
			return base.OnButtonReleaseEvent (evnt);
		}

		protected override bool OnLeaveNotifyEvent (Gdk.EventCrossing evnt)
		{
			Cursor = new Gdk.Point ((int) evnt.X, (int) evnt.Y);
			return base.OnLeaveNotifyEvent (evnt);
		}

		
		public void SetIcons (IEnumerable<DockItem> items)
		{
			dock_items.Clear ();
			foreach (DockItem i in items)
				dock_items.Add (i);
			minimum_dock_size = new Gdk.Rectangle (-1, -1, -1, -1);
			backbuffer.Destroy ();
			backbuffer = null;
			
			SetSizeRequest (Width, Height);
		}
	}
}
