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

namespace Do.Interface
{
	public enum Pane
	{
		None = -1,
		First = 0,
		Second = 1,
		Third = 2,
	}
	
	public interface IDoWindow : IDisposable
	{
		/// <value>
		/// Returns true if window is currently being displayed
		/// </value>
		bool Visible { get; }
		
		/// <value>
		/// The name of the interface
		/// </value>
		string Name { get; }
		
		/// <value>
		/// Set and Get current Pane
		/// </value>
		Pane CurrentPane { get; set; }
		
		/// <summary>
		/// Initializes the interface
		/// </summary>
		/// <param name="controller">
		/// A <see cref="IDoController"/>
		/// </param>
		void Initialize (IDoController controller);
		
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
		event DoEventKeyDelegate KeyPressEvent;
		
		/// <summary>
		/// Inform UI that the third display area needs to be displayed
		/// </summary>
		void Grow ();
		
		/// <summary>
		/// Inform UI that the third display area needs to be hidden
		/// </summary>
		void Shrink ();
		
		/// <summary>
		/// Inform UI that it is appropriate to show more results than are currently shown
		/// </summary>
		void GrowResults ();
		
		/// <summary>
		/// Inform UI that it is appropriate to show fewer results than are currently shown
		/// </summary>
		void ShrinkResults ();
		
		/// <summary>
		/// All inclusive pane set method.  This should update the search results as well as the current
		/// objects and its highlight.
		/// </summary>
		/// <param name="pane">
		/// A <see cref="Pane"/>
		/// </param>
		/// <param name="context">
		/// A <see cref="SearchContext"/>
		/// </param>
		void SetPaneContext (Pane pane, IUIContext context);
		
		/// <summary>
		/// Clear out the context of a pane.
		/// </summary>
		/// <param name="pane">
		/// A <see cref="Pane"/>
		/// </param>
		void ClearPane (Pane pane);
		
		bool ResultsCanHide { get; }
	}
}
