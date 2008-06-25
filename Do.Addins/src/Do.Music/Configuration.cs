// Configuration.cs created with MonoDevelop
// User: zgold at 20:48 06/15/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using Do.Addins;

namespace Do.Addins.DoMusic
{
	
	
	public static class Configuration
	{
		public static IMusicSource CurrentSource;
		public static bool allSources = true;
		
		public static bool AllSources { 
			get { return DoMusic.GetSources ().Count != 1 && allSources;} 
		}
		
		public static void setUseAllSources (bool use)
		{
			allSources = use;
		}
	}
}
