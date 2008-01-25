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
using Do.Universe;

namespace Do
{
	public class Preferences
	{
		const string GConfRootPath = "/apps/gnome-do/preferences/";

		GConf.Client client;

		public Preferences ()
		{
			client = new GConf.Client();
		}

		public string SummonKeyBinding
		{
			get {
				return Get<string> ("key_binding", "<Super>space");
			}
			set {
				Set<string> ("key_binding", value);
			}
		}

		public bool UpdatingEnabled
		{
			get {
				return Get<bool> ("enable_updating", false);
			}
			set {
				Set<bool> ("enable_updating", value);
			}
		}

		private string MakeKeyPath (string key)
		{
			if (key.StartsWith ("/"))
				return key;
			else
				return GConfRootPath + key;
		}


		public T Get<T> (string key, T def)
		{
			T val;

			TryGet (key, def, out val);
			return val;
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
	}

}
