// ICommander.cs created with MonoDevelop
// User: dave at 12:00 PM 8/25/2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using NDesk.DBus;

namespace Do.DBusLib
{
	[Interface ("org.gnome.Do.Commander")]
	public interface ICommander
	{
		void Show ();
	}

}