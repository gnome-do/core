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
using System.Threading;
using System.ComponentModel;

using Mono.Unix;

using Gnome.Keyring;

using Do.Platform;

namespace Do.Platform.Linux
{
	public class GnomeKeyringSecurePreferencesService : ISecurePreferencesService
	{
		readonly string ErrorSavingMessage = Catalog.GetString ("Error saving {0}");
		readonly string KeyNotFoundMessage = Catalog.GetString ("Key \"{0}\" not found in keyring");
		readonly string KeyringUnavailableMessage = Catalog.GetString ("gnome-keyring-daemon could not be reached!");
		
		const string DefaultRootPath = "gnome-do";

		string RootPath { get; set; }

		public GnomeKeyringSecurePreferencesService () : this (DefaultRootPath)
		{
		}

		public GnomeKeyringSecurePreferencesService (string rootPath)
		{
			RootPath = rootPath;
		}

		string AbsolutePathForKey (string key)
		{
			if (key.StartsWith ("/"))
				return key;
			return string.Format ("{0}/{1}", RootPath, key);
		}
		
		#region ISecurePreferencesService
	
		public bool Set (string key, string val)
		{
			Hashtable keyData;

			if (!Ring.Available) {
				Log.Error (KeyringUnavailableMessage);
				return false;
			}

			keyData = new Hashtable ();
			keyData[AbsolutePathForKey (key)] = key;

			bool success = false;
			AutoResetEvent completed = new AutoResetEvent (false);

			Services.Application.RunOnMainThread (delegate {
				try {
					Ring.CreateItem (Ring.GetDefaultKeyring (), ItemType.GenericSecret, AbsolutePathForKey (key), keyData, val.ToString (), true);
					success = true;
				} catch (KeyringException e) {
					Log.Error (ErrorSavingMessage, key, e.Message);
					Log.Debug (e.StackTrace);
					success = false;
				}
				completed.Set ();
			});
			if (!completed.WaitOne (200)) {
				return false;
			}
			return success;
		}

		public bool TryGet (string key, out string val)
		{
			Hashtable keyData;

			string secret = "";

			if (!Ring.Available) {
				Log.Error (KeyringUnavailableMessage);
				val = "";
				return false;
			}

			keyData = new Hashtable ();
			keyData[AbsolutePathForKey (key)] = key;
			
			bool success = false;
			AutoResetEvent completed = new AutoResetEvent (false);

			Services.Application.RunOnMainThread (delegate {
				try {
					foreach (ItemData item in Ring.Find (ItemType.GenericSecret, keyData)) {
						if (!item.Attributes.ContainsKey (AbsolutePathForKey (key)))
							continue;

						secret = item.Secret;
						success = true;
						break;
					}
				} catch (KeyringException) {
					Log.Debug (KeyNotFoundMessage, AbsolutePathForKey (key));
					success = false;
				} finally {
					completed.Set ();
				}
			});
			if (!completed.WaitOne (200)) {
				success = false;
			}
			val = secret;
			return success;
		}
		
		#endregion
	}
}
