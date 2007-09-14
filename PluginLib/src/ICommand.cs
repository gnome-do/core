// ICommand.cs created with MonoDevelop
// User: dave at 12:55 PM 8/29/2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;

namespace Do.PluginLib
{
	
	public interface ICommand : IObject
	{
		Type[] SupportedTypes { get; }
		Type[] SupportedIndirectTypes { get; }
		void PerformOnItem (IItem item);
		void PerformOnItemWithIndirectItem (IItem item, IItem iitem);
		bool SupportsItem (IItem item);
	}
}
