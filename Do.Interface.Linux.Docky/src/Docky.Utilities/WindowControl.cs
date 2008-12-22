// WindowControl.cs
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

using Wnck;

namespace Docky.Utilities
{
	
	
	public static class WindowControl
	{
		
		/// <summary>
		/// Handles intelligent minimize/restoring of windows.  If one or more windows is minimized, it restores
		/// all windows.  If more all are visible, it minimizes.  This operation only takes into account windows
		/// on the current workspace (by design).
		/// </summary>
		/// <param name="windows">
		/// A <see cref="IEnumerable"/>
		/// </param>
		public static void MinimizeRestoreWindows (IEnumerable<Window> windows)
		{
			bool restore = false;
			foreach (Window w in windows) {
				if (w.IsMinimized) {
					restore = true;
					break;
				}
			}
			if (restore)
				RestoreWindows (windows);
			else
				MinimizeWindows (windows);
		}
		
		/// <summary>
		/// Minimizes every window in the list if it is not minimized
		/// </summary>
		/// <param name="windows">
		/// A <see cref="IEnumerable"/>
		/// </param>
		public static void MinimizeWindows (IEnumerable<Window> windows)
		{
			foreach (Window window in windows) {
				if (window.IsInViewport (window.Workspace) && !window.IsMinimized)
					window.Minimize ();
			}
		}
		
		/// <summary>
		/// Restores every window in the list that is minimized
		/// </summary>
		/// <param name="windows">
		/// A <see cref="IEnumerable"/>
		/// </param>
		public static void RestoreWindows (IEnumerable<Window> windows)
		{
			foreach (Window window in windows) {
				if (window.IsInViewport (window.Workspace) && window.IsMinimized)
					window.Unminimize (Gtk.Global.CurrentEventTime);
			}
		}
		
		public static void CloseWindows (IEnumerable<Window> windows)
		{
			foreach (Window window in windows)
				window.Close (Gtk.Global.CurrentEventTime);
		}
		
		public static void CloseWindows (Window window)
		{
			CloseWindows (new [] {window});
		}
		
		public static void MinimizeRestoreWindows (Window window)
		{
			MinimizeRestoreWindows (new [] {window});
		}
		
		public static void MaximizeWindow (Window window)
		{
			window.Maximize ();
		}
	}
}
