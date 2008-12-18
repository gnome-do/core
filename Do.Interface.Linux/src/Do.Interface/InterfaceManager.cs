// UserInterfaces.cs
// 
// Copyright (C) 2008 GNOME Do
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
using System.Linq;

using Mono.Addins;

using Do.Platform;

namespace Do.Interface
{
	
	
	public static class InterfaceManager
	{
		public static void Initialize ()
		{
			if (!AddinManager.IsInitialized)
				throw new Exception ("Addin manager was not initialized before initializing user interfaces");
			AddinManager.AddExtensionNodeHandler ("/Do/InterfaceWindow", OnInterfaceChanged);
		}
		
		static void OnInterfaceChanged (object sender, ExtensionNodeEventArgs e)
		{
			//fixme: this is very generic but making an instance of the objects at this time wastes LOTS of memory
			switch (e.Change) {
			case ExtensionChange.Add:
				Log.Debug ("User Interface was loaded");
				break;
			case ExtensionChange.Remove:
				Log.Debug ("User Interface was removed");
				break;
			}
		}
		
		public static IEnumerable<IDoWindow> Interfaces {
			get {
				return AddinManager.GetExtensionObjects ("/Do/InterfaceWindow").Cast<IDoWindow> ();
			}
		}
	}
}
