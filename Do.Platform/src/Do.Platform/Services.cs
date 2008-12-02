// ServiceManager.cs created with MonoDevelop
// User: david at 5:42 PM 12/1/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
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
		public static IEnvironmentService Environment { get; private set; }
		
		public static void Initialize ()
		{
			Environment = LocateService<IEnvironmentService, Default.EnvironmentService> ();
		}

		static TService LocateService<TService, TElse> ()
			where TService : IService
			where TElse : TService
		{
			TService service = LocateService<TService> ();
			if (service == null) {
				Log.Warn ("Service of type \"{0}\" not found. Using default service instead.", typeof (TService).Name);
				service = (TService) Activator.CreateInstance (typeof (TElse));
			}
			return service;
		}
		
		static TService LocateService<TService> ()
			where TService : IService
		{
			return LocateServices<TService> ().FirstOrDefault ();
		}
		
		static IEnumerable<TService> LocateServices<TService> ()
			where TService : IService
		{
			return AddinManager.GetExtensionObjects ("/Do/Service", typeof (TService), true).Cast<TService> ();
		}
	}
}
