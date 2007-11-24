/* DoItem.cs
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
using System.Collections;
using System.Collections.Generic;

using Do.Universe;

namespace Do.Core
{
	public class DoItem : DoObject, IItem
	{
		
		protected IItem item;
		
		public DoItem (IItem item):
			base (item)
		{
			this.item = item;
		}
		
		public IItem IItem {
			get { return item; }
		}
		
	}
	
	public class DoTextItem: DoItem, ITextItem
	{
		public DoTextItem (string text):
			base (new TextItem (text))
		{
		}
		
		public string Text {
			get { return (item as ITextItem).Text; }
		}
	}
}
