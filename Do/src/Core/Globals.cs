// Global.cs created with MonoDevelop
// User: dave at 1:00 PM 10/11/2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using Do;

namespace Do.Core
{
	
	
	public class Globals
	{
		
		public static void Initialize ()
		{
			InitializePluginUtil ();
			InitializeLogging ();
		}
		
		/// <summary>
		/// Initialize class PluginLib.Util.
		/// </summary>
		static void InitializePluginUtil ()
		{
			// Misc
			PluginLib.Util.FormatCommonSubstrings = Util.FormatCommonSubstrings;
			
			// System utilities
			PluginLib.Util.Desktop.Open = Util.Desktop.Open;
			
			// Appearance utilities
			PluginLib.Util.Appearance.PixbufFromIconName = Util.Appearance.PixbufFromIconName;
			PluginLib.Util.Appearance.MarkupSafeString = Util.Appearance.MarkupSafeString;
		}
		
		static void InitializeLogging ()
		{
			Log.AddLog (new ConsoleLog ());
		}
	}
}
