// Do.cs
//
// GNOME Do is the legal property of its developers. Please refer to the
// COPYRIGHT file distributed with this source distribution.
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
using System.Threading;

using Mono.Unix;

using Do.UI;
using Do.Core;
using Do.DBusLib;

namespace Do {

	public static class Do {
		
		static GConfXKeybinder keybinder;
		
		static DoPreferences preferences;
		static Controller controller;
		static UniverseManager universe_manager;
		static NotificationIcon notification_icon;

		public static void Main (string[] args)
		{
			Catalog.Init ("gnome-do", "/usr/local/share/locale");
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

			PluginManager.Initialize ();
			UniverseManager.Initialize ();
			Controller.Initialize ();
			DBusRegistrar.RegisterController (Controller);
			
			keybinder = new GConfXKeybinder ();
			SetupKeybindings ();
			
			notification_icon = new NotificationIcon ();

			if (!Preferences.QuietStart)
				Controller.Summon ();
				
			if (Array.IndexOf (args, "--update") != -1)
				PluginManager.InstallAvailableUpdates (false);
			
			CheckForUpdates ();
			
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

		public static DoPreferences Preferences {
			get { return preferences ?? 
					preferences = new DoPreferences (); }
		}

		public static Controller Controller {
			get {
				return controller ??
					controller = new Controller ();
			}
		}

		public static UniverseManager UniverseManager {
			get {
				return universe_manager ??
					universe_manager = new UniverseManager ();
			}
		}
		
		public static NotificationIcon NotificationIcon {
			get {
				return notification_icon ??
					notification_icon = new NotificationIcon ();
			}
		}
		
		static void SetupKeybindings ()
		{
			keybinder.Bind ("/apps/gnome-do/preferences/core-preferences/SummonKeyBinding",
					Preferences.SummonKeyBinding, OnActivate);
		}
		
		static void OnActivate (object sender, EventArgs args)
		{
			controller.Summon ();
		}
		
		private static void CheckForUpdates ()
		{
			// Sets a timer to check for updates after 5 minutes
			// and again every 2 hours
			//Timer timer = new Timer (CheckForUpdatesCb, null, 300000, 72000000);
			GLib.Timeout.Add (30000, delegate { CheckForUpdates (); return false; });
			GLib.Timeout.Add (72000000, delegate { CheckForUpdates (); return true; });
		}
		
		private static void CheckForUpdatesCb ()
		{
			new Thread ((ThreadStart) delegate {
				bool updatesAvailable = PluginManager.UpdatesAvailable ();
				Gtk.Application.Invoke (delegate {
					if (updatesAvailable)
						NotificationIcon.NotifyUpdatesAvailable ();
				});
			}).Start ();
		}
	}
}