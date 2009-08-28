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

using Mono.Addins;

using Do.Platform;

using Docky.Core;
using Docky.Interface;

namespace Docky.Core.Default
{
	
	
	public class DockletService : IDockletService
	{
		const string ExtensionPath = "/Docky/Docklet";
		Dictionary<AbstractDockletItem, bool> docklets;
		
		#region IDockletService implementation 
		
		public event EventHandler AppletVisibilityChanged;
		
		public bool ToggleDocklet (AbstractDockletItem docklet)
		{
			if (!docklets.ContainsKey (docklet))
				return false;
			
			if (docklets [docklet])
				docklet.Disable ();
			else
				docklet.Enable ();
			
			docklets [docklet] = !docklets [docklet];
			SaveConfiguration ();
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
				if (docklets == null)
					yield break;
				foreach (AbstractDockletItem adi in docklets
				         .Where (kvp => kvp.Value)
				         .Select (kvp => kvp.Key)
				         .OrderBy (adi => adi.Name))
					yield return adi;
			}
		}
		
		#endregion 

		IPreferences prefs;
		
		public static IEnumerable<AbstractDockletItem> MADocklets {
			get { return AddinManager.GetExtensionObjects (ExtensionPath).OfType<AbstractDockletItem> (); }
		}
		
		public DockletService()
		{
			prefs = Services.Preferences.Get<DockletService> ();
			prefs.PreferencesChanged += HandlePreferencesChanged;
			
			AddinManager.AddExtensionNodeHandler (ExtensionPath, HandleDockletsChanged);
			
			BuildDocklets ();
		}
		
		void HandlePreferencesChanged (object sender, PreferencesChangedEventArgs args)
		{
			Console.WriteLine("key = " + args.Key);
			Console.WriteLine("val = " + args.Value);
			if (args.Key != "ActiveApplets")
				return;
			BuildDocklets ();
			OnAppletVisibilityChanged ();
		}
		
		void HandleDockletsChanged (object sender, ExtensionNodeEventArgs args)
		{
			BuildDocklets ();
			SaveConfiguration ();
			OnAppletVisibilityChanged ();
		}
		
		void BuildDocklets ()
		{
			// ToArray this due to lazy evaluation
			IEnumerable<AbstractDockletItem> previous = ActiveDocklets.ToArray ();
			
			IEnumerable<string> visible = VisibleApplets ();
			docklets = new Dictionary<AbstractDockletItem, bool> ();
			
			foreach (AbstractDockletItem adi in MADocklets) {
				docklets.Add (adi, visible.Contains (adi.GetType ().Name));
			}
			
			foreach (AbstractDockletItem adi in ActiveDocklets.Where (d => !previous.Contains (d))) {
				adi.Enable ();
			}
		}
		
		IEnumerable<string> VisibleApplets ()
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
			if (s != prefs.Get ("ActiveApplets", "ClockDockItem;"))
				prefs.Set ("ActiveApplets", s);
		}
		
		void OnAppletVisibilityChanged ()
		{
			if (AppletVisibilityChanged != null)
				AppletVisibilityChanged (this, EventArgs.Empty);
		}

		#region IDisposable implementation 
		
		public void Dispose ()
		{
			AddinManager.RemoveExtensionNodeHandler (ExtensionPath, HandleDockletsChanged);
		}
		
		#endregion 
		
	}
}
