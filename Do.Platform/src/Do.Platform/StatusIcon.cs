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
		public abstract class Implementation
		{			
			protected string RootKey = "StatusIcon";
			protected string VisibleKey = "StatusIconVisible";
			protected bool   VisibleDefault = true;			
			
			public abstract bool VisibilityPreference { get; set; }
			public abstract void Show ();
			public abstract void Hide ();
			public abstract void Notify ();
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
		/// Whether or not the icon is visible
		/// </summary>
		public static bool VisibilityPreference {
			get { return Imp.VisibilityPreference; }
			set { Imp.VisibilityPreference = value; }
		}

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
