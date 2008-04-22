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

using Mono.Unix;

using Do.UI;
using Do.Core;
using Do.DBusLib;

namespace Do {

	public static class Do {
		
		static GConfXKeybinder keybinder;
		
		static Preferences preferences;
		static Controller controller;
		static PluginManager plugin_manager;
		static UniverseManager universe_manager;
		static SettingsWindow preferences_window;

		public static void Main (string[] args)
		{
			Catalog.Init ("gnome-do", "/usr/local/share/locale");
			Gtk.Application.Init ();
			
			preferences = new Preferences (args);

			DetectInstanceAndExit ();
			Log.Initialize ();
			Util.Initialize ();

			Gdk.Threads.Init ();			

			try {
				Util.SetProcessName ("gnome-do");
			} catch (Exception e) {
				Log.Error ("Failed to set process name: {0}", e.Message);
			}

			PluginManager.Initialize ();
			UniverseManager.Initialize ();

			// Previously, Controller's constructor created a Gtk.Window, and
			// that window used Util.Appearance to load an icon, and
			// Util.Appearance used Do.Controller in its constructor to
			// subscribe to an event.  This lead to some strange behavior, so
			// we new the Controller, /then/ Initialize it so that
			// Do.Controller is non-null when Util.Appearance references it.
			Controller.Initialize ();
			DBusRegistrar.RegisterController (Controller);
			
			keybinder = new GConfXKeybinder ();
			SetupKeybindings ();

			if (!Preferences.BeQuiet) {
				Controller.Summon ();
			}
			
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

		public static Preferences Preferences {
			get { return preferences; }
		}
		
		public static SettingsWindow PreferencesWindow {
			get {
				if (null == preferences_window) {
					preferences_window = new SettingsWindow ();
					preferences_window.Destroyed +=
						delegate {
							preferences_window = null;
						};
				}
				return preferences_window;
			}
		}

		public static Controller Controller {
			get {
				return controller ??
					controller = new Controller ();
			}
		}

		public static PluginManager PluginManager {
			get {
				return plugin_manager ??
					plugin_manager = new PluginManager ();
			}
		}

		public static UniverseManager UniverseManager {
			get {
				return universe_manager ??
					universe_manager = new UniverseManager ();
			}
		}
		
		static void SetupKeybindings ()
		{
			keybinder.Bind ("/apps/gnome-do/preferences/key_binding",
					Preferences.SummonKeyBinding, OnActivate);
		}
		
		static void OnActivate (object sender, EventArgs args)
		{
			controller.Summon ();
		}
	}
}
