// IEnvironmentService.cs created with MonoDevelop
// User: david at 5:43 PM 12/1/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;

namespace Do.Platform.ServiceStack
{
	
	
	public interface IEnvironmentService : IService
	{
		void OpenURL (string url);
		void OpenPath (string path);
			
		bool IsExecutable (string line);
		void Execute (string line);
	}
}
