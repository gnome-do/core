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
using System.Reflection;

using NDesk.DBus;
using org.freedesktop.DBus;

using Do.Platform.ServiceStack;
using Do.Platform.Linux.DBus;

using Gnome;

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
		
		bool on_battery;
		
		IPowerManagement power;
		IDeviceKitPower devicekit;
		DesktopItem autostartfile;
		
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

		string InitialAutoStartFile ()
		{
			System.IO.Stream s = Assembly.GetExecutingAssembly ().GetManifestResourceStream ("gnome-do.desktop");
			using (System.IO.StreamReader sr = new System.IO.StreamReader (s)) {
				return sr.ReadToEnd ();
			}
		}
		
		string AutoStartUri {
			get {
				return Gnome.Vfs.Uri.GetUriFromLocalPath (AutoStartFileName);
			}
		}
		
		DesktopItem AutoStartFile {
			get {
				if (autostartfile != null) 
					return autostartfile;
				
				try {
					autostartfile = DesktopItem.NewFromUri (AutoStartUri, DesktopItemLoadFlags.NoTranslations);
				} catch (GLib.GException loadException) {
					Log<SystemService>.Info ("Unable to load existing autostart file: {0}", loadException.Message);
					Log<SystemService>.Info ("Writing new autostart file to {0}", AutoStartFileName);
					autostartfile = DesktopItem.NewFromFile (System.IO.Path.Combine (AssemblyInfo.InstallData, "applications/gnome-do.desktop"),
					                                         DesktopItemLoadFlags.NoTranslations);
					try {
						autostartfile.Save (AutoStartUri, true);
						autostartfile.Location = AutoStartUri;
					} catch (Exception e) {
						Log<SystemService>.Error ("Failed to write initial autostart file: {0}", e.Message);
					}
				}
				return autostartfile;
			}
		}
		
		public override bool IsAutoStartEnabled ()
		{
			DesktopItem autostart = AutoStartFile;
			
			if (!autostart.Exists ()) {
				Log<SystemService>.Error ("Could not open autostart file {0}", AutoStartUri);
			}
			
			if (autostart.AttrExists (AutoStartKey)) {
				return !String.Equals(autostart.GetString (AutoStartKey), "true", StringComparison.OrdinalIgnoreCase);
			}
			return false;
		}
		
		public override void SetAutoStartEnabled (bool enabled)
		{
			DesktopItem autostart = AutoStartFile;
			
			autostart.SetBoolean (AutoStartKey, !enabled);
			try {
				autostart.Save (null, true);
			} catch (Exception e) {
				Log<SystemService>.Error ("Failed to update autostart file: {0}", e.Message);
			}
		}
	}
}
