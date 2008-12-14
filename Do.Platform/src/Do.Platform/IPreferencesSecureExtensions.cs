/* ISecurePreferences.cs 
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

namespace Do.Platform
{
	
	public static class IPreferencesSecureExtensions
	{
		static ISecurePreferencesService service { get; set; }
		
		static IPreferencesSecureExtensions ()
		{
			 service = Services.Preferences.SecureService;
		}

		public static string SecureAbsolutePathForKey (this IPreferences self, string key)
		{
			return service.AbsolutePathForKey (key);
		}
		
		public static bool SecureSet<T> (this IPreferences self, string key, T val)
		{
			T oldValue;
			string keypath = SecureAbsolutePathForKey (self, key);
			
			if (!service.TryGet (keypath, out oldValue))
				oldValue = default (T);
			
			if (service.Set (keypath, val)) {
				//OnPreferenceChanged (key, oldValue, val);
				return true;
			}
			return false;
		}

		public static bool SecureTryGet<T> (this IPreferences self, string key, T def, out T val)
		{
			bool success;

			success = self.SecureTryGet<T> (key, out val); 	
			if (!success) {
				success = SecureSet (self, key, def);
				val = def;
			}
			return success;
		}

		public static bool SecureTryGet<T> (this IPreferences self, string key, out T val)
		{
			string keypath = SecureAbsolutePathForKey (self, key);
			return service.TryGet (keypath, out val);
		}
	}
}
