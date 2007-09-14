// CommandInterface.cs created with MonoDevelop
// User: dave at 4:04 PM 8/22/2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;

using NDesk.DBus;
using org.freedesktop.DBus;

namespace Do.DBusLib
{

	public class DBusRegistrar
	{
		
		public static readonly string BusName = "org.gnome.Do";
		
		public static readonly string BaseItemPath = "/org/gnome/Do";
		public static readonly string CommanderItemPath = BaseItemPath + "/Commander";
		
		static DBusRegistrar ()
		{
			BusG.Init ();
		}
		
		public static T GetInstance<T> (string objectPath) {
			if (!Bus.Session.NameHasOwner (BusName)) {
				return default (T);
			}
			return Bus.Session.GetObject<T> (BusName, new ObjectPath (objectPath));
		}

		public static T Register<T> (T busItem, string objectPath) {
			RequestNameReply reply;

			try {
				reply = Bus.Session.RequestName (BusName);
				Bus.Session.Register (BusName, new ObjectPath (objectPath), busItem);
			} catch {
				return default (T);
			}
			return busItem;
		}
		
		public static ICommander GetCommanderInstance ()
		{
			return GetInstance<ICommander> (CommanderItemPath);
		}
		
		public static ICommander RegisterCommander (ICommander commander)
		{
			return Register<ICommander> (commander, CommanderItemPath);
		}
		
	}
}
