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

namespace Do.Platform
{
	
	public class SecurePreferencesServiceWrapper : ISecurePreferencesService
	{
		ISecurePreferencesService SecureService { get; set; }
		
		public SecurePreferencesServiceWrapper (ISecurePreferencesService secureService)
		{
			SecureService = secureService;
		}

		public string AbsolutePathForKey (string key)
		{
			return SecureService.AbsolutePathForKey (key);
		}

		public bool Set<T> (string key, T val)
		{
			return SecureService.Set<T> (key, val);
		}

		public bool TryGet<T> (string key, out T val)
		{
			CheckType<T> ();

			return SecureService.TryGet<T> (key, out val);
		}

		void CheckType<T> ()
		{
			if (typeof (T) != typeof (string)) throw new NotImplementedException ("Unimplemented for non string values");
		}
	}
}
