/* IController.cs
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
#if USE_DBUS_SHARP
using DBus;
#else
using NDesk.DBus;
#endif

namespace Do.Platform.Linux.DBus
{
	[Interface ("org.gnome.Do.Controller")]
	public interface IController
	{
		/// <summary>
		/// Causes an IController instance to show its user interface
		/// so the user can interact with it. For example, making a
		/// SymbolWindow become visible and raise to the top.
		/// </summary>
		void Summon ();
	}
}
