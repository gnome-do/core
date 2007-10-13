// GCApplicationItem.cs created with MonoDevelop
// User: dave at 11:00 AM 8/18/2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.Collections.Generic;
using Mono.Unix;
using System.Runtime.InteropServices;

namespace Do.Universe
{

	public class ApplicationDetailMissingException: ApplicationException
	{
		public ApplicationDetailMissingException (string message) : base (message)
		{
		}
	}

	public class ApplicationItem : IRunnableItem
	{
		
		protected string desktopFile;
		protected IntPtr desktopFilePtr;
		protected string name, description, icon;
		
		public ApplicationItem (string desktopFile)
		{
			this.desktopFile = desktopFile;

			desktopFilePtr = gnome_desktop_item_new_from_file (desktopFile, 0, IntPtr.Zero);
	     	if (desktopFilePtr == IntPtr.Zero) {
					throw new ApplicationDetailMissingException("Failed to load launcher");
	     	}
			name = gnome_desktop_item_get_string(desktopFilePtr, "Name");
			description = gnome_desktop_item_get_string(desktopFilePtr, "Comment");
			icon = gnome_desktop_item_get_string(desktopFilePtr, "Icon");
			
			if (icon == null || icon == "") {
				// If there's no icon, throw an exception and disregard this object.
				throw new ApplicationDetailMissingException (name + " has no icon.");
			}
		}
		
		public string Name {
			get { return name; }
		}
		
		public string Description {
			get { return description; }
		}
		
		public string Icon {
			get { return icon; }
		}
		
		public void Run () {
			if (desktopFilePtr != IntPtr.Zero) {
				gnome_desktop_item_launch(desktopFilePtr, IntPtr.Zero, 0, IntPtr.Zero);
			}
		}
		
		
		[DllImport ("libgnome-desktop-2.so.2")]
		private static extern IntPtr gnome_desktop_item_new_from_file(string file, int flags, IntPtr error);

		[DllImport ("libgnome-desktop-2.so.2")]
		private static extern int gnome_desktop_item_launch(IntPtr item, IntPtr args, int flags, IntPtr error);
		
		[DllImport("libgnome-desktop-2.so.2")]
		private static extern string gnome_desktop_item_get_string(IntPtr item, string id);
	}
}
