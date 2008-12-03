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
		public static ICoreService Core { get; private set; }
		public static IEnvironmentService Environment { get; private set; }
		
		public static void Initialize ()
		{
			Core = LocateService<ICoreService, Default.CoreService> ();
			Environment = LocateService<IEnvironmentService, Default.EnvironmentService> ();
		}

		static TService LocateService<TService, TElse> ()
			where TService : class, IService
			where TElse : TService
		{
			TService service = LocateService<TService> ();
			if (service == null) {
				Log.Fatal ("Service of type \"{0}\" not found. Using default service instead.", typeof (TService).Name);
				service = Activator.CreateInstance<TElse> () as TService;
			} else {
				Log.Info ("Successfully located service of type \"{0}\".", typeof (TService).Name);
			}
			return service;
		}
		
		static TService LocateService<TService> ()
			where TService : class, IService
		{
			return LocateServices<TService> ().FirstOrDefault ();
		}
		
		static IEnumerable<TService> LocateServices<TService> ()
			where TService : class, IService
		{
			Log.Info ("Looking for services of type \"{0}\"...", typeof (TService).Name);			
			return AddinManager.GetExtensionObjects ("/Do/Service", true)
				.OfType<TService> ();
		}
	}
}
