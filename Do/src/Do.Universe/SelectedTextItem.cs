/* SelectedSelectedTextItem.cs
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

using Do.Platform;

namespace Do.Universe {

	public class SelectedTextItem : IProxyItem, ITextItem {		
		
		private static string text;
		
		public IObject Inner {
			get {
				return Services.UniverseFactory.NewTextItem (Text);
			}
		}
		
		public string Name {
			get { return Catalog.GetString ("Selected text"); }
		}
		
		public string Description {
			get { return Catalog.GetString ("Currently selected text."); }
		}
		
		public string Icon {
			get { return "gtk-select-all"; }
		}
		
		public string Text {
			get { return text; }
		}
		
		public static void UpdateText ()
		{
			Gtk.Clipboard primary;
			
			primary = Gtk.Clipboard.Get (Gdk.Selection.Primary);
			if (primary.WaitIsTextAvailable ()) {
				text = primary.WaitForText ();
			} else {
				text = "";
			}
		}
	}
}
