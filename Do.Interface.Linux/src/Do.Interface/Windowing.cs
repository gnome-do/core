/* Windowing.cs
 *
 * GNOME Do is the legal property of its developers. Please refer to the
 * COPYRIGHT file distributed with this source distribution.
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Gdk;
using Mono.Unix;

namespace Do.Interface
{

	public static class Windowing
	{

		public static void PresentWindow (Gtk.Window window)
		{
			window.Present ();
			window.GdkWindow.Raise ();

			for (int i = 0; i < 100; i++) {
				if (TryGrabWindow (window)) {
					break;
				}
				Thread.Sleep (100);
			}
		}
		
		public static void UnpresentWindow (Gtk.Window window)
		{
			uint time;
			time = Gtk.Global.CurrentEventTime;
			
			Pointer.Ungrab (time);
			Keyboard.Ungrab (time);
			Gtk.Grab.Remove (window);
		}
		
		private static bool TryGrabWindow (Gtk.Window window)
		{
			uint time;

			time = Gtk.Global.CurrentEventTime;
			if (Pointer.Grab (window.GdkWindow,
												true,
												EventMask.ButtonPressMask |
												EventMask.ButtonReleaseMask |
												EventMask.PointerMotionMask,
												null,
												null,
												time) == GrabStatus.Success)
			{
				if (Keyboard.Grab (window.GdkWindow, true, time) == GrabStatus.Success) {
					Gtk.Grab.Add (window);
					return true;
				} else {
					Pointer.Ungrab (time);
					return false;
				}
			}
			return false;
		}

	}
}
