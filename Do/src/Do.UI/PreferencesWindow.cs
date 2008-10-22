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
using System.Linq;
using System.Collections.Generic;

using Gtk;
using Mono.Addins.Gui;

using Do;
using Do.Addins;

namespace Do.UI
{	
	public partial class PreferencesWindow : Window
	{

		const string HelpUrl = "http://do.davebsd.com/wiki/index.php?title=Using_Do";

		public PreferencesWindow () : 
			base (WindowType.Toplevel)
		{
			Build ();

			btn_close.IsFocus = true;
			foreach (IConfigurable p in Pages) {
				notebook.AppendPage (p.GetConfiguration (), new Label (p.Name));
			}
			notebook.CurrentPage = Pages.FindIndex (p => p.Name == "Plugins");
		}

		IConfigurable[] pages;
		IConfigurable[] Pages {
			get {
				return pages ?? pages = new IConfigurable[] {
					new GeneralPreferencesWidget (),
					new KeybindingsPreferencesWidget (),
					new ManagePluginsPreferencesWidget (),
					new ColorConfigurationWidget (),
				};
			}
		}

		protected virtual void OnBtnCloseClicked (object sender, EventArgs e)
		{
			Destroy ();
		}

		protected virtual void OnBtnHelpClicked (object sender, EventArgs e)
		{
			Util.Environment.Open (HelpUrl);
		}
	}
}
