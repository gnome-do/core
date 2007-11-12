/* Do.cs
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

using Do.Core;
using Do.DBusLib;

namespace Do
{
	
	public class Do {
		
		static Commander commander;
		static UniverseManager universeManager;
	
		public static void Main (string[] args) {

			DetectInstanceAndExit ();	

			Gtk.Application.Init ();
			Log.Initialize ();
			Util.Initialize ();
		
			Gdk.Threads.Init ();
			
			try {
				Util.SetProcessName ("gnome-do");
			} catch (Exception e) {
				Log.Error ("Failed to set process name: {0}", e.Message);
			}
			
			universeManager = new UniverseManager ();
			commander = new DefaultCommander ();	
			DBusRegistrar.RegisterCommander (commander);
			commander.Show ();
			
			Gtk.Application.Run ();
		}	

		static void DetectInstanceAndExit ()
		{
			ICommander dbus_commander;			
			dbus_commander = DBusRegistrar.GetCommanderInstance ();
			if (dbus_commander != null) {
				dbus_commander.Show ();
				System.Environment.Exit (0);
			}
		}
		
		public static Commander Commander {
			get { return commander; }
		}
		
		public static UniverseManager UniverseManager {
			get { return universeManager; }
		}
	}
}
