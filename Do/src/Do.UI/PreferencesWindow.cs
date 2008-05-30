/* PreferencesWindow.cs
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
using Mono.Addins.Gui;

namespace Do.UI
{	
	public partial class PreferencesWindow : Window
	{
		public PreferencesWindow () : 
			base (WindowType.Toplevel)
		{
			Build ();

			btn_close.IsFocus = true;
			// Add notebook pages.
			foreach (IPreferencePage page in Pages) {
				notebook.AppendPage (page.Page, new Label (page.Label));
			}
		}

		IPreferencePage[] pages;
		IPreferencePage[] Pages {
			get {
				if (null == pages) {
					pages = new IPreferencePage[] {
						new GeneralPreferencesWidget (),
						new KeybindingsPreferencesWidget (),
						new ManagePluginsPreferencesWidget (),
					};
				}
				return pages;
			}
		}

		protected virtual void OnBtnCloseClicked (object sender, EventArgs e)
		{
			Destroy ();
		}

		protected virtual void OnBtnHelpClicked (object sender, EventArgs e)
		{
			Util.Environment.Open ("https://wiki.ubuntu.com/GnomeDo/Use");
		}
	}
}
