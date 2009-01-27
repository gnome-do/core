// DockServices.cs
// 
// Copyright (C) 2009 GNOME Do
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
using System.Collections.Generic;
using System.Linq;

namespace Docky.Core
{
	
	
	public static class DockServices
	{
		static List<IDockService> services = new List<IDockService> ();
		
		static IItemsService items_service;
		static IDrawingService drawing_service;
		static IDoInteropService do_interop_service;
		static IPainterService painter_service;
		
		public static IItemsService ItemsService {
			get { return items_service ?? (items_service = new Default.ItemsService () as IItemsService); }
		}

		public static IDrawingService DrawingService {
			get { return drawing_service ?? (drawing_service = new Default.DrawingService () as IDrawingService); }
		}

		public static IDoInteropService DoInteropService {
			get { return do_interop_service ?? (do_interop_service = LoadService<IDoInteropService, Default.DoInteropService> ()); }
		}

		public static IPainterService PainterService {
			get { return painter_service ?? (painter_service = LoadService<IPainterService, Default.PainterService> ()); }
		}

		public static void RegisterService (IDockService service)
		{
			services.Add (service);
		}

		static TService LoadService<TService, TElse> ()
			where TService : class, IDockService
			where TElse : TService
		{
			if (services.OfType<TService> ().Any ()) {
				return services.OfType<TService> ().First () as TService;
			} else {
				return Activator.CreateInstance<TElse> () as TService;
			}
		}
	}
}
