// DefaultEnvironmentService.cs created with MonoDevelop
// User: david at 6:02 PM 12/1/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;

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
