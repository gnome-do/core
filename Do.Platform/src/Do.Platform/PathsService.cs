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

using Do.Universe;
using Do.Platform.ServiceStack;

namespace Do.Platform
{
	
	public abstract class PathsService : IService
	{
		public abstract string UserDataDirectory { get; }
		
		public abstract string TemporaryDirectory { get; }
		
		public string GetTemporaryFilePath ()
		{
			int fileId;
			string fileName;
			string directory = TemporaryDirectory;
			Random random = new Random ();

			if (!Directory.Exists (directory))
				Directory.CreateDirectory (directory);

			do {
				fileId = random.Next ();
				fileName = Path.Combine (directory, fileId.ToString ());
			} while (File.Exists (fileName));
			return fileName;
		}

	}
}
