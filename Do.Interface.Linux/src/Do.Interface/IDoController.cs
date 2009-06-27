//  IDoController.cs
//
//  GNOME Do is the legal property of its developers, whose names are too numerous
//  to list here.  Please refer to the COPYRIGHT file distributed with this
//  source distribution.
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;

using Do.Interface;
using Do.Universe;

namespace Do.Interface
{
	public enum ControlOrientation {
		Vertical,
		Horizontal,
	}
	
	public interface IDoController
	{
		/// <summary>
		/// Allows the UI to alert the controller that a mouse event has selected a new object in a context
		/// </summary>
		/// <param name="pane">
		/// A <see cref="Pane"/>
		/// </param>
		/// <param name="index">
		/// A <see cref="System.Int32"/>
		/// </param>
		void NewContextSelection (Pane pane, int index);
		
		/// <summary>
		/// Allows the UI to alert the controller that a mouse click has occured off of its main window
		/// </summary>
		void ButtonPressOffWindow ();
		
		/// <value>
		/// Allow the UI to set the Controllers idea of the CurrentPane.  Useful for allowing current
		/// pane setting through mouse events.
		/// </value>
		Pane CurrentPane { set; }
		
		/// <value>
		/// Set the control layout orientation
		/// </value>
		ControlOrientation Orientation { get; set; }
		
		/// <summary>
		/// Check and see if an object likely has children
		/// </summary>
		/// <param name="item">
		/// A <see cref="Item"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/>
		/// </returns>
		bool ItemHasChildren (Item item);
	}
}
