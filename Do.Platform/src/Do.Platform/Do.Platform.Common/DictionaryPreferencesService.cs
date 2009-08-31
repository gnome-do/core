// DictionaryPreferencesService.cs
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
using System.Collections.Generic;

using Do.Platform;

namespace Do.Platform.Common
{
	
	public class DictionaryPreferencesService : IPreferencesService
	{

		IDictionary<string, object> Store { get; set; }

		public DictionaryPreferencesService ()
		{
			Store = new Dictionary<string, object> ();
		}
		
		void OnPreferencesChanged (string key, object newValue)
		{
			if (PreferencesChanged != null)
				PreferencesChanged (this, new PreferencesChangedEventArgs (key, newValue));
		}
		
		#region IPreferencesService
		
		public event EventHandler<PreferencesChangedEventArgs> PreferencesChanged;
		
		public bool Set<T> (string key, T val)
		{
			Store [key] = val;
			OnPreferencesChanged (key, val);
			return true;
		}
		
		public bool TryGet<T> (string key, out T val)
		{
			object val_object;
			bool success = Store.TryGetValue (key, out val_object);
			
			val = default (T);
			if (success)
				val = (T) val_object;
			
			return success;
		}

		#endregion
		
	}

}
