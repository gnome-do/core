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

using Do.Universe;
using Do.Platform;

namespace Do.Platform.Default
{
	
	public class CoreService : ICoreService
	{
		#region ICoreService

		#region IObject
		
		public string Name {
			get { return Catalog.GetString ("Default Core Service"); }
		}

		public string Description {
			get { return Catalog.GetString ("Just prints warnings and returns default values."); }
		}

		public string Icon {
			get { return "gnome-do"; }
		}

		#endregion
		
		public string GetUID (IObject o)
		{
			Log.Warn ("Default ICoreService cannot get UIDs.");
			return string.Empty;
		}
		
		public IObject GetIObject (string uid)
		{
			Log.Warn ("Default ICoreService cannot get IObjects.");
			return this;
		}

		public IObject Unwrap (IObject o)
		{
			Log.Warn ("Default ICoreService cannot unwrap IObjects.");
			return o;
		}

		#endregion
		
	}

}
