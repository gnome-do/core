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
	
	internal class PreferencesImplementation : IPreferences
	{

		string RootPath { get; set; }
		public IPreferencesService Service { get; set; }
		public IPreferencesService SecureService { get; set; }
		
		public PreferencesImplementation (IPreferencesService service, IPreferencesService secureService)
			: this (service, secureService, "")
		{
		}

		public PreferencesImplementation (IPreferencesService service, IPreferencesService secureService, string rootPath)
		{
			RootPath = rootPath;
			
			Service = service;
			SecureService = secureService;
		}
		
		void OnPreferenceChanged (string key, object oldValue, object newValue)
		{
			if (PreferenceChanged == null) return;
			
			PreferenceChangedEventArgs args
				= new PreferenceChangedEventArgs (key, oldValue, newValue);
			PreferenceChanged (this, args);
		}

		#region IPreferences
		
		public event EventHandler<PreferenceChangedEventArgs> PreferenceChanged;

		public string AbsolutePathForKey (string key)
		{
			return AbsolutePathForKey (Service, key);
		}

		public string AbsolutePathForSecureKey (string key)
		{
			return AbsolutePathForKey (SecureService, key);
		}
				
		public string this [string key] {
			get {
				return Get (key, "");
			}
			set {
				Set (key, value);
			}
		}

		public T Get<T> (string key, T def)
		{
			T val;

			TryGet (key, def, out val);
			return val;
		}
		
		public bool TryGet<T> (string key, T def, out T val)
		{
			return TryGet (Service, key, def, out val);
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
			return Set<T> (Service, key, val);
		}
		
		/// <summary>
		/// Try to read a value for a given key.
		/// </summary>
		/// <param name="key">
		/// A <see cref="System.String"/> key (e.g. "key_binding").
		/// </param>
		/// <param name="val">
		/// A <see cref="T"/> value if the key was found.
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/> indicating whether or not the value
		/// was read successfully.
		/// </returns>
		public bool TryGet<T> (string key, out T val)
		{
			return TryGet (Service, key, out val);
		}

		public T SecureGet<T> (string key, T def)
		{
			T val;
			
			TryGet<T> (SecureService, key, def, out val);
			return val;
		}

		public bool SecureSet<T> (string key, T val)
		{
			return Set<T> (SecureService, key, val);
		}

		public bool SecureTryGet<T> (string key, out T val)
		{
			return TryGet (SecureService, key, out val);
		}

		public bool SecureTryGet<T> (string key, T def, out T val)
		{
			return TryGet (SecureService, key, def, out val);
		}

		#endregion

		string AbsolutePathForKey (IPreferencesService service, string key)
		{
			if (key.StartsWith ("/"))
				return key;
			return service.AbsolutePathForKey (string.Format ("{0}/{1}", RootPath , key));
		}

		bool Set<T> (IPreferencesService service, string key, T val)
		{
			T oldValue;
			string keypath = AbsolutePathForKey (service, key);
			
			if (!service.TryGet (keypath, out oldValue))
				oldValue = default (T);
			
			if (service.Set (keypath, val)) {
				OnPreferenceChanged (key, oldValue, val);
				return true;
			}
			return false;
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
