// EnvironmentService.cs
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
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

using Do.Universe;

namespace Do.Platform.Default
{
	
	class EnvironmentService : IEnvironmentService
	{
		#region IEnvironmentService

		public void OpenEmail (IEnumerable<string> to, IEnumerable<string> cc, IEnumerable<string> bcc,
			string subject, string body, IEnumerable<string> attachments)
		{
			Log.Debug ("Default IEnvironmentService cannot send email.");
		}
		
		public void OpenUrl (string url)
		{
			Log.Debug ("Default IEnvironmentService cannot open url \"{0}\".", url);
		}
		
		public void OpenPath (string path)
		{
			Log.Debug ("Default IEnvironmentService cannot open path \"{0}\".", path);
		}
			
		public bool IsExecutable (string line)
		{
			Log.Debug ("Default IEnvironmentService cannot determine if \"{0}\" is executable.", line);
			return false;
		}
		
		public void Execute (string line)
		{
			Log.Debug ("Default IEnvironmentService cannot execute \"{0}\".", line);
		}

		public Process ExecuteWithArguments (string command, IEnumerable<string> arguments)
		{
			Log.Debug ("Default IEnvironmentService cannot execute \"{0}\".",
				arguments.Aggregate (command, (current, item) => current + " " + item));
			return new Process ();
		}

		public Process ExecuteWithArguments (string command, params string[] arguments)
		{
			return ExecuteWithArguments (command, arguments);
		}

		public void CopyToClipboard (Item item)
		{
			Log.Debug ("Default IEnvironmentService cannot copy \"{0}\".", item.Name);
		}
		
		public string ExpandPath (string path)
		{
			Log.Debug ("Default IEnvironmentService cannot expand path \"{0}\".", path);
			return path;
		}
		
		#endregion
	}
}
