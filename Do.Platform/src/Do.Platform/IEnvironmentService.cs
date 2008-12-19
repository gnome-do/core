// IEnvironmentService.cs
// 
// GNOME Do is the legal property of its developers, whose names are too
// numerous to list here.  Please refer to the COPYRIGHT file distributed with
// this source distribution.
// 
// This program is free software: you can redistribute it and/or modify it under
// the terms of the GNU General Public License as published by the Free Software
// Foundation, either version 3 of the License, or (at your option) any later
// version.
// 
// This program is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
// FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more
// details.
// 
// You should have received a copy of the GNU General Public License along with
// this program.  If not, see <http://www.gnu.org/licenses/>.
//

using System;
using System.Linq;
using System.Collections.Generic; 

using Do.Platform.ServiceStack;

namespace Do.Platform
{
	
	public interface IEnvironmentService : IService
	{
		void OpenUrl (string url);
		void OpenPath (string path);

		void OpenEmail (IEnumerable<string> to, IEnumerable<string> cc, IEnumerable<string> bcc,
			string subject, string body, IEnumerable<string> attachments);
			
		bool IsExecutable (string line);
		void Execute (string line);
	}

	public static class IEnvironmentServiceExtensions
	{
		public static void OpenEmail (this IEnvironmentService self, string to)
		{
			self.OpenEmail (to, "", "");
		}

		public static void OpenEmail (this IEnvironmentService self, string to, string subject, string body)
		{
			self.OpenEmail (new [] { to }, subject, body);
		}

		public static void OpenEmail (this IEnvironmentService self, IEnumerable<string> to, string subject, string body)
		{
			self.OpenEmail (to, subject, body, Enumerable.Empty<string> ());
		}

		public static void OpenEmail (this IEnvironmentService self, IEnumerable<string> to, string subject, string body,
			IEnumerable<string> attachments)
		{
			IEnumerable<string> nostrings = Enumerable.Empty<string> ();
			self.OpenEmail (to, nostrings, nostrings, subject, body, attachments);
		}
	}
}
