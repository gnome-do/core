// PreferencesFactory.cs
//
// GNOME Do is the legal property of its developers. Please refer to the
// COPYRIGHT file distributed with this
// source distribution.
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

using Do.Platform;

namespace Do.Platform.Preferences
{
	
	public class PreferencesFactory
	{
		
		internal IPreferencesService Service { get; private set; }
		internal ISecurePreferencesService SecureService { get; private set; }
		
		public PreferencesFactory (IPreferencesService service, ISecurePreferencesService secureService)
		{
			Service = service;
			SecureService = secureService;
		}

		/// <summary>
		/// IPreferences factory method with a little bit of type-level protection.
		/// </summary>
		/// <returns>
		/// A <see cref="IPreferences"/> for exclusive use by type TOwner. TOwner
		/// should be a type that the caller has exclusive access to (e.g. an
		/// internal class).
		/// </returns>
		public IPreferences Get<TOwner> () where TOwner : class
		{
			return new PreferencesImplementation<TOwner> (Service, SecureService);
		}
	}
}
