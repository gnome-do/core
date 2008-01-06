/* ${FileName}
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
using System.Text.RegularExpressions;
using Mono.Unix;

using Do.Addins;

namespace Do.Universe
{
	public class OpenURLCommand : ICommand
	{
		const string urlPattern = @"(^\w+:\/\/\w+)|(\w+\.\w+)";
		
		Regex urlRegex;
		
		public OpenURLCommand ()
		{
			urlRegex = new Regex (urlPattern, RegexOptions.Compiled);
		}
		
		public string Name
		{
			get { return  Catalog.GetString ("Open URL"); }
		}
		
		public string Description
		{
			get { return  Catalog.GetString ("Opens bookmarks and manually-typed URLs."); }
		}
		
		public string Icon
		{
			get { return "web-browser"; }
		}
		
		public Type[] SupportedItemTypes
		{
			get {
				return new Type[] {
					typeof (IURLItem),
					typeof (ITextItem),
				};
			}
		}
		
		public Type[] SupportedModifierItemTypes
		{
			get {
				return null;
			}
		}

		public bool SupportsItem (IItem item)
		{
			if (item is ITextItem) {
				return urlRegex.IsMatch ((item as ITextItem).Text);
			}
			return true;
		}
		
		public bool SupportsModifierItemForItems (IItem[] items, IItem modItem)
		{
			return false;
		}
		
		public void Perform (IItem[] items, IItem[] modifierItems)
		{
			string url;
			
			url = null;
			foreach (IItem item in items) {
				if (item is IURLItem) {
					url = (item as IURLItem).URL;
				} else if (item is ITextItem) {
					url = (item as ITextItem).Text;
				}
				Util.Environment.Open (url);	
			}
		}
	}
}
