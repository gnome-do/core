/* GnomeKeyringSecurePreferencesService.cs 
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
using System.Collections;

using Mono.Unix;

using Gnome.Keyring;

using Do.Platform;

namespace Do.Platform.Linux
{
	public class GnomeKeyringSecurePreferencesService : ISecurePreferencesService
	{
		readonly string ErrorSavingMsg = Catalog.GetString ("Error saving {0}");
		readonly string KeyringUnavailableMsg = Catalog.GetString ("gnome-keyring-daemon could not be reached!");
		
		string root_path;

		public GnomeKeyringSecurePreferencesService () : this ("")
		{
		}
		
		public GnomeKeyringSecurePreferencesService (string rootKey)
		{
			root_path = rootKey;
		}
		
		#region ISecurePreferencesService

		public string AbsolutePathForKey (string key)
		{
			if (key.StartsWith (root_path))
				return key;
			return string.Format (root_path + ".{0}", key);
		}
		
		public bool Set<T> (string key, T val)
		{
			Hashtable keyData;

			if (!Ring.Available) {
				Log.Error (KeyringUnavailableMsg);
				return false;
			}
			
			keyData = new Hashtable ();
			keyData[AbsolutePathForKey (key)] = val;
			
			try {
				Ring.CreateItem (Ring.GetDefaultKeyring (), ItemType.GenericSecret, root_path, keyData, "12341234", true);
			} catch (KeyringException e) {
				Log.Error (ErrorSavingMsg, key, e.Message);
				Log.Debug (e.StackTrace);
				return false;
			}

			return true;
		}

		public bool TryGet<T> (string key, out T val)
		{
			if (!Ring.Available) {
				Log.Error (KeyringUnavailableMsg);
				return false;
			}
			
			foreach (ItemData item in Ring.Find (ItemType.GenericSecret, new Hashtable ())) {
				Console.Error.WriteLine ("Looking in {0}", item.ItemID, item.Keyring);
				if (item.Attributes.ContainsKey (AbsolutePathForKey (key))) {
					val = (T) item.Attributes[AbsolutePathForKey (key)];
					Console.Error.WriteLine ("Found {0}", item.Attributes[AbsolutePathForKey (key)].ToString ());
					
					if (val == null) break;
					
					return true;
				}
			}

			return false;
		}
		
		#endregion
	}
}
