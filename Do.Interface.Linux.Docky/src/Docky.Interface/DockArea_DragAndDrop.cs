// DockArea_DragAndDrop.cs
// 
// Copyright (C) 2009 GNOME Do
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
using System.Text.RegularExpressions;

using Gdk;
using Gtk;

using Do.Platform;
using Do.Interface;
using Do.Universe;
using Do.Interface.CairoUtils;
using Do;

using Docky.Core;
using Docky.Utilities;

namespace Docky.Interface
{
	
	
	public partial class DockArea
	{
		enum DragEdge {
			None = 0,
			Top,
			Left,
			Right,
		}

		bool drag_resizing;
		bool gtk_drag_source_set;
		
		int drag_start_icon_size;
		
		DragEdge drag_edge;

		Gdk.Point drag_start_point;
		
		Gdk.CursorType cursor_type;

		Gdk.Window drag_proxy;

		DragState DragState { get; set; }

		bool GtkDragging { get; set; }

		bool CursorNearTopDraggableEdge {
			get {
				return MinimumDockArea.Contains (Cursor) && CurrentDockItem is SeparatorItem;
			}
		}
		
		bool CursorNearLeftEdge {
			get {
				if (DockPreferences.DockIsHorizontal)
					return CursorIsOverDockArea && Math.Abs (Cursor.X - MinimumDockArea.X) < 8;
				return CursorIsOverDockArea && Math.Abs (Cursor.Y - MinimumDockArea.Y) < 8;
			}
		}
		
		bool CursorNearRightEdge {
			get {
				if (DockPreferences.DockIsHorizontal)
					return CursorIsOverDockArea && Math.Abs (Cursor.X - (MinimumDockArea.X + MinimumDockArea.Width)) < 8;
				return CursorIsOverDockArea && Math.Abs (Cursor.Y - (MinimumDockArea.Y + MinimumDockArea.Height)) < 8;
			}
		}
		
		bool CursorNearDraggableEdge {
			get {
				return CursorNearTopDraggableEdge || 
					   CursorNearRightEdge || 
					   CursorNearLeftEdge;
			}
		}
		
		DragEdge CurrentDragEdge {
			get {
				if (CursorNearTopDraggableEdge)
					return DragEdge.Top;
				else if (CursorNearLeftEdge)
					return DragEdge.Left;
				else if (CursorNearRightEdge)
					return DragEdge.Right;
				return DragEdge.None;
			}
		}

		void RegisterGtkDragSource ()
		{
			gtk_drag_source_set = true;
			TargetEntry te = new TargetEntry ("text/uri-list", TargetFlags.OtherApp, 0);
			Gtk.Drag.SourceSet (this, Gdk.ModifierType.Button1Mask, new [] {te}, DragAction.Copy);
		}
		
		void RegisterGtkDragDest ()
		{
			TargetEntry dest_te = new TargetEntry ("text/uri-list", 0, 0);
			Gtk.Drag.DestSet (this, DestDefaults.Motion | DestDefaults.Drop, new [] {dest_te}, Gdk.DragAction.Copy);
		}
		
		void UnregisterGtkDragSource ()
		{
			gtk_drag_source_set = false;
			Gtk.Drag.SourceUnset (this);
		}

		void SetDragProxy (Gdk.Window window)
		{
			if (window == drag_proxy)
				return;
			drag_proxy = window;
			Gtk.Drag.DestSetProxy (this, window, DragProtocol.Xdnd, true);
		}

		void UnsetDragProxy ()
		{
			if (drag_proxy == null)
				return;
			drag_proxy = null;
			RegisterGtkDragDest ();
		}

		void DragCursorUpdate ()
		{
			if (GtkDragging && (CursorModifier & ModifierType.Button1Mask) != ModifierType.Button1Mask)
				GtkDragging = false;
			if (!GtkDragging || CursorIsOverDockArea) {
				UnsetDragProxy ();
			} else {
				Gdk.Point local_cursor = Cursor.RelativePointToRootPoint (window);

				IEnumerable<Gdk.Window> windows;
				try {
					windows = Screen.WindowStack;
				} catch { return; }
				
				foreach (Gdk.Window w in windows.Reverse ()) {
					if (w == window.GdkWindow || !w.IsVisible)
						continue;
					
					Gdk.Rectangle rect;
					int depth;
					w.GetGeometry (out rect.X, out rect.Y, out rect.Width, out rect.Height, out depth);
					if (rect.Contains (local_cursor)) {
						SetDragProxy (w);
						break;
					}
				}
			}
		}

		protected override bool OnDragMotion (Gdk.DragContext context, int x, int y, uint time)
		{
			GtkDragging = true;
			
			do {
				if (DragState.DragItem == null || DragState.IsFinished || !DockItems.Contains (DragState.DragItem) || !CursorIsOverDockArea)
					continue;
				
				int draggedPosition = DockItems.IndexOf (DragState.DragItem);
				int currentPosition = PositionProvider.IndexAtPosition (Cursor);
				if (draggedPosition == currentPosition || currentPosition == -1)
					continue;
				
				DockServices.ItemsService.MoveItemToPosition (draggedPosition, currentPosition);
			} while (false);
			
			AnimatedDraw ();
			return base.OnDragMotion (context, x, y, time);
		}

