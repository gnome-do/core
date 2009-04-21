// UniverseFactoryService.cs
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
using System.Collections.Generic;
using System.Linq;

using Do.Universe;
using Do.Universe.Linux;
using Do.Platform;

namespace Do.Platform.Linux
{
	
	public class UniverseFactoryService : IUniverseFactoryService
	{
		public IFileItem NewFileItem (string path)
		{
			return new FileItem (path);
		}

		public IApplicationItem NewApplicationItem (string path)
		{
			// We attempt to create the Application item, but return a NullApplicationItem
			// instead of null if MaybeCreateFromDesktopItem fails.
			IApplicationItem maybe = ApplicationItem.MaybeCreateFromDesktopItem (path);
			return maybe ?? new NullApplicationItem (path);
		}
		
		public IApplicationItem MaybeApplicationItemFromCommand (string cmd)
		{
			return ApplicationItem.MaybeCreateFromCmd (cmd);
		}
	}
}
