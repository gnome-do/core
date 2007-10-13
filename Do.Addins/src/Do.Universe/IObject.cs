// IObject.cs created with MonoDevelop
// User: dave at 1:38 PM 8/29/2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;

namespace Do.Universe
{
	
	public interface IObject
	{
		string Name { get; }
		string Description { get; }
		string Icon { get; }
	}
}
