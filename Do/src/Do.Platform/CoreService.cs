// CoreService.cs
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

using Mono.Unix;

using Do.Core;
using Do.Universe;
using Do.Platform;

namespace Do.Platform
{
	
	public class CoreService : ICoreService
	{
		#region ICoreService

		#region IObject
		
		public string Name {
			get { return Catalog.GetString ("Do Core Service"); }
		}

		public string Description {
			get { return Catalog.GetString ("Provides plugins with safe access to internal application state."); }
		}

		public string Icon {
			get { return "gnome-do"; }
		}

		#endregion
		
		public string GetUID (IObject o)
		{
			return (DoObject.Wrap (o) as DoObject).UID;
		}

		public IObject GetIObject (string uid)
		{
			IObject o;
			Do.UniverseManager.TryGetObjectForUID (uid, out o);
			return o;
		}

		public IObject Unwrap (IObject o)
		{
			return DoObject.Unwrap (o);
		}

		#endregion
		
	}

}
