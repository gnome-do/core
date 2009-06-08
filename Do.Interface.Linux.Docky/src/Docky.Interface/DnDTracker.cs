//  
//  Copyright (C) 2009 GNOME Do
// 
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
// 
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
// 

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;

using Gdk;
using Gtk;

using Docky.Core;
using Docky.Utilities;

using Do.Interface;

namespace Docky.Interface
{
	public enum DragEdge {
		None = 0,
		Top,
		Left,
		Right,
	}
	
	public class DnDTracker : IDisposable
	{
		public event EventHandler DrawRequired;
		public event EventHandler DragEnded;
		
		const int DragHotZoneSize = 8;

		public bool DragResizing { get; private set; }

		public DragEdge DragEdge { get; private set; }

		public DragState DragState { get; set; }

		public bool GtkDragging { get; set; }
		
		public bool PreviewIsDesktopFile { get; set; }
		
		bool gtk_drag_source_set;
		
		int drag_start_icon_size;
		
		DockArea parent;
		
		Gdk.Point drag_start_point;
		
		Gdk.CursorType cursor_type;

		Gdk.Window drag_proxy;
		
		Gdk.DragContext drag_context;

		IEnumerable<string> uri_list;
		
		Gdk.Point Cursor {
			get { return CursorTracker.Cursor; }
		}
		
		Gdk.Rectangle MinimumDockArea {
			get { return parent.MinimumDockArea; }
		}
		
		ReadOnlyCollection<AbstractDockItem> DockItems { 
			get { return DockServices.ItemsService.DockItems; } 
		}
		
		ItemPositionProvider PositionProvider { get; set; }
		
		CursorTracker CursorTracker { get; set; }
		
		AbstractDockItem CurrentDockItem {
			get {
				try { return DockItems [PositionProvider.IndexAtPosition (Cursor)]; }
				catch { return null; }
			}
		}
		
		bool CursorNearTopDraggableEdge {
			get {
				return MinimumDockArea.Contains (Cursor) && CurrentDockItem is SeparatorItem;
			}
		}
		
		bool CursorNearLeftEdge {
			get {
				return parent.CursorIsOverDockArea && Math.Abs (Cursor.X - MinimumDockArea.X) < DragHotZoneSize;
			}
		}
		
		bool CursorNearRightEdge {
			get {
				return parent.CursorIsOverDockArea && Math.Abs (Cursor.X - (MinimumDockArea.X + MinimumDockArea.Width)) < DragHotZoneSize;
			}
		}
		
		bool CursorNearDraggableEdge {
			get {
				return CursorNearTopDraggableEdge || 
					   CursorNearRightEdge || 
					   CursorNearLeftEdge;
			}
		}
		
