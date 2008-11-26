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

namespace Do.Platform {
	
	/// <summary>
	/// A class providing generic functionality for dealing with preferences,
	/// whether those preferences are provided by gconf, command line options,
	/// etc.
	/// </summary>
	public class Preferences
	{

		public class ChangedEventArgs : EventArgs {
			public string Key { get; private set; }
			public object Value { get; private set; }
			
			public ChangedEventArgs (string key, object value)
			{
				Key = key; Value = value;
			}
		}

		public interface Implementation
		{
			bool Set<T>    (string key, T val);
			bool TryGet<T> (string key, out T val);
		}
				
		public static Implementation Imp { get; private set; }

		public static void Initialize (Implementation imp)
		{
			if (Imp != null)
				throw new Exception ("Already has Implementation");
			if (imp == null)
				throw new ArgumentNullException ("Implementation may not be null");
			
			Imp = imp;
		}

		public static Preferences Get (string rootKey)
		{
			return new Preferences (rootKey);
		}

		static string Combine (string key1, string key2)
		{
			return string.Format ("{0}/{1}", key1, key2);
		}

		public event PreferenceChangedDelegate PreferenceChanged;
		public delegate void PreferenceChangedDelegate (object sender, ChangedEventArgs args);
		
		string RootKey { get; set; }

		protected Preferences (string rootKey)
		{
			RootKey = rootKey;
		}
		
		public string this [string key] {
			get {
				return Get<string> (key, string.Empty);
			}
			set {
				Set<string> (key, value);
			}
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

		#region Implementation
		
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
			bool success = Imp.Set<T> (Combine (RootKey, key), val);
			if (success && null != PreferenceChanged) {
				// We send the unmodifed key (without RootKey) to subscribers.
				PreferenceChanged (this, new ChangedEventArgs (key, val));
			}
			return success;
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
			return Imp.TryGet<T> (Combine (RootKey, key), out val);
		}

		#endregion
		
		/// <summary>
		/// Read a value stored for a given key from a given root. Return the default if
		/// no value exists for that key. If you only need one key from an arbitrary root,
		/// this is a good choice.
		/// </summary>
		/// <param name="rootKey">
		/// A <see cref="System.String"/> root path to find the key in
		/// </param>
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
		public static T Get<T> (string rootKey, string key, T def)
		{
			T val;
			
			Preferences prefs = new Preferences (rootKey);
			prefs.TryGet (key, def, out val);
			
			return val;
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
		public static bool Set<T> (string rootKey, string key, T val)
		{
			Preferences prefs = new Preferences (rootKey);
			bool success = Imp.Set<T> (Combine (prefs.RootKey, key), val);
			if (success && null != prefs.PreferenceChanged)
				prefs.PreferenceChanged (prefs, new ChangedEventArgs (key, val));
			
			return success;
		}

	}
}