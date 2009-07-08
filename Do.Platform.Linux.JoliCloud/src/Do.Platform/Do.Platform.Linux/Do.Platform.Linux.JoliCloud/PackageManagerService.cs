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
using System.IO;
using System.Threading;
using System.Collections.Generic;

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
		
		Dictionary<string, string> PackagePluginMap;
		
		~PackageManagerService ()
		{
			if (daemon != null)
				daemon.ActionProcessed -= HandleActionProcessed;
		}
		
		/// <summary>
		/// Find jolicloud on the bus
		/// </summary>
		public override void Initialize ()
		{
			session_bus = Bus.Session.GetObject<IBus> ("org.freedeskop.DBus", new ObjectPath ("/org/freedesktop/DBus"));
			session_bus.NameOwnerChanged += HandleNameOwnerChanged;
			
			// this call will instaniate the daemon, as well as make sure we also got a DBus object
			daemon = GetIJoliCloudDaemonObject (ObjectPath);
			daemon.ActionProcessed += HandleActionProcessed;
			
			LoadJolicloudPackageMap ();
			base.Initialize ();
		}
		
		//// <value>
		/// This needs to be static to ensure that we only ever have one daemon instance aquired
		/// </value>
		IJolicloudDaemon Daemon {
			get {
				if (daemon == null) {
					daemon = GetIJoliCloudDaemonObject (ObjectPath);
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
			Log<PackageManagerService>.Debug ("got a {0} action", action);
			Addin addin = null;
			string cleanName = "";
			
			if (action != "install" || packages.Length < 1 || DontShowPluginAvailableDialog)
				return;
			
			// first check our explicit mapping, then try and clean up the package into something that
			// might be a plugin name.
			if (!PackagePluginMap.TryGetValue (packages [0], out cleanName))
				cleanName = HumanNameFromPackageName (packages [0]);
			
			addin = MaybePluginForPackage (cleanName);
			
			if (addin == null || addin.Enabled)
				return;
			
			Log<PackageManagerService>.Debug ("Showing dialog for {0}", cleanName);
			new PluginAvailableDialog (cleanName, addin);
		}

		/// <summary>
		/// Maps an installed package to a Do plugin
		/// the next release of Mono.Addins will have support for tagging in the addin manifest, when this comes out 
		/// we'll use it, but for now I'm just going to define a mapping. With the jolicloud case this isn't a big deal
		/// becuase the set of packages available from the installer interface is relatively small.
		/// </summary>
		void LoadJolicloudPackageMap ()
		{
			PackagePluginMap = new Dictionary<string, string> {
				{"skype", "skype"},
				{"pidgin", "pidgin"},
				{"evolution", "evolution"},
				{"nautilus-dropbox", "dropbox"},
				{"prism-webapp-flickr", "flickr"},
				{"prism-webapp-youtube", "youtube"},
				{"prism-webapp-twitter", "microblogging"},
				{"prism-webapp-gmail", "google contacts"},
				{"prism-webapp-google-docs", "google docs"},
				{"prism-webapp-google-calendar", "google calendar"}
			};
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
			
			package = package.Replace ("-", " ");
			
			return package.ToLower ();
		}

#region DBus handling
		void HandleNameOwnerChanged (string name, string old_owner, string new_owner)
		{
			if (name == BusName)
				Log.Debug ("{0} is not owned by {1}, now {2} is our daddy", name, old_owner, new_owner);
			// if the jolicloud daemon gets released, we should drop our object
			if (daemon != null && name == BusName) {
				Log<PackageManagerService>.Debug ("DROPPING DAEMON");
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
		}
#endregion
	}
}