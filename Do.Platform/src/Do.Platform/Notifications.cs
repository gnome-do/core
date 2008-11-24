// Notifications.cs created with MonoDevelop
// User: alex at 7:00 PM 11/23/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;

namespace Do.Platform
{
	
	public static class Notifications
	{
		public interface Implementation
		{
			void SendNotification (string message);
			void SendNotification (string message, string title);
			void SendNotification (string message, string title, string icon);
			void ShowKillNotification (object handler);
		}
		
		public static Implementation Imp { get; private set; }
		
		public static void Initialize (Implementation imp)
		{
			if (Imp != null)
				throw new Exception ("Already has Implementation");
			if (imp == null)
				throw new ArgumentNullException ("Implementation may not be null");
			
			Imp = imp;
		}
		
		#region Implementation
		
		public static void SendNotification (string message)
		{
			Imp.SendNotification (message);
		}
		
		public static void SendNotification (string message, string title)
		{
			Imp.SendNotification (message, title);
		}
		
		public static void SendNotification (string message, string title, string icon)
		{
			Imp.SendNotification (message, title, icon);
		}
		
		internal static void ShowKillNotification (object handler)
		{
			Imp.ShowKillNotification (handler);
		}
		
		#endregion
	}
}
