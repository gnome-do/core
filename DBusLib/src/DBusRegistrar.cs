/* DBusRegistrar.cs
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

using NDesk.DBus;
using org.freedesktop.DBus;

namespace Do.DBusLib
{

	/// <summary>
	/// DBusRegistrar is used for getting DBus-ready classes on and off the
	/// session bus.
	/// </summary>
	public class DBusRegistrar
	{
		
		public static readonly string BusName = "org.gnome.Do";
		
		public static readonly string BaseItemPath = "/org/gnome/Do";
		public static readonly string CommanderItemPath = BaseItemPath + "/Commander";
		
		static DBusRegistrar ()
		{
			BusG.Init ();
		}
		
		
		/// <summary>
		/// Get an instace of T if it exists on the session bus.
		/// Returns null if there is no such instance.
		/// </summary>
		/// <param name="objectPath">
		/// A <see cref="System.String"/> containing the bus path of the instance.
		/// </param>
		/// <returns>
		/// A <see cref="T"/> instance if it was found on the bus; null otherwise.
		/// </returns>
		public static T GetInstance<T> (string objectPath) {
			if (!Bus.Session.NameHasOwner (BusName)) {
				return default (T);
			}
			return Bus.Session.GetObject<T> (BusName, new ObjectPath (objectPath));
		}

		/// <summary>
		/// Register an instance of T on the session bus. Returns null if another
		/// instance at the same path exists on the bus, or if registration fails
		/// in another way.
		/// </summary>
		/// <param name="busItem">
		/// A <see cref="T"/> instance to register on the bus.
		/// </param>
		/// <param name="objectPath">
		/// A <see cref="System.String"/> containing the bus path being requested for
		/// the object.
		/// </param>
		/// <returns>
		/// A <see cref="T"/> instance registered on the bus if successful; null otherwise.
		/// </returns>
		public static T Register<T> (T busItem, string objectPath) {
			try {
				Bus.Session.RequestName (BusName);
				Bus.Session.Register (BusName, new ObjectPath (objectPath), busItem);
			} catch {
				return default (T);
			}
			return busItem;
		}
		
		/// <summary>
		/// Get an ICommander instance registered on the session bus.
		/// </summary>
		/// <returns>
		/// A <see cref="ICommander"/> instance if successful; null otherwise.
		/// </returns>
		public static ICommander GetCommanderInstance ()
		{
			return GetInstance<ICommander> (CommanderItemPath);
		}
		
		/// <summary>
		/// Register an ICommander instance on the session bus.
		/// </summary>
		/// <param name="commander">
		/// A <see cref="ICommander"/> instance to register.
		/// </param>
		/// <returns>
		/// A <see cref="ICommander"/> instance if registered successfully; null otherwise.
		/// </returns>
		public static ICommander RegisterCommander (ICommander commander)
		{
			return Register<ICommander> (commander, CommanderItemPath);
		}
		
	}
}
