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
using System.IO;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;

#if USE_DBUS_SHARP
using DBus;
#else
using NDesk.DBus;
#endif
using org.freedesktop.DBus;

using Do.Platform.ServiceStack;
using Do.Platform.Linux.DBus;

using GLib;

using Mono.Unix.Native;

namespace Do.Platform.Linux
{
	
	public class SystemService : AbstractSystemService, IController, IInitializedService
	{
		delegate void BoolDelegate (bool val);
		
		const string PowerManagementName = "org.freedesktop.PowerManagement";
		const string PowerManagementPath = "/org/freedesktop/PowerManagement";
		const string DeviceKitPowerName = "org.freedesktop.DeviceKit.Power";
		const string DeviceKitPowerPath = "/org/freedesktop/DeviceKit/Power";
		const string AutoStartKey = "Hidden";
		
		[Interface(PowerManagementName)]
		interface IPowerManagement
		{
			bool GetOnBattery ();
			event BoolDelegate OnBatteryChanged;
		}
		
		[Interface(DeviceKitPowerName)]
		interface IDeviceKitPower : org.freedesktop.DBus.Properties
		{
			event Action OnChanged;
		}
		
		[DllImport ("libc")]
		private static extern int prctl (int option, byte [] arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5);

		private static int prctl (int option, string arg2)
		{
			return prctl (option, Encoding.ASCII.GetBytes (arg2 + "\0"), IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
		}
		
		bool on_battery;
		
		IPowerManagement power;
		IDeviceKitPower devicekit;
		KeyFile.GKeyFile autostartfile;
		
		public void Initialize ()
		{
			// Set a sane default value for on_battery.  Thus, if we don't find a working power manager
			// we assume we're not on battery.
			on_battery = false;
			try {
				BusG.Init ();
				if (Bus.System.NameHasOwner (DeviceKitPowerName)) {
					devicekit = Bus.System.GetObject<IDeviceKitPower> (DeviceKitPowerName, new ObjectPath (DeviceKitPowerPath));
					devicekit.OnChanged += DeviceKitOnChanged;
					on_battery = (bool) devicekit.Get (DeviceKitPowerName, "on-battery");
					Log<SystemService>.Debug ("Using org.freedesktop.DeviceKit.Power for battery information");
				} else if (Bus.Session.NameHasOwner (PowerManagementName)) {
					power = Bus.Session.GetObject<IPowerManagement> (PowerManagementName, new ObjectPath (PowerManagementPath));
					power.OnBatteryChanged += PowerOnBatteryChanged;
					on_battery = power.GetOnBattery ();
					Log<SystemService>.Debug ("Using org.freedesktop.PowerManager for battery information");
				}
			} catch (Exception e) {
				Log<SystemService>.Error ("Could not initialize dbus: {0}", e.Message);
				Log<SystemService>.Debug (e.StackTrace);
			}
		}

		void PowerOnBatteryChanged (bool val)
		{
			on_battery = val;
			OnOnBatteryChanged ();
		}
		
		void DeviceKitOnChanged ()
		{
			bool newState = (bool) devicekit.Get (DeviceKitPowerName, "on-battery");
			if (on_battery != newState) {
				on_battery = newState;
				OnOnBatteryChanged ();
			}
		}
		
		public override bool GetOnBatteryPower ()
		{
			return on_battery;
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
		

		string AutoStartDir {
			get {
				return System.IO.Path.Combine (
					Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData), "autostart");
		    }
		}
		
		string AutoStartFileName {
		  get {
		      return System.IO.Path.Combine (AutoStartDir, "gnome-do.desktop");
		    }
		}
		
		KeyFile.GKeyFile AutoStartFile {
			get {
				if (autostartfile != null) 
					return autostartfile;
				
				try {
					autostartfile = new KeyFile.GKeyFile(AutoStartFileName);
				} catch (GLib.GException loadException) {
					Log<SystemService>.Info ("Unable to load existing autostart file: {0}", loadException.Message);
					Log<SystemService>.Info ("Writing new autostart file to {0}", AutoStartFileName);
					autostartfile = new KeyFile.GKeyFile (System.IO.Path.Combine (AssemblyInfo.InstallData, "applications/gnome-do.desktop"));
					if (!Directory.Exists (AutoStartDir))
						Directory.CreateDirectory (AutoStartDir);

					// This *enables* autostart, by setting the "Hidden" key (which disables autostart) to false.
					// Explicitly setting this key fixes LP #398303; otherwise our IsAutoStartEnabled method won't find
					// the AutoStartKey, and will erroneously return false..
					autostartfile.SetBoolean ("Desktop Entry", AutoStartKey, false);
					autostartfile.Save (AutoStartFileName);
				}
				return autostartfile;
			}
		}
		
		public override bool IsAutoStartEnabled ()
		{
			try {
				return AutoStartFile.GetBoolean ("Desktop Entry", AutoStartKey);
			} catch (GLib.GException)	{
				Log<SystemService>.Info ("Failed to find autostart key in autostart file, assuming enabled");
			}
			return true;
		}
		
		public override void SetAutoStartEnabled (bool enabled)
		{
			try {
				AutoStartFile.SetBoolean ("Desktop Entry", AutoStartKey, !enabled);
				try {
					AutoStartFile.Save (AutoStartFileName);
				} catch (GLib.GException e) {
					Log<SystemService>.Error ("Failed to update autostart file: {0}", e.Message);
				}
			}
			catch (Exception e) {
				Log<SystemService>.Error("Failed to access autostart file: {0}", e.Message);
			}
		}
		
		public override void SetProcessName (string name)
		{
			prctl (15 /* PR_SET_NAME */, name);
		}
	}
}
