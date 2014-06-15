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
using Do.Interface;
using Do.Platform;
using Do.Platform.Linux;

namespace Do.UI
{	
	public partial class PreferencesWindow : Window
	{

		const int ManagePreferencesPreferencesPageIndex = 2;
		const string HelpUrl = "https://answers.launchpad.net/do";

		readonly IEnumerable<IConfigurable> Pages = new IConfigurable [] {
			new GeneralPreferencesWidget (),
			new KeybindingsPreferencesWidget (),
			new ManagePluginsPreferencesWidget (),
			new ColorConfigurationWidget (),
		};

		public PreferencesWindow () : 
			base (WindowType.Toplevel)
		{
			Build ();
			IconName = "gnome-do";

			btn_close.IsFocus = true;
			
			TargetEntry [] targets = { new TargetEntry ("text/uri-list", 0, 0) };
			Drag.DestSet (this, DestDefaults.All, targets, Gdk.DragAction.Copy);
			
			// Add notebook pages.
			foreach (IConfigurable page in Pages) {
				notebook.AppendPage (page.GetConfiguration (), new Label (page.Name));
			}
			
			// Sets default page to the plugins tab, a good default.
			notebook.CurrentPage = ManagePreferencesPreferencesPageIndex;
		}

		protected virtual void OnBtnCloseClicked (object sender, EventArgs e)
		{
			Hide ();
		}

		protected virtual void OnBtnHelpClicked (object sender, EventArgs e)
		{
			Services.Environment.OpenUrl (HelpUrl);
		}
		
		public override void Dispose ()
		{
			foreach (Gtk.Widget w in notebook.Children)
				w.Dispose ();
			base.Dispose ();
		}
	}
}
