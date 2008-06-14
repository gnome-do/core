/* CopyToClipboard.cs
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
using Do.Universe;
using Gtk;
using Gdk;

namespace Do.Universe
{
	public class CopyToClipboard : AbstractAction
	{
		public override string Name {
			get { return "Copy to Clipboard"; }
		}
		
		public override string Description {
			get { return "Copy current text to clipboard"; }
		}
		
		public override string Icon {
			get { return "edit-paste"; }
		}
		
		public override Type[] SupportedItemTypes {
			get { return new Type [] { typeof (IItem), }; }
		}
		
		public override bool SupportsModifierItemForItems (IItem[] items, IItem modItem)
		{
			return false;
		}
		
		public override IItem[] Perform (IItem[] items, IItem[] modItems)
		{
			Gtk.Clipboard main = Gtk.Clipboard.Get (Gdk.Selection.Clipboard);
			main.Text = items [0].Description;
			return null;
		}
	}
}
