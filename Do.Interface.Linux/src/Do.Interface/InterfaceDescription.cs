// InterfaceDescription.cs
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

using Mono.Addins;

namespace Do.Interface
{
	
	
	public class InterfaceDescription
	{
		TypeExtensionNode node;
		
		public InterfaceDescription (TypeExtensionNode node)
		{
			if (node == null) throw new ArgumentNullException ("node");
			
			this.node = node;
		}

		public string Name {
			get {
				return Addin.Name;
			}
		}

		Addin Addin {
			get {
				return AddinManager.Registry.GetAddin (node.Addin.Id);
			}
		}

		public IDoWindow GetNewInstance ()
		{
			return node.CreateInstance () as IDoWindow;
		}
	}
}
