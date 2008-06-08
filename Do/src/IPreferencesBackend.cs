// IPreferencesBackend.cs created with MonoDevelop
// User: dave at 4:17 PM 6/7/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;

namespace Do
{	
	public interface IPreferencesBackend
	{
		bool Set<T>    (string key, T val);
		bool TryGet<T> (string key, out T val);
	}
}
