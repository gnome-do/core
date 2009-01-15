// InterfaceManager.cs
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
using System.Linq;
using System.Collections.Generic;

using Mono.Addins;

using Do.Platform;

namespace Do.Interface
{
	
	public class InterfaceManager
	{

		public static void Initialize ()
		{
			if (!AddinManager.IsInitialized)
				throw new Exception ("Addin manager was not initialized before initializing user interfaces");
			AddinManager.AddExtensionNodeHandler ("/Do/InterfaceWindow", OnInterfaceChanged);
		}
		
		static void OnInterfaceChanged (object sender, ExtensionNodeEventArgs e)
		{
			TypeExtensionNode node = e.ExtensionNode as TypeExtensionNode;
			InterfaceDescription desc = new InterfaceDescription (node);
			switch (e.Change) {
			case ExtensionChange.Add:
				Log<InterfaceManager>.Debug ("\"{0}\" interface was loaded", desc.Name);
				break;
			case ExtensionChange.Remove:
				Log<InterfaceManager>.Debug ("\"{0}\" interface was unloaded", desc.Name);
				break;
			}
		}
		
		public static IEnumerable<InterfaceDescription> GetInterfaceDescriptions ()
		{
			return AddinManager.GetExtensionNodes ("/Do/InterfaceWindow")
				.Cast<TypeExtensionNode> ()
				.Select (node => new InterfaceDescription (node));
		}

		public static IDoWindow MaybeGetInterfaceNamed (string name)
		{
			return GetInterfaceDescriptions ()
				.Where (d => d.Name == name)
				.Select (d => d.GetNewInstance ())
				.FirstOrDefault ();
		}
	}
}