		public bool InternalDragActive {
			get { return DragResizing && drag_start_point != Cursor; }
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
		
		IEnumerable<Gdk.Window> WindowStack {
			get {
				try {
					return parent.Screen.WindowStack;
				} catch { 
					try {
						return Wnck.Screen.Default.WindowsStacked.Select (wnk => Gdk.Window.ForeignNew ((uint) wnk.Xid));
					} catch {
						return null;
					}
				}
			}
		}
		
		internal DnDTracker (DockArea parent, ItemPositionProvider position, CursorTracker cursorTracker)
		{
			this.parent = parent;
			cursor_type = CursorType.LeftPtr;
			PositionProvider = position;
			CursorTracker = cursorTracker;

			DragState = new DragState (Cursor, null);
			DragState.IsFinished = true;
			
			RegisterEvents ();
			
			RegisterGtkDragDest ();
			RegisterGtkDragSource ();
		}
		
		public void Enable ()
		{
			RegisterGtkDragSource ();
		}
		
		public void Disable ()
		{
			UnregisterGtkDragSource ();
		}
		
		void RegisterEvents ()
		{
			parent.DragDataReceived  += HandleDragDataReceived;
			parent.DragMotion        += HandleDragMotionEvent; 
			parent.DragBegin         += HandleDragBegin;
			parent.DragEnd           += HandleDragEnd; 
			parent.DragDrop          += HandleDragDrop;
			parent.DragFailed        += HandleDragFailed; 
			parent.ButtonPressEvent  += HandleButtonPressEvent;
			parent.MotionNotifyEvent += HandleMotionNotifyEvent;
			parent.ButtonReleaseEvent += HandleButtonReleaseEvent; 
			
			CursorTracker.CursorUpdated += HandleCursorUpdated; 
		}

		void UnregisterEvents ()
		{
			parent.DragDataReceived  -= HandleDragDataReceived;
			parent.DragMotion        -= HandleDragMotionEvent; 
			parent.DragBegin         -= HandleDragBegin;
			parent.DragEnd           -= HandleDragEnd; 
			parent.DragDrop          -= HandleDragDrop;
			parent.DragFailed        -= HandleDragFailed; 
			parent.ButtonPressEvent  -= HandleButtonPressEvent; 
			parent.MotionNotifyEvent -= HandleMotionNotifyEvent;
			parent.ButtonReleaseEvent -= HandleButtonReleaseEvent;
			
			CursorTracker.CursorUpdated -= HandleCursorUpdated; 
		}
		
		void HandleMotionNotifyEvent(object o, MotionNotifyEventArgs args)
		{
			GtkDragging = false;
		}

		void HandleButtonPressEvent(object o, ButtonPressEventArgs args)
		{
			if (CursorNearDraggableEdge)
				StartDrag ();
		}

		void HandleButtonReleaseEvent(object o, ButtonReleaseEventArgs args)
		{
			if (DragResizing)
				EndDrag ();
		}
		
		void HandleCursorUpdated(object sender, CursorUpdatedArgs e)
		{
			if (GtkDragging && (CursorTracker.ModifierType & ModifierType.Button1Mask) != ModifierType.Button1Mask) {
				GtkDragging = false;
			}
			
			SetDragProxy ();
			
			ConfigureCursor ();
			
			if (DragResizing)
				HandleDragMotion ();
		}

		void HandleDragFailed (object o, DragFailedArgs args)
		{
			// disable the animation
			args.RetVal = parent.CursorIsOverDockArea || DockServices.ItemsService.ItemCanBeRemoved (DockItems.IndexOf (DragState.DragItem));
		}

		void HandleDragDrop (object o, DragDropArgs args)
		{
			int index = PositionProvider.IndexAtPosition (Cursor);
			if (parent.CursorIsOverDockArea && index >= 0 && index < DockItems.Count && uri_list != null) {
				foreach (string uri in uri_list) {
					if (CurrentDockItem != null && CurrentDockItem.IsAcceptingDrops && !uri.EndsWith (".desktop")) {
						CurrentDockItem.ReceiveItem (uri);
					} else {
						Gdk.Point center = PositionProvider.IconUnzoomedPosition (index);
						if (center.X < Cursor.X && index < DockItems.Count - 1)
							index++;
						DockServices.ItemsService.AddItemToDock (uri, index);
					}
				}
			}
			
			Gtk.Drag.Finish (args.Context, true, true, args.Time);
		}

		void HandleDragEnd (object o, DragEndArgs args)
		{
			if (parent.CursorIsOverDockArea) {
				int currentPosition = PositionProvider.IndexAtPosition (Cursor);
				if (currentPosition != -1)
					DockServices.ItemsService.DropItemOnPosition (DragState.DragItem, currentPosition);
			} else {
				bool result = DockServices.ItemsService.RemoveItem (DragState.DragItem);
				if (result) {
					PoofWindow poof = new PoofWindow (DockPreferences.FullIconSize);
					poof.SetCenterPosition (CursorTracker.RootCursor);
					poof.Run ();
				}
			}
			
			DragState.IsFinished = true;
			GtkDragging = false;
			SetDragProxy ();
		
			OnDragEnded ();
			OnDrawRequired ();
		}

		void HandleDragBegin (object o, DragBeginArgs args)
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
				Gtk.Drag.SetIconPixbuf (args.Context, pbuf, pbuf.Width / 2, pbuf.Height / 2);
		}

		void HandleDragMotionEvent (object o, DragMotionArgs args)
		{
			GtkDragging = true;
			
			do {
				if (DragState.DragItem == null || DragState.IsFinished || 
				    !DockItems.Contains (DragState.DragItem) || !parent.CursorIsOverDockArea)
					continue;
				
				int draggedPosition = DockItems.IndexOf (DragState.DragItem);
				int currentPosition = PositionProvider.IndexAtPosition (Cursor);
				if (draggedPosition == currentPosition || currentPosition == -1)
					continue;
				
				DockServices.ItemsService.MoveItemToPosition (draggedPosition, currentPosition);
			} while (false);
			
			OnDrawRequired ();
			
			if (drag_context != args.Context) {
				Gdk.Atom target = Gtk.Drag.DestFindTarget (parent, args.Context, null);
				Gtk.Drag.GetData (parent, args.Context, target, Gtk.Global.CurrentEventTime);
				drag_context = args.Context;
				
			}
			
			Gdk.Drag.Status (args.Context, DragAction.Copy, args.Time);
			args.RetVal = true;
		}

		void HandleDragDataReceived (object o, DragDataReceivedArgs args)
		{
			IEnumerable<string> uriList;
			try {
				string data = System.Text.Encoding.UTF8.GetString (args.SelectionData.Data);
				data = System.Uri.UnescapeDataString (data);
				//sometimes we get a null at the end, and it crashes us
				data = data.TrimEnd ('\0'); 
				
				uriList = Regex.Split (data, "\r\n")
					.Where (uri => uri.StartsWith ("file://"))
					.Select (uri => uri.Substring ("file://".Length));
			} catch {
				uriList = Enumerable.Empty<string> ();
			}
			
			
			uri_list = uriList;
				
			PreviewIsDesktopFile = !uriList.Any () || uriList.Any (s => s.EndsWith (".desktop"));
			
			args.RetVal = true;
		}
		
		
		void RegisterGtkDragSource ()
		{
			gtk_drag_source_set = true;
			// we dont really want to offer the drag to anything, merely pretend to, so we set a mimetype nothing takes
			TargetEntry te = new TargetEntry ("nomatch", TargetFlags.App | TargetFlags.OtherApp, 0);
			Gtk.Drag.SourceSet (parent, Gdk.ModifierType.Button1Mask, new [] {te}, DragAction.Copy);
		}
		
