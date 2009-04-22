/* PluginErrorDialog.cs
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

namespace Do.UI
{
	public partial class PluginErrorDialog : Gtk.Dialog
	{
		
		public PluginErrorDialog(string[] files)
		{
			string errorMessage = Catalog.GetString ("<b><span size=\"large\">There was an error installing the selected") +  "{0}</span></b>";
			
			this.Build();
			
			header_lbl.Markup = Catalog.GetPluralString (string.Format (errorMessage, Catalog.GetString ("plugin")),
				string.Format (errorMessage, Catalog.GetString ("plugins")), files.Length);
				
			string errors = "";
			for (int i = 0; i < files.Length; i++) {
				errors += string.Format ("{0}", files[i]);
				if (i != (files.Length - 1)) errors += ", ";
				if (i == files.Length - 2) errors += Catalog.GetString ("and ");
			}
			
			if (files.Length == 1)
				errors += Catalog.GetString (" is not a valid plugin file.");
			else
				errors += Catalog.GetString (" are not valid plugin files.");
				
			file_lbl.Text = errors;
		}

		protected virtual void OnButtonOkClicked (object sender, System.EventArgs e)
		{
			this.Destroy ();
		}
	}
}
