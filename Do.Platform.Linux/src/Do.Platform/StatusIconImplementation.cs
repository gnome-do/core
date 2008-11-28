/* StatusIconImplementation.cs
*
* GNOME Do is the legal property of its developers. Please refer to the
* COPYRIGHT file distributed with this
* source distribution.
*  
* This program is free software: you can redistribute it and/or modify
* it under the terms of the GNU General Public License as published by
* the Free Software Foundation, either version 3 of the License, or
* (at your option) any later version.
* 
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU General Public License for more details.
* 
* You should have received a copy of the GNU General Public License
* along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using Mono.Unix;

using Gdk;

using Do.Platform;

namespace Do.Platform.Linux
{

	public class StatusIconImplementation : StatusIcon.Implementation
	{
		const int IconSize = 24;
		const string IconName = "gnome-do";

		Preferences prefs;
		Gtk.StatusIcon status_icon;
		static readonly Pixbuf normal_icon;

		static StatusIconImplementation ()
		{
			normal_icon = Icons.PixbufFromIconName  (IconName, IconSize);
		}

		public StatusIconImplementation ()
		{
			prefs = Preferences.Get (RootKey);
			
			status_icon = new Gtk.StatusIcon (normal_icon);
			status_icon.FromPixbuf = normal_icon;
			status_icon.PopupMenu += new Gtk.PopupMenuHandler (OnTrayIconPopup);

			if (VisibilityPreference)
				Show ();
			else
				Hide ();
		}

		#region StatusIcon.Implementation
		
		public override bool VisibilityPreference {
			get { return prefs.Get<bool> (VisibleKey, VisibleDefault); }
			set { prefs.Set<bool> (VisibleKey, value); }
		}

		public override void Show ()
		{
			status_icon.Visible = true;
		}

		public override void Hide ()
		{
			if (!VisibilityPreference)
				status_icon.Visible = false;
		}

		public override void Notify ()
		{
			Show ();
		}

		#endregion

		public void GetLocationOnScreen (out Gdk.Screen screen, out int x, out int y)
		{
			Gdk.Rectangle area;
			Gtk.Orientation orien;

			status_icon.GetGeometry (out screen, out area, out orien);
			x = area.X + area.Width / 2;
			y = area.Y + area.Height - 5;
		}

		protected void OnTrayIconPopup (object o, EventArgs args) 
		{
			int x, y;
			Gdk.Screen screen;

			GetLocationOnScreen (out screen, out x, out y); 
			Windowing.ShowMainMenu (x, y);
		}
	}
}
