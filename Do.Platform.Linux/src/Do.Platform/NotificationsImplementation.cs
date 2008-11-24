// NotificationsImplementation.cs created with MonoDevelop
// User: alex at 7:25 PM 11/23/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using Notifications;
using Mono.Unix;
using GLib;

namespace Do.Platform.Linux
{
	
	public class NotificationsImplementation : Notifications.Implementation
	{
		
		#region Notifications.Implementation
		
		public void SendNotification (string message)
		{
			SendNotification ("GNOME Do", message, null);
		}
		
		public void SendNotification (string message, string title)
		{
			SendNotification (title, message, null);
		}
		
		public void SendNotification (string message, string title, string icon)
		{
			ShowNotification (title, message, icon, null, null, null);
		}
		
		internal void ShowKillNotification (object handler)
		{
			ActionHandler action = (ActionHandler) handler;
			if (action == null) throw new ArgumentException ("Must pass a valid ActionHandler");
			
			string title = Catalog.GetString ("Do Error");
			string message = Catalog.GetString ("Do is still executing the last requested task, please wait for this to finish"); 
			string icon = "dialog-error";
			ShowNotification (title, message, icon, Catalog.GetString ("Stop"), Catalog.GetString ("Stop Action"), action);
		}
		
		#endregion
		
		private static void ShowNotification (string title, string message, string icon,
			string action_name, string action_label, ActionHandler action)
		{
		
			/*
			int x, y;
			Gdk.Screen screen;

			 put this back when NotifcationIcon gets moved out of core
			NotificationIcon.GetLocationOnScreen (
				out screen, out x, out y);
			*/
			
			Notification msg;
			try {
				msg = new Notification ();
			} catch (Exception e) {
				Log.Error ("Could not show notification: " + e.Message);
				return;
			}
			
			msg.Closed += new EventHandler (OnNotificationClosed); 
			msg.Summary = GLib.Markup.EscapeText (title);
			msg.Body = GLib.Markup.EscapeText (message);
			if (icon != null)
				msg.Icon = IconProvider.PixbufFromIconName (icon,
					IconSize);
					
			if (action_name != null && action_label != null && action != null)
				msg.AddAction (action_name, action_label, action);
				
			msg.Timeout = message.Length / 10 * 1000;
			if (msg.Timeout > 10000) msg.Timeout = 10000;
			if (msg.Timeout < 5000) msg.Timeout = 5000;
			
			/* because screen, x and y aren't set!
			msg.SetGeometryHints (screen, x, y);
			*/
			
			msg.Show ();
		}
		
		// we might want to remove all of this icon showing code anyway, if the user says not to use
		// the tray icon, then we shouldn't show it anyway. except for updates.
		private static void OnNotificationClosed (object sender, EventArgs args)
		{
			/*
			if (!CorePreferences.StatusIconVisible)
				Do.NotificationIcon.Hide ();
			*/
		}
	}
}
