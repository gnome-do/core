/* CommandLinePreferencesBackend.cs
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

namespace Do
{
	public class CommandLinePreferencesBackend : IPreferencesBackend
	{
		string [] options;
		
		public CommandLinePreferencesBackend (string [] args)
		{
			options = args;
		}
		
		/// <summary>
		/// We will never be setting a CLI opt from inside the program.
		/// </summary>
		/// <param name="key">
		/// A <see cref="System.String"/>
		/// </param>
		/// <param name="val">
		/// A <see cref="T"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/>
		/// </returns>
		public bool Set<T> (string key, T val)
		{
			return false;
		}
		
		/// <summary>
		/// Returns whether or not the requested CLI option was passed to Do
		/// </summary>
		/// <param name="key">
		/// A <see cref="System.String"/> a command line option
		/// </param>
		/// <param name="val">
		/// A <see cref="T"/> of whether or not said option was passed
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/>
		/// </returns>
		public bool TryGet<T> (string key, out T val)
		{
			if (HasOption (key))
				val = (T)((object)key);
			else
				val = (T)((object)string.Empty);
			return true;
		}
		
		private bool HasOption (string option)
		{
			return Array.IndexOf (options, option) != -1;
		}
	}
}
