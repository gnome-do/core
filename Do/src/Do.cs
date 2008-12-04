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
using System.Collections.Generic;

using Mono.Unix;

using Do.UI;
using Do.Core;
using Do.DBusLib;
using Do.Platform;

namespace Do {

	static class Do {
		
		static GConfXKeybinder keybinder;
		static Controller controller;
		static UniverseManager universe_manager;

		internal static void Main (string[] args)
		{
			Catalog.Init ("gnome-do", AssemblyInfo.LocaleDirectory);
			Gtk.Application.Init ();
			Gdk.Threads.Init ();

			DetectInstanceAndExit ();

			UniverseFactory.Initialize (new UniverseFactoryImplementation ());
			Paths.Initialize (new PathsImplementation ());
			Icons.Initialize (new Platform.Linux.IconsImplementation ());
			Windowing.Initialize (new WindowingImplementation ());

			PluginManager.Initialize ();

			Log.DisplayLevel = CorePreferences.QuietStart ? LogLevel.Error : LogLevel.Info;
			if (CorePreferences.Debug) Log.DisplayLevel = LogLevel.Debug;
			
			StatusIcon.Initialize (new Platform.Linux.StatusIconImplementation ());
			Platform.Notifications.Initialize (new Platform.Linux.NotificationsImplementation ());
			
			Util.Initialize ();

			try {
				Util.SetProcessName ("gnome-do");
			} catch (Exception e) {
				Log.Error ("Failed to set process name: {0}", e.Message);
			}
			
			Controller.Initialize ();
			UniverseManager.Initialize ();
			DBusRegistrar.RegisterController (Controller);
			
			keybinder = new GConfXKeybinder ();
			SetupKeybindings ();

			if (!CorePreferences.QuietStart)
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

		public static Controller Controller {
			get {
				if (controller == null)
					controller = new Controller ();
				return controller;
			}
		}

		public static UniverseManager UniverseManager {
			get {
				if (universe_manager == null)
					universe_manager = new UniverseManager ();
				return universe_manager;
			}
		}
		
		static void SetupKeybindings ()
		{
			keybinder.Bind (CorePreferences.SummonKeyBindingPath,
				CorePreferences.SummonKeyBinding, OnActivate);
		}
		
		static void OnActivate (object sender, EventArgs args)
		{
			controller.Summon ();
		}
	}
}
