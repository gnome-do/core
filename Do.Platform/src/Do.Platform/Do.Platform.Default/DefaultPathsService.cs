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
using System.Collections.Generic;

using Do.Platform;
using Do.Platform.ServiceStack;

namespace Do.Platform.Default
{
	
	public class DefaultPathsService : PathsService, IInitializedService
	{
		
		const string TemporaryDirectoryName = "tmp";
		const string ApplicationDirectoryName = "gnome-do";

		public void Initialize ()
		{
			DeleteDirectory (TemporaryDirectory);

			CreateDirectory (UserDataDirectory);
			CreateDirectory (TemporaryDirectory);
		}

		static void CreateDirectory (string path)
		{
			if (Directory.Exists (path)) return;

			try {
				Directory.CreateDirectory (path);
			} catch (Exception e) {
				Log<DefaultPathsService>.Error ("Could not create directory {0}: {1}", path, e.Message);
				Log<DefaultPathsService>.Debug (e.StackTrace);
			}
		}

		static void DeleteDirectory (string path)
		{
			if (!Directory.Exists (path)) return;

			try {
				Directory.Delete (path, true);
			} catch (Exception e) {
				Log<DefaultPathsService>.Error ("Could not delete directory {0}: {1}", path, e.Message);
				Log<DefaultPathsService>.Debug (e.StackTrace);
			}
		}
		
		public override string UserDataDirectory {
			get { return Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.LocalApplicationData), ApplicationDirectoryName); }
		}
		
		public override string TemporaryDirectory {
			get { return Path.Combine (UserDataDirectory, TemporaryDirectoryName); }
		}
		
	}
}
