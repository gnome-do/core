//   PackageManagerService.cs
// 
//   GNOME Do is the legal property of its developers. Please refer to the
//   COPYRIGHT file distributed with this source distribution.
// 
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//  
//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   GNU General Public License for more details.
//  
//   You should have received a copy of the GNU General Public License
//   along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Threading;

using NDesk.DBus;
using org.freedesktop.DBus;

using Mono.Addins;

using Do.Platform;
using Do.Platform.Linux;
using Do.Platform.ServiceStack;

namespace Do.Platform.Linux.JoliCloud
{
	
	[Interface ("org.jolicloud.JolicloudDaemon")]
	interface IJolicloudDaemon
	{
		event ActionProcessedEventHandler ActionProcessed;
	}
	
	delegate void ActionProcessedEventHandler (string action, string [] packages, bool success, string error);
		
	/// <summary>
	/// Listens to the JoliCloud daemon for package install events, and offers to install an appropriate
	/// plugin if one is found.
	/// </summary>
	public class PackageManagerService : AbstractPackageManagerService, IStrictService
	{
		const string ObjectPath = "/SoftwareManager";
		const string BusName = "org.jolicloud.JolicloudDaemon";
		
		IBus session_bus;
		IJolicloudDaemon daemon;
		
		/// <summary>
		/// Find jolicloud on the bus
		/// </summary>
		public override void Initialize ()
		{
			session_bus = Bus.Session.GetObject<IBus> ("org.freedeskop.DBus", new ObjectPath ("/org/freedesktop/DBus"));
			session_bus.NameOwnerChanged += HandleNameOwnerChanged;
			
			// this call will instaniate the daemon, as well as make sure we also got a DBus object
			if (Daemon == null)
				Log<PackageManagerService>.Debug ("Failed to locate JoliCloud daemon on DBus.");
			
			base.Initialize ();
		}
		
		IJolicloudDaemon Daemon {
			get {
				if (daemon == null) {
					daemon = GetIJoliCloudDaemonObject (ObjectPath);
					daemon.ActionProcessed += HandleActionProcessed;
					Log<PackageManagerService>.Debug ("Aquired instance of JolicloudDaemon");
				}
				return daemon;
			}
		}

		/// <summary>
		/// When a package manager action is performed we receive a signal, this method processes the signal
		/// and decides how to act.
		/// </summary>
		/// <param name="action">
		/// A <see cref="System.String"/> the action performed by the package manager
		/// </param>
		/// <param name="packages">
		/// A <see cref="System.String"/> the packages touched by the action
		/// </param>
		/// <param name="success">
		/// A <see cref="System.Boolean"/> did the action succeed
		/// </param>
		/// <param name="error">
		/// A <see cref="System.String"/> errors
		/// </param>
		void HandleActionProcessed (string action, string[] packages, bool success, string error)
		{
			Addin addin;
			string cleanName;
			
			if (action != "install" || packages.Length < 1 || DontShowPluginAvailableDialog)
				return;
				
			cleanName = HumanNameFromPackageName (packages [0]);
			if ((addin = MaybePluginForPackage (cleanName)) == null && !addin.Enabled)
				return;
			
			new PluginAvailableDialog (cleanName, addin);
		}
		
		/// <summary>
		/// Attempts to clean up a package name into something a little bit more friendly.
		/// </summary>
		/// <param name="package">
		/// A <see cref="System.String"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		string HumanNameFromPackageName (string package)
		{
			if (package.Contains ("prism-webapp"))
				package = package.Substring ("prism-webapp".Length + 1);
			
			package.Replace ('-', ' ');
			
			return package.ToLower ();
		}
		
#region DBus handling
		void HandleNameOwnerChanged (string name, string old_owner, string new_owner)
		{
			// if the jolicloud daemon gets released, we should drop our object
			if (Daemon != null && name == BusName) {
				Daemon.ActionProcessed -= HandleActionProcessed;
				daemon = null;
			}
		}
		
		IJolicloudDaemon GetIJoliCloudDaemonObject (string objectPath)
		{
			MaybeStartDaemon ();
			return Bus.Session.GetObject<IJolicloudDaemon> (BusName, new ObjectPath (objectPath));
		}
		
		bool DaemonIsRunning {
			get { return Bus.Session.NameHasOwner (BusName); }
		}
		
		void MaybeStartDaemon ()
		{
			if (DaemonIsRunning)
				return;
			
			Bus.Session.StartServiceByName (BusName);
			Thread.Sleep (5000);

			if (!DaemonIsRunning)
				throw new Exception (string.Format("Name {0} has no owner.", BusName));
		}
#endregion
	}
}
	