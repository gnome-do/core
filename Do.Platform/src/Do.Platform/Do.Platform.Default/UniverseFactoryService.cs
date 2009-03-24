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

using Do.Universe;
using Do.Platform;

namespace Do.Platform.Default
{
	
	public class UniverseFactoryService : IUniverseFactoryService
	{
		#region IUniverseFactoryService

		public IFileItem NewFileItem (string path)
		{
			Log.Debug ("Default IUniverseFactoryService cannot return a useful IFileItem.");
			return new EmptyFileItem ();
		}
		
		public IApplicationItem NewApplicationItem (string path)
		{
			Log.Debug ("Default IUniverseFactoryService cannot return a useful IApplicationItem.");
			return new EmptyApplicationItem ();
		}
		
		public IApplicationItem MaybeApplicationItemFromCommand (string cmd)
		{
			Log.Debug ("Default IUniverseFactoryService cannot return a useful IApplicationItem.");
			return null;
		}

		class EmptyFileItem : EmptyItem, IFileItem
		{
			public string Path {
				get { return ""; }
			}
			
			public string Uri {
				get { return ""; }
			}
		}

		class EmptyApplicationItem : EmptyItem, IApplicationItem
		{
			public string Exec {
				get { return ""; }
			}
			
			public void Run ()
			{
			}

			public void LaunchWithFiles (IEnumerable<IFileItem> files)
			{
			}
			
		}

		#endregion
		
	}

}
