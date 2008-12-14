// IPathsService.cs
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

using Do.Universe;
using Do.Platform.ServiceStack;

namespace Do.Platform
{
	
	public interface IPathsService : IService
	{
	}

	public static class IPathsServiceExtensions
	{

		const string TemporaryDirectoryName = "tmp";
		const string ApplicationDirectoryName = "gnome-do";
		
		static IPathsServiceExtensions ()
		{
			DeleteDirectory (GetTemporaryDirectory (null));

			CreateDirectory (GetTemporaryDirectory (null));
			CreateDirectory (GetApplicationDataDirectory (null));
			CreateDirectory (GetUserDataDirectory (null));
		}

		static void CreateDirectory (string path)
		{
			if (Directory.Exists (path)) return;

			try {
				Directory.CreateDirectory (path);
			} catch (Exception e) {
				Log.Error ("Could not create directory {0}: {1}", path, e.Message);
				Log.Debug (e.StackTrace);
			}
		}

		static void DeleteDirectory (string path)
		{
			if (!Directory.Exists (path)) return;

			try {
				Directory.Delete (path, true);
			} catch (Exception e) {
				Log.Error ("Could not delete directory {0}: {1}", path, e.Message);
				Log.Debug (e.StackTrace);
			}
		}

		public static string GetUserHomeDirectory (this IPathsService self)
		{
			return Environment.GetFolderPath (Environment.SpecialFolder.Personal);
		}

		public static string GetUserHomeDirectory (this IPathsService self, string path)
		{
			return Path.Combine (GetUserHomeDirectory (self), path);
		}

		public static string GetApplicationDataDirectory (this IPathsService self)
		{
			return Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData), ApplicationDirectoryName);
		}

		public static string GetApplicationDataDirectory (this IPathsService self, string path)
		{
			return Path.Combine (GetApplicationDataDirectory (self), path);
		}

		public static string GetUserDataDirectory (this IPathsService self)
		{
			return Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.LocalApplicationData), ApplicationDirectoryName);
		}

		public static string GetUserDataDirectory (this IPathsService self, string path)
		{
			return Path.Combine (GetUserDataDirectory (self), path);
		}

		public static string GetTemporaryDirectory (this IPathsService self)
		{
			return Path.Combine (self.GetApplicationDataDirectory (), TemporaryDirectoryName);
		}

		public static string GetTemporaryDirectory (this IPathsService self, string path)
		{
			return Path.Combine (GetTemporaryDirectory (self), path);
		}
		
		public static string GetTemporaryFilePath (this IPathsService self)
		{
			int fileId;
			string fileName;
			Random random = new Random ();

			if (!Directory.Exists (GetTemporaryDirectory (self)))
				Directory.CreateDirectory (GetTemporaryDirectory (self));

			do {
				fileId = random.Next ();
				fileName = GetTemporaryDirectory (self, fileId.ToString ());
			} while (File.Exists (fileName));
			return fileName;
		}

	
	}
}
