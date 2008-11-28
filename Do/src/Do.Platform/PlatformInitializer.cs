// PlatformInitializer.cs
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
using System.IO;
using Env = System.Environment;

namespace Do.Platform
{
	
	internal static class PlatformInitializer
	{
		
		public static void Initialize ()
		{
			Core.Initialize (new CoreImplementation ());
			UniverseFactory.Initialize (new UniverseFactoryImplementation ());
			Environment.Initialize (new EnvironmentImplementation ());
			Paths.Initialize (new PathsImplementation ());
			Preferences.Initialize (new GConfPreferencesImplementation ());

			#region Log initialization
			Log.Initialize ();
			Log.AddImplementation (new ConsoleLogImplementation ());
			if (CorePreferences.WriteLogToFile) {
				if (File.Exists (Paths.Log)) File.Delete (Paths.Log);
				Log.AddImplementation (new Common.FileLogImplementation ());
			}
			Log.LogLevel = CorePreferences.QuietStart ? Log.Level.Error : Log.Level.Info;
			if (CorePreferences.Debug) Log.LogLevel = Log.Level.Debug;
			#endregion
			
			#region Icons initialization
			Icons.Initialize (new Linux.IconsImplementation ());
			#endregion
			
			#region Windowing initialization
			Windowing.Initialize (new Linux.WindowingImplementation ());
			#endregion
			
			#region StatusIcon initialization
			StatusIcon.Initialize (new Linux.StatusIconImplementation ());
			#endregion
			
			#region Notifications initialization
			Notifications.Initialize (new Linux.NotificationsImplementation ());
			#endregion
		}
	}
}