// PreferencesImplementation.cs
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
	
	internal class PreferencesImplementation<TOwner> : IPreferences
		where TOwner : class
	{
		IPreferencesService Service { get; set; }
		IPreferencesService SecureService { get; set; }

		readonly string OwnerString = typeof (TOwner).FullName.Replace (".", "/");
		
		public PreferencesImplementation (IPreferencesService service, ISecurePreferencesService secureService)
		{
			Service = service;
			SecureService = new SecurePreferencesServiceWrapper (secureService);
			Service.PreferencesChanged += HandlePreferencesChanged;
		}
		
		void HandlePreferencesChanged (object o, PreferencesChangedEventArgs e)
		{
			if (e.Key.Length <= OwnerString.Length + 1 || e.Key.Substring(0, OwnerString.Length) != OwnerString)
				return;
			if (PreferencesChanged != null)
				PreferencesChanged (this, new PreferencesChangedEventArgs (e.Key.Substring(OwnerString.Length + 1), e.Value));
		}

		#region IPreferences
		
		public event EventHandler<PreferencesChangedEventArgs> PreferencesChanged;

		public T Get<T> (string key, T def)
		{
			T val;

			TryGet (Service, key, def, out val);
			return val;
		}

		/// <summary>
		/// Sets a preferences key to a given value.
		/// </summary>
		/// <param name="key">
		/// A <see cref="System.String"/> key (e.g. "key_binding") stored
		/// under Do's root preferences path.
		/// </param>
		/// <param name="val">
		/// A <see cref="T"/> value to set for the given key. Should be a
		/// simple (value) type or a string.
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/> indicating whether the key was
		/// successfuly set.
		/// </returns>
		public bool Set<T> (string key, T val)
		{
			return Set (Service, key, val);
		}
		
		public T GetSecure<T> (string key, T def)
		{
			T val;
			
			TryGet (SecureService, key, def, out val);
			return val;
		}

		public bool SetSecure<T> (string key, T val)
		{
			return Set (SecureService, key, val);
		}

		#endregion

		string AbsolutePathForKey (IPreferencesService service, string key)
		{
			if (key.StartsWith ("/"))
				return key;
			return string.Format ("{0}/{1}", OwnerString , key);
		}

		bool Set<T> (IPreferencesService service, string key, T val)
		{
			string keypath = AbsolutePathForKey (service, key);
			
			return service.Set (keypath, val);
		}

		bool TryGet<T> (IPreferencesService service, string key, T def, out T val)
		{
			bool success;

			success = TryGet (service, key, out val);
			if (!success) {
				success = Set (service, key, def);
				val = def;
			}
			return success;
		}

		bool TryGet<T> (IPreferencesService service, string key, out T val)
		{
			string keypath = AbsolutePathForKey (service, key);
			return service.TryGet (keypath, out val);
		}
	}
}
