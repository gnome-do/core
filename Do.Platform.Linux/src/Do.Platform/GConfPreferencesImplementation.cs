/* GConfPreferencesImplementation.cs
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
	public class GConfPreferencesImplementation : Preferences.Implementation
	{
		const string GConfRootPath = "/apps/gnome-do/preferences";

		GConf.Client client;

		public GConfPreferencesImplementation ()
		{
			client = new GConf.Client ();
		}

		/// <summary>
		/// If key contains an absolute path, return it; otherwise, return
		/// an absolute path for the key by appending it to Do's root gconf path.
		/// </summary>
		/// <param name="key">
		/// A <see cref="System.String"/> gconf key, containing either an
		/// absolute path or a key relative to Do's root path (e.g "key_binding"
		/// or "ui/color").
		/// </param>
		/// <returns>
		/// A <see cref="System.String"/> containing an absolute gconf path.
		/// </returns>
		private string MakeKeyPath (string key)
		{
			if (key.StartsWith ("/")) return key;
			return string.Format ("{0}/{1}", GConfRootPath, key);
		}
		
		public bool Set<T> (string key, T val)
		{
			bool success;

			success = true;
			try {
				client.Set (MakeKeyPath (key), val);
			} catch {
				success = false;
			}
			return success;
		}

		public bool TryGet<T> (string key, out T val)
		{
			bool success;

			success = true;
			try {
				val = (T) client.Get (MakeKeyPath (key));
			} catch {
				success = false;
			}
			return success;
		}
	}
}
