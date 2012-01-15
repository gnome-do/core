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
using System.Reflection;
using System.Collections.Generic;

using Mono.Addins;

using Do.Platform.Default;
using Do.Platform.Preferences;
using Do.Platform.ServiceStack;

namespace Do.Platform
{

	public class Services
	{
		static ICoreService core;
		static PathsService paths;
		static INetworkService network;
		static IWindowingService windowing;
		static AbstractSystemService system;
		static IKeyBindingService keybinder;
		static PreferencesFactory preferences;
		static IEnvironmentService environment;
		static INotificationsService notifications;
		static IPluginManagerService plugin_manager;
		static AbstractApplicationService application;
		static IUniverseFactoryService universe_factory;
		static AbstractPackageManagerService package_manager;

		// Logs are special; ensure we always have at least a default log service
		static IEnumerable<ILogService> logs = new ILogService[] { new Default.LogService () };

		/// <summary>
		/// Initializes the class. Must be called after Mono.Addins is initialized; if this is
		/// called and Mono.Addins is not initialized, an exception will be thrown.
		/// </summary>
		/// <remarks>
		/// For testing purposes, you may omit the call to Initialize and default services will be
		/// loaded when Mono.Addins is not available.
		/// </remarks>
		public static void Initialize ()
		{
			if (!AddinManager.IsInitialized) {
				// TODO find a better exception to throw.
				throw new Exception ("AddinManager was initialized before Services.");
			}

			LoadLogServices ();

			AddinManager.AddExtensionNodeHandler ("/Do/Service", OnServiceChanged);
			InitializeStrictServices ();
		}
		
		/// <summary>
		/// When a service is changed, we "dirty the cache".
		/// </summary>
		static void OnServiceChanged (object sender, ExtensionNodeEventArgs e)
		{			
			IService service = e.ExtensionObject as IService;

			switch (e.Change) {
				case ExtensionChange.Add:
					if (service is IInitializedService)
						(service as IInitializedService).Initialize ();
					break;
				case ExtensionChange.Remove:
					break;
			}

			// Loggers must be handled specially.
			if (service is ILogService)
				LoadLogServices ();

			// Dirty the appropriate cache.
			if (service is ICoreService)
				core = null;
			if (service is PathsService)
				paths = null;
			if (service is INetworkService)
				network = null;
			if (service is IWindowingService)
				windowing = null;
			// Although it is not obvious, this also takes care of the ISecurePreferences service.
			if (service is IPreferencesService)
				preferences = null;
			if (service is IEnvironmentService)
				environment = null;
			if (service is AbstractSystemService)
				system = null;
			if (service is INotificationsService)
				notifications = null;
			if (service is IUniverseFactoryService)
				universe_factory = null;			
			if (service is AbstractApplicationService)
				application = null;
		}

		// Log services need to be treated specially.  They're not lazy-initialised,
		// and cannot log while being initialised.

		static void LoadLogServices ()
		{
			IEnumerable<ILogService> tmp = null;
			if (AddinManager.IsInitialized) {
				tmp = AddinManager.GetExtensionObjects ("/Do/Service", true).OfType<ILogService> ();
				if (!tmp.Any ()) {
					tmp = null;
				}
			}
			logs = tmp ?? new ILogService [] { new Default.LogService () };
		}

		/// <summary>
		/// All available log services. Used primarily by the static Log class.
		/// </summary>
		public static IEnumerable<ILogService> Logs {
			get {
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

		public static PathsService Paths {
			get {
				if (paths == null)
					paths = LocateService<PathsService, DefaultPathsService> ();
				return paths;
			}
		}

		public static AbstractApplicationService Application {
			get {
				if (application == null)
					application = LocateService<AbstractApplicationService, DefaultApplicationService> ();
				return application;
			}
		}

		public static AbstractSystemService System {
			get {
				if (system == null)
					system = LocateService<AbstractSystemService, DefaultSystemService> ();
				return system;
			}
		}

		public static IWindowingService Windowing {
			get {
				if (windowing == null)
					windowing = LocateService<IWindowingService, Default.WindowingService> ();
				return windowing;
			}
		}
		
		public static IEnvironmentService Environment {
			get {
				if (environment == null)
					environment = LocateService<IEnvironmentService, Default.EnvironmentService> ();
				return environment;
			}
		}

		public static INotificationsService Notifications {
			get {
				if (notifications == null)
					notifications = LocateService<INotificationsService, Default.NotificationsService> ();
				return notifications;
			}
		}

		public static IUniverseFactoryService UniverseFactory {
			get {
				if (universe_factory == null)
					universe_factory = LocateService<IUniverseFactoryService, Default.UniverseFactoryService> ();
				return universe_factory;
			}
		}
		
		public static INetworkService Network {
			get {
				if (network == null)
					network = LocateService<INetworkService, Default.NetworkService> ();
				return network;
			}
		}
		
		public static AbstractPackageManagerService PackageManager {
			get {
				if (package_manager == null)
					package_manager = LocateService<AbstractPackageManagerService, Default.DefaultPackageManagerService> ();
				return package_manager;
			}
		}
		
		public static IPluginManagerService PluginManager {
			get {
				if (plugin_manager == null)
					plugin_manager = LocateService<IPluginManagerService, Default.PluginManagerService> ();
				return plugin_manager;
			}
		}
		
		public static IKeyBindingService Keybinder {
			get {
				if (keybinder == null)
					keybinder = LocateService<IKeyBindingService, Default.KeyBindingService> ();
				return keybinder;
			}
		}

		public static PreferencesFactory Preferences {
			get {
				if (preferences == null) {
					IPreferencesService service = LocateService<IPreferencesService, Default.PreferencesService> ();
					ISecurePreferencesService secureService = LocateService<ISecurePreferencesService, Default.SecurePreferencesService> ();
					preferences = new PreferencesFactory (service, secureService);
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
		
		static IEnumerable<TService> LocateServices<TService, TElse> ()
			where TService : class, IService
			where TElse : TService
		{
			IEnumerable<TService> services = LocateServices<TService> ();
			if (services.Any ()) {
				Log<Services>.Info ("Successfully located service of type {0}.", typeof (TService).Name);
			} else {
				Log<Services>.Fatal ("Service of type {0} not found. Using default service instead.", typeof (TService).Name);
				services = new [] { Activator.CreateInstance<TElse> () as TService };
			}
			return services;
		}
		
		static IEnumerable<TService> LocateServices<TService> ()
			where TService : IService
		{		
			if (AddinManager.IsInitialized) {
				return AddinManager.GetExtensionObjects ("/Do/Service", true).OfType<TService> ();
			} else {
				Log<Services>.Warn ("AddinManager is not initialized; only default services are available.");
				return Enumerable.Empty<TService> ();
			}
		}
		
		/// <summary>
		/// loops through the Property members of this class, and if it's a strict service gets it's value.
		/// This will in turn cause a LocateService call, and the appropriate service will be loaded.
		/// </summary>
		static void InitializeStrictServices ()
		{
			foreach (PropertyInfo property in typeof (Services).GetProperties ()) {
				Type returnType = property.PropertyType;
				if (returnType.GetInterface ("Do.Platform.IStrictService") != null) {
					// this looks stupid, but this is how you call the method on static classes.
					property.GetValue (null, null);
				}
			}
		}
	}
}
