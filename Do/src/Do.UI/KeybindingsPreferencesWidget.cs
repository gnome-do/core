/* KeybindingsPreferencesWidget.cs
 *
 * GNOME Do is the legal property of its developers. Please refer to the
 * COPYRIGHT file distributed with this source distribution.
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
using System.Collections.Generic;

using Gtk;

using Do;

namespace Do.UI
{
	public partial class KeybindingsPreferencesWidget : Bin, Addins.IConfigurable
	{			
		new public string Name {
			get { return "Keyboard"; }
		}
		
        public string Description {
        	get { return ""; }
        }
        
        public string Icon {
        	get { return ""; }
        }
		
		public KeybindingsPreferencesWidget ()
		{
			Build ();
			
			// Initialize combo_summon
			if (!SummonKeyBindings.Contains (Do.Preferences.SummonKeyBinding)) {
				SummonKeyBindings.Insert (0, Do.Preferences.SummonKeyBinding);
			}
			foreach (string combo in SummonKeyBindings) {
				combo_summon.AppendText (combo);
			}
			combo_summon.Active = SummonKeyBindings.IndexOf (Do.Preferences.SummonKeyBinding);
		}
		
		public Bin GetConfiguration ()
        {
        	return this;
        }

		protected virtual void OnComboSummonChanged (object sender, System.EventArgs e)
		{
			Do.Preferences.SummonKeyBinding = (sender as ComboBox).ActiveText;
		}
		
		List<string> summonKeyBindings;		
		List<string> SummonKeyBindings {
			get {
				if (null == summonKeyBindings) {
					summonKeyBindings = new List<string> (
					    new string [] {
						    "<Super>space",
						    "<Ctrl>space",
					    }
					);
				}
				return summonKeyBindings;
			}
		}
		
	}
}
