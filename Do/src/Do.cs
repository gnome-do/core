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
using Mono.GetOptions;

namespace Do
{
	public class Do
	{
		const string kActivateKeybinding = "<Super>space";
		static Tomboy.GConfXKeybinder keybinder;
		
		static Controller controller;
		static UniverseManager universeManager;

		public static void Main (string[] args)
		{
			DoOptions options;
			
			Gtk.Application.Init ();
			
			DetectInstanceAndExit ();
			Log.Initialize ();
			Util.Initialize ();
			
			Gdk.Threads.Init ();			

			try {
				Util.SetProcessName ("gnome-do");
			} catch (Exception e) {
				Log.Error ("Failed to set process name: {0}", e.Message);
			}

			options = new DoOptions ();
			options.ProcessArgs (args);
			
			universeManager = new UniverseManager ();
			universeManager.Initialize ();

			controller = new Controller ();
			DBusRegistrar.RegisterController (controller);
			
			keybinder = new Tomboy.GConfXKeybinder ();
			SetupKeybindings ();

			if (!options.quiet)
				controller.Summon ();
			
			Gtk.Application.Run ();
		}

		static void DetectInstanceAndExit ()
		{
			IController dbus_controller;
			dbus_controller = DBusRegistrar.GetControllerInstance ();
			if (dbus_controller != null) {
				dbus_controller.Summon ();
				System.Environment.Exit (0);
			}
		}

		public static Controller Controller
		{
			get { return controller; }
		}

		public static UniverseManager UniverseManager
		{
			get { return universeManager; }
		}
		
		static void SetupKeybindings ()
		{
			GConf.Client client;
			string binding;

			client = new GConf.Client();
			try {
				binding = client.Get ("/apps/gnome-do/preferences/key_binding") as string;
			} catch {
				binding = kActivateKeybinding;
				client.Set ("/apps/gnome-do/preferences/key_binding", binding);
			}
			keybinder.Bind ("/apps/gnome-do/preferences/key_binding", binding, OnActivate);
		}
		
		static void OnActivate (object sender, EventArgs args)
		{
			controller.Summon ();
		}
	}
}
