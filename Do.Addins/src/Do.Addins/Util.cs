// Util.cs created with MonoDevelop
// User: dave at 12:58 PM 10/11/2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;

namespace Do.Addins
{
	public delegate bool EnvironmentOpenDelegate (string item, out string error);
	public delegate void ShowMainMenuAtPositionDelegate (int x, int y);
	public delegate Gdk.Pixbuf PixbufFromIconNameDelegate (string icon_name, int size);
	public delegate string StringTransformationDelegate (string old);
	public delegate string FormatCommonSubstringsDelegate (string main, string highlight, string format);
	
	public class Util
	{
		
		public static FormatCommonSubstringsDelegate FormatCommonSubstrings;
		
		public static class Environment
		{
			public static EnvironmentOpenDelegate Open;
		}
		
		public class Appearance
		{
			public static PixbufFromIconNameDelegate PixbufFromIconName;
			public static StringTransformationDelegate MarkupSafeString;
			public static ShowMainMenuAtPositionDelegate ShowMainMenuAtPosition;
		}		
	}
}
