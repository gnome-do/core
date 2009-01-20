// SystemService.cs
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

using NDesk.DBus;
using org.freedesktop.DBus;

using Do.Platform.ServiceStack;
using Do.Platform.Linux.DBus;

namespace Do.Platform.Linux
{
	
	public class SystemService : AbstractSystemService, IController, IInitializedService
	{

		const string PowerManagementName = "org.freedesktop.PowerManagement";
		const string PowerManagementPath = "/org/freedesktop/PowerManagement";

		[Interface(PowerManagementName)]
		interface IPowerManagement
		{
			bool GetOnBattery ();
		}
		
		public void Initialize ()
		{
			try {
				BusG.Init ();
			} catch (Exception e) {
				Log<SystemService>.Error ("Could not initialize dbus: {0}", e.Message);
				Log<SystemService>.Debug (e.StackTrace);
			}
		}
	
		public override bool GetOnBatteryPower ()
		{
			try {
				if (!Bus.Session.NameHasOwner (PowerManagementName))
					return false;
				IPowerManagement power = Bus.Session.GetObject<IPowerManagement> (PowerManagementName, new ObjectPath (PowerManagementPath));
				return power.GetOnBattery ();
			} catch (Exception e) {
				Log<SystemService>.Error ("Could not GetOnBattery: {0}", e.Message);
				Log<SystemService>.Debug (e.StackTrace);
			}
			return false;
		}

		public override void EnsureSingleApplicationInstance ()
		{
			try {
				IController controller = Registrar.GetControllerInstance ();
				if (controller == null) {
					// No IController found on the bus, so no other
					// instance is running. Register self.
					Log<SystemService>.Debug ("No other application instance detected. Continue startup.");
					Registrar.RegisterController (this);
				} else {
					// Another IController was found, so summon it
					// and exit.
					Log<SystemService>.Debug ("Existing application instance detected. Summon and bail.");
					controller.Summon ();
					System.Environment.Exit (0);
				}
			} catch (Exception e) {
				Log<SystemService>.Error ("Could not EnsureSingleApplicationInstance: {0}", e.Message);
				Log<SystemService>.Debug (e.StackTrace);
			}
		}

		public void Summon ()
		{
			Gdk.Threads.Enter ();
			Services.Windowing.SummonMainWindow ();
			Gdk.Threads.Leave ();
		}
	}
}