		void RegisterGtkDragDest ()
		{
			TargetEntry dest_te = new TargetEntry ("text/uri-list", 0, 0);
			Gtk.Drag.DestSet (parent, 0, new [] {dest_te}, Gdk.DragAction.Copy);
		}
		
		void UnregisterGtkDragSource ()
		{
			gtk_drag_source_set = false;
			Gtk.Drag.SourceUnset (parent);
		}

		void SetDragProxy ()
		{
			if ((CursorTracker.ModifierType & ModifierType.Button1Mask) != ModifierType.Button1Mask || parent.CursorIsOverDockArea) {
				if (drag_proxy == null)
					return;
				drag_proxy = null;
				RegisterGtkDragDest ();
			} else {
				Gdk.Point local_cursor = CursorTracker.RootCursor;
	
				IEnumerable<Gdk.Window> windows = WindowStack;

				foreach (Gdk.Window w in windows.Reverse ()) {
					if (w == null || w == DockWindow.Window.GdkWindow || !w.IsVisible)
						continue;
					
					Gdk.Rectangle rect;
					int depth;
					w.GetGeometry (out rect.X, out rect.Y, out rect.Width, out rect.Height, out depth);
					if (rect.Contains (local_cursor)) {
						if (w == drag_proxy)
							break;
						
						drag_proxy = w;
						Gtk.Drag.DestSetProxy (parent, w, DragProtocol.Xdnd, true);
						break;
					}
				}
			}
		}

		void ConfigureCursor ()
		{
			// we do this so that our custom drag isn't destroyed by gtk's drag
			if (gtk_drag_source_set && CursorNearDraggableEdge) {
				UnregisterGtkDragSource ();

				if (cursor_type != CursorType.SbVDoubleArrow && CursorNearTopDraggableEdge) {
					SetCursor (CursorType.SbVDoubleArrow);
					
				} else if (cursor_type != CursorType.LeftSide && CursorNearLeftEdge) {
					SetCursor (CursorType.LeftSide);
					
				} else if (cursor_type != CursorType.RightSide && CursorNearRightEdge) {
					SetCursor (CursorType.RightSide);
				}
				
			} else if (!gtk_drag_source_set && !DragResizing && !CursorNearDraggableEdge) {
				if (!parent.PainterOverlayVisible)
					RegisterGtkDragSource ();
				if (cursor_type != CursorType.LeftPtr)
					SetCursor (CursorType.LeftPtr);
			}
		}
		
		void SetCursor (Gdk.CursorType type)
		{
			cursor_type = type;
			Gdk.Cursor tmp_cursor = new Gdk.Cursor (type);
			parent.GdkWindow.Cursor = tmp_cursor;
			tmp_cursor.Dispose ();
		}

		void StartDrag ()
		{
			if (parent.PainterOverlayVisible) return;
			
			drag_start_point = Cursor;
			drag_start_icon_size = DockPreferences.IconSize;
			DragResizing = true;
			DragEdge = CurrentDragEdge;
		}
		
		void EndDrag ()
		{
			if (parent.PainterOverlayVisible) return;
			
			DragEdge = DragEdge.None;
			DragResizing = false;
			
			OnDragEnded ();
		}
		
		void HandleDragMotion ()
		{
			int movement = 0;
			switch (DragEdge) {
			case DragEdge.Top:
				int delta = drag_start_point.Y - Cursor.Y;
				if (DockPreferences.Orientation == DockOrientation.Top)
					delta = 0 - delta;
				DockPreferences.IconSize = Math.Min (drag_start_icon_size + delta, DockPreferences.MaxIconSize);
				return;
			case DragEdge.Left:
				movement = drag_start_point.X - Cursor.X;
				break;
			case DragEdge.Right:
				movement = Cursor.X - drag_start_point.X;
				break;
			}

			if (movement > DockPreferences.IconSize / 2 + 2) {
				DockPreferences.AutomaticIcons++;
			} else if (movement < 0 - (DockPreferences.IconSize / 2 + 2)) {
				DockPreferences.AutomaticIcons--;
			} else {
				return;
			}
			
			drag_start_point = Cursor;
		}
		
		void OnDrawRequired ()
		{
			if (DrawRequired != null)
				DrawRequired (this, EventArgs.Empty);
		}
		
		void OnDragEnded ()
		{
			if (DragEnded != null)
				DragEnded (this, EventArgs.Empty);
		}

		#region IDisposable implementation
		public void Dispose ()
		{
			UnregisterEvents ();
		}
		#endregion

	}
}
