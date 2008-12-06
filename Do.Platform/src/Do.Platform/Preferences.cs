/* Preferences.cs
 *
 * GNOME Do is the legal property of its developers. Please refer to the
 * COPYRIGHT file distributed with this
 * source distribution.
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;

namespace Do.Platform
{
	
	class Preferences : IPreferences
	{

		string RootPath { get; set; }
		public IPreferencesService Service { get; set; }
		
		public Preferences (IPreferencesService service) : this (service, "")
		{
		}

		public Preferences (IPreferencesService service, string rootPath)
		{
			RootPath = rootPath;
			Service = service;
		}
		
		void OnPreferenceChanged (string key, object oldValue, object newValue)
		{
			if (PreferenceChanged == null) return;
			
			PreferenceChangedEventArgs args = new PreferenceChangedEventArgs (key, oldValue, newValue);
			PreferenceChanged (this, args);
		}

		#region IPreferences
		
		public event EventHandler<PreferenceChangedEventArgs> PreferenceChanged;

		public string AbsolutePathForKey (string key)
		{
			if (key.StartsWith ("/"))
				return Service.AbsolutePathForKey (key);
			return Service.AbsolutePathForKey (string.Format ("{0}/{1}", RootPath, key));
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
			bool success;

			success = TryGet (key, out val);
			if (!success) {
				success = Set (key, def);
				val = def;
			}
			return success;
		}

		/// <summary>
		/// Sets a gconf key to a given value.
		/// </summary>
		/// <param name="key">
		/// A <see cref="System.String"/> key (e.g. "key_binding") stored
		/// under Do's root gconf path.
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
			T oldValue;
			string keypath = AbsolutePathForKey (key);
			
			if (!Service.TryGet (keypath, out oldValue))
				oldValue = default (T);
			
			if (Service.Set (keypath, val)) {
				OnPreferenceChanged (key, oldValue, val);
				return true;
			}
			return false;
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
			string keypath = AbsolutePathForKey (key);
			return Service.TryGet (keypath, out val);
		}

		#endregion
		
	}

}