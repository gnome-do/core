// IItemSource.cs created with MonoDevelop
// User: dave at 12:54 PM 8/29/2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.Collections.Generic;

namespace Do.PluginLib
{
	
	public interface IItemSource : IObject
	{
		bool UpdateItems ();
		ICollection<IItem> Items { get; }
		// ICollection<IItem> ChildrenOfItem (IItem item);
	}
}
