//  IDoWindow.cs
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
using Do.Universe;
using Gdk;

namespace Do.Addins.UI
{
	public enum Pane
	{
		First = 0,
		Second = 1,
		Third = 2,
	}
	
	public interface IDoWindow
	{
		/// <value>
		/// Returns true if window is currently being displayed
		/// </value>
		bool Visible { get; }
		
		/// <value>
		/// Returns true if the results window is currently being displayed
		/// </value>
		bool ResultsWindowVisible { get; set; }
		
		/// <value>
		/// Set and Get current Pane
		/// </value>
		Pane CurrentPane { get; set; }
		
		/// <summary>
		/// Summoning of main window.  Does not imply resetting of normal state
		/// </summary>
		void Summon ();
		
		/// <summary>
		/// Vanishing of main window.  Does not imply resetting of normal state
		/// </summary>
		void Vanish ();
		
		/// <summary>
		/// Signal to UI window to clear out its panes and set a "default" state as it defines.
		/// Focusing of the first pane is implied with this method call
		/// </summary>
		void Reset ();
		
		/// <summary>
		/// Event to Controller to pass keypress information on to it.  This should be a direct
		/// pass to the controller with no modification in between.
		/// </summary>
		/// <param name="pane">
		/// A <see cref="Pane"/>
		/// </param>
		event Gtk.KeyPressEventHandler KeyPressEvent;
		
		/// <summary>
		/// Inform UI that it is now proper to focus a new pane.
		/// </summary>
		/// <param name="pane">
		/// A <see cref="Pane"/>
		/// </param>
		void FocusPane (Pane pane);
		
		/// <summary>
		/// Informs the UI of the need to display multiple objects in a result window.  Updating
		/// the main window with new object displays as they are selected is the responsibility 
		/// of the UI.  Showing of the results window is implied.
		/// </summary>
		/// <param name="objects">
		/// A <see cref="IObject"/>
		/// </param>
		void DisplayObjects (IObject[] objects);
		
		/// <summary>
		/// Select next object in the results window.
		/// </summary>
		void ResultsWindowNext ();
		
		/// <summary>
		/// Select previous object in the results window.
		/// </summary>
		void ResultsWindowPrev ();
		
		/// <summary>
		/// Hide results window.  Implies destruction of current results window list.
		/// </summary>
		void HideResultWindow ();
		
		/// <summary>
		/// Inform UI that a pane that is currently hidden should be diplayed.
		/// </summary>
		/// <param name="pane">
		/// A <see cref="Pane"/>
		/// </param>
		void DisplayPane (Pane pane);
		
		/// <summary>
		/// Inform UI that a pane this is currently show should be hidden.
		/// </summary>
		/// <param name="pane">
		/// A <see cref="Pane"/>
		/// </param>
		void HidePane (Pane pane);
		
		/// <summary>
		/// Inform UI that a pane should display the passed IObject.
		/// </summary>
		/// <param name="pane">
		/// A <see cref="Pane"/>
		/// </param>
		/// <param name="item">
		/// A <see cref="IObject"/>
		/// </param>
		void DisplayInPane (Pane pane, IObject item);
		
		/// <summary>
		/// Clear out all contents of a pane.
		/// </summary>
		/// <param name="pane">
		/// A <see cref="Pane"/>
		/// </param>
		void ClearPane (Pane pane);
		
		/// <summary>
		/// Check if pane is visible
		/// </summary>
		/// <param name="pane">
		/// A <see cref="Pane"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/>
		/// </returns>
		bool PaneVisible (Pane pane);
	}
}
