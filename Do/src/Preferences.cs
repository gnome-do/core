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
using System.Reflection;
using System.Collections.Generic;

using Do;
using Do.Addins;
using Do.Universe;

namespace Do {
	
	public class PreferenceChangedEventArgs : EventArgs {
		public readonly string Key;
		public readonly object Value;
		public PreferenceChangedEventArgs (string key, object value)
		{
			Key = key; Value = value;
		}
	}
	
	/// <summary>
	/// A class providing generic functionality for dealing with preferences,
	/// whether those preferences are provided by gconf, command line options,
	/// etc.
	/// </summary>
	public class Preferences : IPreferences {
	
		public delegate void PreferenceChangedDelegate (object sender, PreferenceChangedEventArgs args);
		public event PreferenceChangedDelegate PreferenceChanged;
		
		IPreferencesBackend backend;
		
		public Preferences (string name)
			: this (new GconfPreferencesBackend (name))
		{
		}
		
		public Preferences (IPreferencesBackend backend)
		{
			this.backend = backend;
		}
		
		public string this [string key] {
			get {
				return Get<string> (key, "");
			}
			set {
				Set<string> (key, value);
			}
		}
		
		/// <summary>
		/// Sets a gconf key to a given value.
		/// </summary>
		/// <param name="key">
		/// A <see cref="System.String"/> gconf key (e.g. "key_binding") stored
		/// under Do's root gconf path. You may also specify an absoulte gconf
		/// path if you want to read any other key.
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
			bool success = backend.Set<T> (key, val);
			if (success && null != PreferenceChanged) {
				PreferenceChanged (this,
					new PreferenceChangedEventArgs (key, val));
			}
			return success;
		}

		/// <summary>
		/// Read a value stored for a given key. Return the default if
		/// no value exists for that key.
		/// </summary>
		/// <param name="key">
		/// A <see cref="System.String"/> key (e.g. "key_binding").
		/// </param>
		/// <param name="def">
		/// A defaukt <see cref="T"/> value to be returned if the key is not
		/// found.
		/// </param>
		/// <returns>
		/// A <see cref="T"/> consisting of the found value, or the default.
		/// </returns>
		public T Get<T> (string key, T def)
		{
			T val;

			TryGet (key, def, out val);
			return val;
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
			return backend.TryGet<T> (key, out val);
		}
		
		/// <summary>
		/// Try to read a value for a given key.
		/// </summary>
		/// <param name="key">
		/// A <see cref="System.String"/> key (e.g. "key_binding").
		/// </param>
		/// <param name="def">
		/// A default <see cref="T"/> value to be returned if the key is not
		/// found.
		/// </param>
		/// <param name="val">
		/// A <see cref="T"/> value if the key was found.
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/> indicating whether or not the value
		/// was read successfully.
		/// </returns>
		public bool TryGet<T> (string key, T def, out T val)
		{
			bool success;

			success = TryGet<T> (key, out val);
			if (!success) {
				success = Set<T> (key, def);
				val = def;
			}
			return success;
		}
	}
}
