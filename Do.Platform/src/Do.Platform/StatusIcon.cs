/* StatusIcon.cs
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
	
	public static class StatusIcon
	{		
		public interface Implementation
		{
			void Show ();
			void Hide ();
			void Notify ();
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
		
		#region Preference key info
		
		/// <value>
		/// StatusIcon root key name.
		/// </value>
		public static string RootKey { get { return "StatusIcon"; } }
		
		/// <value>
		/// Key containing whether or not the StatusIcon should be visible
		/// </value>
		public static string VisibleKey { get { return "StatusIconVisible"; } }
		
		/// <value>
		/// Default value for StatusIcon visibility.
		/// </value>
		public static bool VisibleDefault { get { return true; } }
		
		#endregion
		
		#region Implementation
		
		/// <summary>
		/// Show the Status icon
		/// </summary>
		public static void Show ()
		{
			Imp.Show ();
		}
		
		/// <summary>
		/// hide the status icon
		/// </summary>
		public static void Hide ()
		{
			Imp.Hide ();
		}
		
		/// <summary>
		/// Perform the notification routine to alert the user that something has happened
		/// </summary>
		public static void Notify ()
		{
			Imp.Notify ();
		}
		
		#endregion
	}
}
