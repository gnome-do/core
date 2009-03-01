// IRightClickable.cs
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

using Docky.Interface.Menus;

namespace Docky.Interface
{
	public interface IRightClickable
	{
		/// <summary>
		/// Returns a collection of the items that are to be placed in a menu
		/// </summary>
		/// <returns>
		/// A <see cref="IEnumerable"/>
		/// </returns>
		IEnumerable<AbstractMenuArgs> GetMenuItems ();
		
		/// <summary>
		/// Lets the dock item provider know that the remove button was clicked
		/// </summary>
		event EventHandler RemoveClicked;
	}
}
