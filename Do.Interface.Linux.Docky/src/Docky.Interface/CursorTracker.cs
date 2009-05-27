// CursorTracker.cs
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

using Gdk;
using Gtk;

namespace Docky.Interface
{
	public class CursorUpdatedArgs : EventArgs
	{
		public readonly Point OldCursor;
		public readonly Point NewCursor;
		
		public CursorUpdatedArgs (Point newCursor, Point oldCursor)
		{
			NewCursor = newCursor;
			OldCursor = oldCursor;
		}
	}
	
	public class CursorTracker : IDisposable
	{
		uint timer;
		uint timer_length;
		
		DateTime last_gtk_update;
		
		Gtk.Window parent;
		
		public event EventHandler<CursorUpdatedArgs> CursorUpdated;
		
		public bool Enabled { get; set; }
		
		public Gdk.Point Cursor { get; private set; }
		public Gdk.Point RootCursor { get; private set; }
		
		public ModifierType ModifierType { get; private set; }
		
		public Cairo.PointD CursorD { 
			get { return new Cairo.PointD (Cursor.X, Cursor.Y); }
		}
		
		public Cairo.PointD RootCursorD {
			get { return new Cairo.PointD (RootCursor.X, RootCursor.Y); }
		}
		
		public uint TimerLength {
			get { return timer_length; }
			set {
				if (timer_length == value)
					return;
				timer_length = value;
				ResetTimer ();
			}
		}
		
		public CursorTracker (Gtk.Window parent, uint timerLength)
		{
			Enabled = true;
			this.parent = parent;
			
			parent.MotionNotifyEvent += HandleMotionNotifyEvent; 
			parent.AddEvents ((int) EventMask.PointerMotionMask);
			
			timer_length = timerLength;
			timer = GLib.Timeout.Add (timer_length, HandleCursorTimeoutElapsed);
		}
		
		bool HandleCursorTimeoutElapsed ()
		{
			if (!Enabled || (DateTime.UtcNow - last_gtk_update).TotalMilliseconds < timer_length >> 1) {
				return true;
			}
			
			int x, y, xroot, yroot;
			ModifierType mod;
			Gdk.Screen screen;
			
			parent.Display.GetPointer (out screen, out xroot, out yroot, out mod);
			
			if (screen == parent.Screen) {
				Gdk.Rectangle geo;
				if (parent is DockWindow)
					(parent as DockWindow).GetBufferedPosition (out geo.X, out geo.Y);
				else
					parent.GetPosition (out geo.X, out geo.Y);

				x = xroot - geo.X;
				y = yroot - geo.Y;
			} else {
				xroot = -4000;
				yroot = -4000;
				x = -4000;
				y = -4000;
			}
			
			if (Cursor.X != x || Cursor.Y != y || ModifierType != mod) {
				Point last = Cursor;
				Cursor = new Point (x, y);
				RootCursor = new Point (xroot, yroot);
				ModifierType = mod;
				
				OnCursorUpdated (last);
			}
			
			return true;
		}
		
		void HandleMotionNotifyEvent (object o, MotionNotifyEventArgs args)
		{
			if (!Enabled)
				return;
			
			Point last = Cursor;
			last_gtk_update = DateTime.UtcNow;
			Cursor = new Point ((int) args.Event.X, (int) args.Event.Y);
			RootCursor = new Point ((int) args.Event.XRoot, (int) args.Event.YRoot);
			ModifierType = args.Event.State;
			
			OnCursorUpdated (last);
		}
		
		void OnCursorUpdated (Point lastCursor)
		{
			if (CursorUpdated != null)
				CursorUpdated (this, new CursorUpdatedArgs (Cursor, lastCursor));
		}
		
		void ResetTimer ()
		{
			GLib.Source.Remove (timer);
			timer = GLib.Timeout.Add (timer_length, HandleCursorTimeoutElapsed);
		}
		
		public void Dispose ()
		{
			parent.MotionNotifyEvent -= HandleMotionNotifyEvent;
		}
	}
}
