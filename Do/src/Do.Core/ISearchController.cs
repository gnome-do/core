// ISearchController.cs
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

using Do.Interface;
using Do.Universe;
using Do.UI;

namespace Do.Core
{
	
	
	public interface ISearchController
	{
		/// <value>
		/// Returns a context useful for passing to a UI for display.
		/// </value>
		IUIContext UIContext {get;}
		
		/// <value>
		/// Item Results from the current Query
		/// </value>
		IList<Item> Results {get; set;}
		
		/// <value>
		/// The full selection, including secondary selections
		/// </value>
		IList<Item> FullSelection {get;}
		
		/// <value>
		/// The primary selection from the user curosr
		/// </value>
		Item Selection {get;}
		
		/// <value>
		/// The location of the cursor in the results list
		/// </value>
		int Cursor {get; set;}
		
		/// <value>
		/// Places marked by the user as additional cursors
		/// </value>
		int[] SecondaryCursors {get;}
		
		/// <value>
		/// Get or Set text mode on a search context
		/// </value>
		bool TextMode {get; set;}
		
		/// <summary>
		/// Finalize explicit text mode entry
		/// </summary>
		void FinalizeTextMode ();

		/// <value>
		/// The reason for which text mode was entered
		/// </value>
		TextModeType TextType {get;}
		
		/// <value>
		/// Determines if the default filter is applied to the context or not
		/// </value>
		bool DefaultFilter {get;}
		
		/// <value>
		/// The search types used to filter results
		/// </value>
		IEnumerable<Type> SearchTypes {get;}
		
		/// <value>
		/// Returns the current Query
		/// </value>
		string Query {get;}
		
		/// <summary>
		/// Add a single character to the Search Context
		/// </summary>
		/// <param name="character">
		/// A <see cref="System.Char"/>
		/// </param>
		void AddChar (char character);
		
		/// <summary>
		/// Delete the last character from the query
		/// </summary>
		void DeleteChar ();
		
		/// <summary>
		/// Add a secondary cursor.  Returns true if successful.
		/// </summary>
		/// <param name="cursorLocation">
		/// A <see cref="System.Int32"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/>
		/// </returns>
		bool ToggleSecondaryCursor (int cursorLocation);
		
		/// <summary>
		/// Return true if item child search was possible
		/// </summary>
		/// <returns>
		/// A <see cref="System.Boolean"/>
		/// </returns>
		bool ItemChildSearch ();

		/// <summary>
		/// Return true if item parent search was possible
		/// </summary>
		/// <returns>
		/// A <see cref="System.Boolean"/>
		/// </returns>
		bool ItemParentSearch ();
		
		/// <summary>
		/// Resets the controllers context but leaves UpstreamContext links in place
		/// </summary>
		void Reset ();
		
		/// <summary>
		/// The controller has started a search due to upstream results changing
		/// </summary>
		event SearchStartedEventHandler SearchStarted;
		
		/// <summary>
		/// The controllers search has finished.
		/// </summary>
		event SearchFinishedEventHandler SearchFinished;

		/// <summary>
		/// In the unlikely event we wish to set an entire string at once without having to do the
		/// whole thing incrementally.  Useful for things like paste.
		/// </summary>
		/// <param name="s">
		/// A <see cref="System.String"/> to be added to the controllers query
		/// </param>
		void SetString (string s);
	}
}
