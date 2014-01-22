//
//  Cario32Compatibility.cs
//
//  Author:
//       Christohper James Halse Rogers <raof@ubuntu.com>
//
//  Copyright (c) 2014 Christopher James Halse Rogers <raof@ubuntu.com>
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;

using Cairo;

namespace Do.Interface.CairoUtils
{
	public static class Cairo32Compatibility
	{
		public static Surface GetTarget(this Cairo.Context cr)
		{
			return cr.Target;
		}
		public static void SetSource(this Cairo.Context cr, Pattern sourcePattern)
		{
			cr.Source = sourcePattern;
		}
	}
}