		protected override void OnDragDataReceived (Gdk.DragContext context, int x, int y, 
		                                            Gtk.SelectionData selectionData, uint info, uint time)
		{
			if (!CursorIsOverDockArea) return;

			UnsetDragProxy ();
			
			string data = System.Text.Encoding.UTF8.GetString ( selectionData.Data );
			data = System.Uri.UnescapeDataString (data);
			//sometimes we get a null at the end, and it crashes us
			data = data.TrimEnd ('\0'); 
			
			string [] uriList = Regex.Split (data, "\r\n");
			if (CurrentDockItem != null && CurrentDockItem.IsAcceptingDrops) {
				uriList.Where (uri => uri.StartsWith ("file://"))
					.ForEach (uri => CurrentDockItem.ReceiveItem (uri.Substring ("file://".Length)));
			} else {
				uriList.Where (uri => uri.StartsWith ("file://"))
					.ForEach (uri => DockServices.ItemsService.AddItemToDock (uri.Substring ("file://".Length)));
			}
			
			base.OnDragDataReceived (context, x, y, selectionData, info, time);
			GtkDragging = false;
		}
		
		protected override void OnDragBegin (Gdk.DragContext context)
		{
			GtkDragging = true;
			// the user might not end the drag on the same horizontal position they start it on
			int item = PositionProvider.IndexAtPosition (Cursor);

			if (item != -1 && DockServices.ItemsService.ItemCanBeMoved (item))
				DragState = new DragState (Cursor, DockItems [item]);
			else
				DragState = new DragState (Cursor, null);
			
			Gdk.Pixbuf pbuf;
			if (DragState.DragItem == null) {
				pbuf = IconProvider.PixbufFromIconName ("gtk-remove", DockPreferences.IconSize);
			} else {
				pbuf = DragState.DragItem.GetDragPixbuf ();
			}
				
			if (pbuf != null)
				Gtk.Drag.SetIconPixbuf (context, pbuf, pbuf.Width / 2, pbuf.Height / 2);
			base.OnDragBegin (context);
		}
		
		protected override void OnDragEnd (Gdk.DragContext context)
		{
			
			if (CursorIsOverDockArea) {
				int currentPosition = PositionProvider.IndexAtPosition (Cursor);
				if (currentPosition != -1)
					DockServices.ItemsService.DropItemOnPosition (DragState.DragItem, currentPosition);
			} else {
				DockServices.ItemsService.RemoveItem (DragState.DragItem);
			}
			DragState.IsFinished = true;
			GtkDragging = false;
			
			AnimatedDraw ();
			base.OnDragEnd (context);
		}

		void BuildDragAndDrop ()
		{
			cursor_type = CursorType.LeftPtr;

			DragState = new DragState (Cursor, null);
			DragState.IsFinished = true;
		}
		
		void ConfigureCursor ()
		{
			// we do this so that our custom drag isn't destroyed by gtk's drag
			if (gtk_drag_source_set && CursorNearDraggableEdge) {
				UnregisterGtkDragSource ();

				if (cursor_type != CursorType.SbVDoubleArrow && CursorNearTopDraggableEdge) {
					if (DockPreferences.DockIsHorizontal)
						SetCursor (CursorType.SbVDoubleArrow);
					else
						SetCursor (CursorType.SbHDoubleArrow);
					
				} else if (cursor_type != CursorType.LeftSide && CursorNearLeftEdge) {
					if (DockPreferences.DockIsHorizontal)
						SetCursor (CursorType.LeftSide);
					else
						SetCursor (CursorType.TopSide);
					
				} else if (cursor_type != CursorType.RightSide && CursorNearRightEdge) {
					if (DockPreferences.DockIsHorizontal)
						SetCursor (CursorType.RightSide);
					else
						SetCursor (CursorType.BottomSide);
				}
				
			} else if (!gtk_drag_source_set && !drag_resizing && !CursorNearDraggableEdge) {
				if (!PainterOverlayVisible)
					RegisterGtkDragSource ();
				if (cursor_type != CursorType.LeftPtr)
					SetCursor (CursorType.LeftPtr);
			}
		}
		
		void SetCursor (Gdk.CursorType type)
		{
			cursor_type = type;
			Gdk.Cursor tmp_cursor = new Gdk.Cursor (type);
			GdkWindow.Cursor = tmp_cursor;
			tmp_cursor.Dispose ();
		}

		void StartDrag ()
		{
			drag_start_point = Cursor;
			drag_start_icon_size = DockPreferences.IconSize;
			drag_resizing = true;
			drag_edge = CurrentDragEdge;
		}
		
		void EndDrag ()
		{
			drag_edge = DragEdge.None;
			drag_resizing = false;
			SetIconRegions ();
			window.SetStruts ();
			
			AnimatedDraw ();
			
			ResetCursorTimer ();
		}
		
		void HandleDragMotion ()
		{
			int movement = 0;
			switch (drag_edge) {
			case DragEdge.Top:
				int delta = DockPreferences.DockIsHorizontal ? drag_start_point.Y - Cursor.Y : drag_start_point.X - Cursor.X;
				if (DockPreferences.Orientation == DockOrientation.Left || DockPreferences.Orientation == DockOrientation.Top)
					delta = 0 - delta;
				DockPreferences.IconSize = Math.Min (drag_start_icon_size + delta, DockPreferences.MaxIconSize);
				return;
			case DragEdge.Left:
				movement = DockPreferences.DockIsHorizontal ? drag_start_point.X - Cursor.X : drag_start_point.Y - Cursor.Y;
				break;
			case DragEdge.Right:
				movement = DockPreferences.DockIsHorizontal ? Cursor.X - drag_start_point.X : Cursor.Y - drag_start_point.Y;
				break;
			}

			if (movement > IconSize / 2 + 2) {
				DockPreferences.AutomaticIcons++;
			} else if (movement < 0 - (IconSize / 2 + 2)) {
				DockPreferences.AutomaticIcons--;
			} else {
				return;
			}
			
			drag_start_point = Cursor;
		}
	}
}
