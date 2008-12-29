// PathsService.cs
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
using System.IO;
using System.Linq;

namespace Do
{
	
	public static class PathExtensions
	{
		/// <summary>
		/// Shortcut for System.IO.Path.Combine with a varaible number of parameters.
		/// </summary>
		/// <param name="self">
		/// A <see cref="System.String"/> base path.
		/// </param>
		/// <param name="paths">
		/// A <see cref="System.String[]"/> to combine onto the base path. 
		/// </param>
		/// <returns>
		/// A <see cref="System.String"/> fully assembled path.
		/// </returns>
		public static string Combine (this string self, params string [] paths)
		{
			if (self == null) throw new ArgumentNullException ("self");
			if (paths == null) throw new ArgumentNullException ("paths");
			
			return Path.Combine (self, paths.Aggregate (Path.Combine));
		}
	}
}
