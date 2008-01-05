/* Controller.cs
 *
 * GNOME Do is the legal property of its developers. Please refer to the
 * COPYRIGHT file distributed with this
 * source distribution.
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections;
using System.Collections.Generic;

using Do.DBusLib;
using Do.Universe;
using Do.UI;

namespace Do.Core
{
	
	public class Controller : IController
	{
		
		protected SymbolWindow window;
		
		public Controller ()
		{		
			window = new SymbolWindow ();
		}

		bool Summonable {
			get {
				return MainMenu.Instance.AboutDialog == null;
			}
		}
		
		public void SummonWithObjects (IObject[] objects)
		{
			if (!Summonable) return;
			window.DisplayObjects (objects);
			Summon ();
		}

		///////////////////////////
		/// IController Members ///
		///////////////////////////
		
		public void Summon ()
		{
			if (!Summonable) return;
			window.Show ();
			Util.Appearance.PresentWindow (window);
		}
		
		public void Vanish ()
		{
			window.Vanish ();
		}	
	}
}
