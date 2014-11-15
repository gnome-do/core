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
using System.Diagnostics;
using System.Collections.Generic;

using Mono.Unix;

using Do.UI;
using Do.Core;
using Do.Platform;
using Do.Platform.Common;

namespace Do {

	static class Do {
		
		static Controller controller;
		static UniverseManager universe_manager;

		public static CorePreferences Preferences { get; private set; }
		
		internal static void Main (string [] args)
		{
			Catalog.Init ("gnome-do", AssemblyInfo.LocaleDirectory);
			Gtk.Application.Init ();

			// We are conservative with the log at first.
			Log.DisplayLevel = LogLevel.Error;
			if (CorePreferences.PeekDebug)
				Log.DisplayLevel = LogLevel.Debug;

			PluginManager.Initialize ();
			Services.System.EnsureSingleApplicationInstance ();

			Preferences = new CorePreferences ();

			// Now we can set the preferred log level.
			if (Preferences.QuietStart)
				Log.DisplayLevel = LogLevel.Error;
			// Check for debug again in case QuietStart is also set.
			if (Preferences.Debug)
				Log.DisplayLevel = LogLevel.Debug;

			try {
				Services.System.SetProcessName ("gnome-do");
			} catch (Exception e) {
				Log.Error ("Failed to set process name: {0}", e.Message);
			}
			
			Controller.Initialize ();
			UniverseManager.Initialize ();
			
			if (!Preferences.QuietStart)
				Controller.Summon ();

			Gtk.Application.Run ();

			RelevanceProvider.Serialize (RelevanceProvider.DefaultProvider);
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
	}
}