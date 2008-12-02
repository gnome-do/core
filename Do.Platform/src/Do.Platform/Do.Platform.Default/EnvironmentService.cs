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
using System.Collections.Generic;

using Mono.Unix;

using Do.Platform.ServiceStack;

namespace Do.Platform.Default
{
	
	class EnvironmentService : IEnvironmentService
	{
		#region IEnvironmentService

		#region IObject
		
		public string Name {
			get { return Catalog.GetString ("Default Environment Service"); }
		}

		public string Description {
			get { return Catalog.GetString ("Just prints warnings and returns default values."); }
		}

		public string Icon {
			get { return "gnome-do"; }
		}

		#endregion

		public void OpenEmail (IEnumerable<string> to, IEnumerable<string> cc, IEnumerable<string> bcc,
			string subject, string body, IEnumerable<string> attachments)
		{
			Log.Warn ("Default IEnvironmentService cannot send email.");
		}
		
		public void OpenURL (string url)
		{
			Log.Warn ("Default IEnvironmentService cannot open url \"{0}\".", url);
		}
		
		public void OpenPath (string path)
		{
			Log.Warn ("Default IEnvironmentService cannot open path \"{0}\".", path);
		}
			
		public bool IsExecutable (string line)
		{
			Log.Warn ("Default IEnvironmentService cannot determine if \"{0}\" is executable.", line);
			return false;
		}
		
		public void Execute (string line)
		{
			Log.Warn ("Default IEnvironmentService cannot execute \"{0}\".", line);
		}

		#endregion
	}
}
