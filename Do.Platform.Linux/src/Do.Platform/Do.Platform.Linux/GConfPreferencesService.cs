// GConfPreferencesService.cs
//
// GNOME Do is the legal property of its developers. Please refer to the
// COPYRIGHT file distributed with this source distribution.
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using System;

using Do.Platform;

namespace Do.Platform.Linux
{	
	public class GConfPreferencesService : IPreferencesService
	{
		const string ApplicationRootPath = "/apps/gnome-do/preferences";

		GConf.Client client;
		string RootPath { get; set; }

		public GConfPreferencesService () : this (ApplicationRootPath)
		{
		}

		public GConfPreferencesService (string rootPath)
		{
			RootPath = rootPath;
			client = new GConf.Client ();
			client.AddNotify (RootPath, new GConf.NotifyEventHandler (HandleGConfChanged));
		}
		
		void HandleGConfChanged (object sender, GConf.NotifyEventArgs args)
		{
			if (PreferencesChanged != null)
				PreferencesChanged (this, new PreferencesChangedEventArgs (args.Key.Substring(RootPath.Length + 1), args.Value));
		}

		string AbsolutePathForKey (string key)
		{
			if (key.StartsWith ("/"))
				return key;
			return string.Format ("{0}/{1}", RootPath, key);
		}
		
		#region IPreferencesService
		
		public event EventHandler<PreferencesChangedEventArgs> PreferencesChanged;
		
		public bool Set<T> (string key, T val)
		{
			bool success = true;
			try {
				client.Set (AbsolutePathForKey (key), val);
			} catch (Exception e) {
				Log.Error ("Encountered error setting GConf key {0}: {1}", key, e.Message);
				Log.Debug (e.StackTrace);
				success = false;
			}
			return success;
		}

		public bool TryGet<T> (string key, out T val)
		{
			bool success = true;
			val = default (T);
			try {
				val = (T) client.Get (AbsolutePathForKey (key));
			} catch (GConf.NoSuchKeyException) {
				// We don't need to log this, because many keys that do not
				// exist are asked for.
				success = false;
			} catch (Exception e) {
				Log.Error ("Encountered error getting GConf key {0}: {1}", key, e.Message);
				Log.Debug (e.StackTrace);
				success = false;
			}
			return success;
		}

		#endregion
	}
}
