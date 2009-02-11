//  
//  Copyright (C) 2009 GNOME Do
// 
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
// 
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
// 

using System;
using System.Collections.Generic;
using System.Linq;

using Do.Platform;

using Docky.Core;
using Docky.Interface;

namespace Docky.Core.Default
{
	
	
	public class DockletService : IDockletService
	{
		
		Dictionary<AbstractDockletItem, bool> docklets;
		
		#region IDockletService implementation 
		
		public event EventHandler AppletVisibilityChanged;
		
		public bool ActivateDocklet (AbstractDockletItem docklet)
		{
			if (!docklets.ContainsKey (docklet) || docklets [docklet])
				return false;
			
			docklets [docklet] = true;
			OnAppletVisibilityChanged ();
			return true;
		}
		
		public bool DeactiveDocklet (AbstractDockletItem docklet)
		{
			if (!docklets.ContainsKey (docklet) || !docklets [docklet])
				return false;
			
			docklets [docklet] = false;
			OnAppletVisibilityChanged ();
			return true;
		}
		
		public bool ToggleDocklet (AbstractDockletItem docklet)
		{
			if (!docklets.ContainsKey (docklet))
				return false;
			
			docklets [docklet] = !docklets [docklet];
			OnAppletVisibilityChanged ();
			return true;
		}
		
		public IEnumerable<AbstractDockletItem> Docklets {
			get {
				return docklets.Keys.OrderBy (d => d.Name);
			}
		}
		
		public IEnumerable<AbstractDockletItem> ActiveDocklets {
			get {
				return docklets.Where (kvp => kvp.Value).Select (kvp => kvp.Key);
			}
		}
		
		#endregion 

		IPreferences prefs;
		
		public DockletService()
		{
			prefs = Services.Preferences.Get<DockletService> ();
			
			string[] visible = VisibleApplets ();
			docklets = new Dictionary<AbstractDockletItem, bool> ();
			
			TrashDockItem trash = new TrashDockItem ();
			docklets.Add (trash, visible.Contains (trash.GetType ().Name));
			
			ClockDockItem clock = new ClockDockItem ();
			docklets.Add (clock, visible.Contains (clock.GetType ().Name));
		}
		
		string[] VisibleApplets ()
		{
			string configString = prefs.Get ("ActiveApplets", "ClockDockItem;");
			return configString.Split (';');
		}
		
		void SaveConfiguration ()
		{
			string s = "";
			foreach (AbstractDockletItem abi in ActiveDocklets) {
				s += abi.GetType ().Name + ";";
			}
			prefs.Set ("ActiveApplets", s);
		}
		
		void OnAppletVisibilityChanged ()
		{
			SaveConfiguration ();
			if (AppletVisibilityChanged != null)
				AppletVisibilityChanged (this, EventArgs.Empty);
		}

		#region IDisposable implementation 
		
		public void Dispose ()
		{
		}
		
		#endregion 
		
	}
}
