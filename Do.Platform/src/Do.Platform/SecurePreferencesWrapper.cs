/* SecurePreferencesWrapper.cs 
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
using System.ComponentModel;

namespace Do.Platform
{
	/// <summary>
	/// Wrapper class for PreferencesImplementation that allows masquerading an ISecurePreferences
	/// as an IPreferences object. Only string values are allowed, NotImplementedExceptions are thrown when
	/// other types are passed.
	/// </summary>
	public class SecurePreferencesServiceWrapper : IPreferencesService
	{
		ISecurePreferencesService SecureService { get; set; }
		
		public SecurePreferencesServiceWrapper (ISecurePreferencesService secureService)
		{
			SecureService = secureService;
		}
		
		public event EventHandler<PreferencesChangedEventArgs> PreferencesChanged;

		public bool Set<T> (string key, T val)
		{
			EnsureString<T> ();
			
			return SecureService.Set (key, val.ToString ());
		}

		public bool TryGet<T> (string key, out T val)
		{
			string secureValue;
			bool success;
			
			EnsureString<T> ();

			success = SecureService.TryGet (key, out secureValue);
			val = (T) Convert.ChangeType (secureValue, typeof (T));
			
			return success;
		}

		void EnsureString<T> ()
		{
			if (typeof (T) != typeof (string)) throw new NotImplementedException ("Unimplemented for non string values");
		}
	}
}
