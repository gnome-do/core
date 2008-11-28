/* IconProviderImplementation.cs
 *
 * GNOME Do is the legal property of its developers. Please refer to the
 * COPYRIGHT file distributed with this source distribution.
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
using Gdk;

namespace Do.Platform
{
	public static class Icons
	{
		// FIXME if someone can please figure out a way to abstract this so we don't need
		// Gdk in Do.Platform that would be dandy.
		public interface Implementation
		{
			Pixbuf PixbufFromIconName (string name, int size, bool defaultIcon);
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

		public static Pixbuf PixbufFromIconName (string name, int size)
		{
			return PixbufFromIconName (name, size, true);
		}

		#region Implementation

		/// <summary>
		/// Give a Gdk.Pixbuf of the icon from a string name
		/// </summary>
		/// <param name="name">
		/// A <see cref="System.String"/> name of the icon
		/// </param>
		/// <param name="size">
		/// A <see cref="System.Int32"/> size of the pixbuf to return
		/// </param>
		/// <param name="defaultIcon">
		/// A <see cref="System.Boolean"/> to return the default icon
		/// </param>
		/// <returns>
		/// A <see cref="Pixbuf"/>
		/// </returns>
		public static Pixbuf PixbufFromIconName (string name, int size, bool defaultIcon)
		{
			return Imp.PixbufFromIconName (name, size, defaultIcon);
		}
		
		#endregion
	}
}