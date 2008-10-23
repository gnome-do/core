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
		
		static string [] args;
		static GConfXKeybinder keybinder;
		
		static DoPreferences preferences;
		static CommandLinePreferences cliprefs;
		static Controller controller;
		static IUniverseManager universe_manager;
		static NotificationIcon notification_icon;
		
		static DateTime perfTime;

		public static void Main (string[] args)
		{
			perfTime = DateTime.Now;
			Do.args = args;
			
			Catalog.Init ("gnome-do", "/usr/local/share/locale");
			Gtk.Application.Init ();

			DetectInstanceAndExit ();
			Log.Initialize ();
			Util.Initialize ();

			Gdk.Threads.Init ();			
			
			if (CLIPrefs.Debug)
				Log.LogLevel = LogEntryType.Debug;

			try {
				Util.SetProcessName ("gnome-do");
			} catch (Exception e) {
				Log.Error ("Failed to set process name: {0}", e.Message);
			}

			PluginManager.Initialize ();
			Controller.Initialize ();
			UniverseManager.Initialize ();
			DBusRegistrar.RegisterController (Controller);
			
			keybinder = new GConfXKeybinder ();
			SetupKeybindings ();
			
			//whoever keeps pulling this out. STOP. PLEASE.
			notification_icon = new NotificationIcon ();
			
			// Kick-off update timers.			
			GLib.Timeout.Add (5 * 60 * 100, delegate {
				CheckForUpdates ();
				return false;
			});
			GLib.Timeout.Add (2 * 60 * 60 * 100, delegate {
				CheckForUpdates ();
				return true;
			});

			if (!Preferences.QuietStart)
				Controller.Summon ();
			
			AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
				
			Gtk.Application.Run ();
		}
		
		/// <summary>
		/// Used to deal with older versions of mono hanging on exit.
		/// </summary>
		static void OnProcessExit (object o, EventArgs args)
		{
			Thread th = new Thread (new ThreadStart (delegate {
				System.Threading.Thread.Sleep (1000);
				Console.WriteLine ("Process failed to exit cleanly, hard killing");
				System.Diagnostics.Process process =  System.Diagnostics.Process.GetCurrentProcess ();
				process.Kill ();
			}));
			
			th.IsBackground = true;
			th.Start ();
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
					preferences = new DoPreferences (); 
			}
		}
		
		public static CommandLinePreferences CLIPrefs {
			get { return cliprefs ?? 
					cliprefs = new CommandLinePreferences (args); 
			}
		}

		public static Controller Controller {
			get {
				return controller ??
					controller = new Controller ();
			}
		}

		public static IUniverseManager UniverseManager {
			get {
				return universe_manager ??
					universe_manager = new SimpleUniverseManager ();
			}
		}
		
		public static NotificationIcon NotificationIcon {
			get {
				return notification_icon ??
					notification_icon = new NotificationIcon ();
			}
		}
		
		public static void PrintPerf (string caller)
		{
			TimeSpan ts = DateTime.Now.Subtract (perfTime);
			Console.WriteLine(caller + ": " + ts.TotalMilliseconds);
			
			perfTime = DateTime.Now;
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
			Thread th = new Thread ((ThreadStart) delegate {
				if (PluginManager.UpdatesAvailable ())
					Gtk.Application.Invoke (delegate {
						NotificationIcon.NotifyUpdatesAvailable ();
					});
			});
			
			th.IsBackground = true;
			th.Start ();
		}
	}
}
