// Services.cs
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
using System.Linq;
using System.Collections.Generic;

using Mono.Addins;

using Do.Platform.ServiceStack;

namespace Do.Platform
{
	
	public class Services
	{

		static ICoreService core;
		static IEnvironmentService environment;
		static IPreferencesFactory preferences;
		static IEnumerable<ILogService> logs;
		
		public static void Initialize ()
		{
			if (!AddinManager.IsInitialized) {
				throw new Exception ("Serivces.Initialize called before AddinManager was initialized.");
			}
			AddinManager.AddExtensionNodeHandler ("/Do/Service", OnServiceChanged);
		}

		/// <summary>
		/// When a service is changed, we "dirty the cache".
		/// </summary>
		/// <param name="s">
		/// A <see cref="System.Object"/>
		/// </param>
		/// <param name="args">
		/// A <see cref="ExtensionNodeEventArgs"/>
		/// </param>
		static void OnServiceChanged (object s, ExtensionNodeEventArgs args)
		{
			IService service = args.ExtensionObject as IService;

			switch (args.Change) {
			case ExtensionChange.Add:
				if (service is IInitializedService)
					(service as IInitializedService).Initialize ();
				break;
			case ExtensionChange.Remove:
				break;
			}

			// Dirty the appropriate cache.
			if (service is ICoreService)
				core = null;
			if (service is IEnvironmentService)
				environment = null;
			if (service is IPreferencesService)
				preferences = null;
			if (service is ILogService)
				logs = null;
		}

		public static IEnumerable<ILogService> Logs {
			get {
				if (logs == null)
					logs = LocateServices<ILogService, Default.LogService> ().ToArray ();
				return logs;
			}
		}

		public static ICoreService Core {
			get {
				if (core == null)
					core = LocateService<ICoreService, Default.CoreService> ();
				return core;
			}
		}
		
		public static IEnvironmentService Environment {
			get {
				if (environment == null)
					environment = LocateService<IEnvironmentService, Default.EnvironmentService> ();
				return environment;
			}
		}
			
		public static IPreferencesFactory Preferences {
			get {
				if (preferences == null) {
					IPreferencesService service = LocateService<IPreferencesService, Default.PreferencesService> ();
					preferences = new PreferencesFactory (service);
				}
				return preferences;
			}
		}

		static TService LocateService<TService, TElse> ()
			where TService : class, IService
			where TElse : TService
		{
			return LocateServices<TService, TElse> ().First ();
		}
		
		static TService LocateService<TService> ()
			where TService : IService
		{
			return LocateServices<TService> ().FirstOrDefault ();
		}

		static IEnumerable<TService> LocateServices<TService, TElse> ()
			where TService : class, IService
			where TElse : TService
		{
			IEnumerable<TService> services = LocateServices<TService> ();
			if (services.Any ()) {
				Log.Info ("Successfully located services of type {0}.", typeof (TService).Name);
			} else {
				Log.Fatal ("Services of type {0} not found. Using default service instead.", typeof (TService).Name);
				services = new [] { Activator.CreateInstance<TElse> () as TService };
			}
			return services;
		}
		
		static IEnumerable<TService> LocateServices<TService> ()
			where TService : IService
		{		
			return AddinManager.GetExtensionObjects ("/Do/Service", true).OfType<TService> ();
		}
	}
}
