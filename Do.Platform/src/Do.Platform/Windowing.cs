/* Windowing.cs
 *
 * GNOME Do is the legal property of its developers. Please refer to the
 * COPYRIGHT file distributed with this
 * source distribution.
 *  
 * This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
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

namespace Do.Platform
{
	
	public static class Windowing
	{
		public interface Implementation
		{
			void ShowMainMenu (int x, int y);
		}
		
		public static Implementation Imp { get; private set; }
		
		public static void Initialize (Implementation imp)
		{
			if (Imp != null)
				throw new Exception ("Already has Implementation");
			if (imp == null)
				throw new ArgumentNullException ("Implementation may not be null");
			
			Imp = imp;
		}
		
		#region Implementation
		
		/// <summary>
		/// Shows the main menu instance to appear at the given x and y location
		/// </summary>
		/// <param name="x">
		/// A <see cref="System.Int32"/> of the x value on the screen to show the menu
		/// </param>
		/// <param name="y">
		/// A <see cref="System.Int32"/> of the y value on the screen to show the menu
		/// </param>
		public static void ShowMainMenu (int x, int y)
		{
			Imp.ShowMainMenu (x, y);
		}
		
		#endregion
	}
}
