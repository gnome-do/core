// OpenUrlAction.cs
//
// GNOME Do is the legal property of its developers. Please refer to the
// COPYRIGHT file distributed with this source distribution.
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Mono.Unix;

using Do.Platform;

namespace Do.Universe.Common
{

	public class OpenUrlAction : Act
	{

		// Url regex taken from http://www.osix.net/modules/article/?id=586
		const string UrlPattern = "^(https?://)"
	        + "?(([0-9a-zA-Z_!~*'().&=+$%-]+: )?[0-9a-zA-Z_!~*'().&=+$%-]+@)?" //user@
	        + @"(([0-9]{1,3}\.){3}[0-9]{1,3}" // IP- 199.194.52.184
	        + "|" // allows either IP or domain
	        + @"([0-9a-zA-Z_!~*'()-]+\.)*" // tertiary domain(s)- www.
	        + @"([0-9a-zA-Z][0-9a-zA-Z-]{0,61})?[0-9a-zA-Z]\." // second level domain
	        + "[a-zA-Z]{2,6})" // first level domain- .com or .museum
	        + "(:[0-9]{1,4})?" // port number- :80
	        + "((/?)|" // a slash isn't required if there is no file name
	        + "(/[0-9a-zA-Z_!~*'().;?:@&=+$,%#-]+)+/?) *$";
		
		readonly Regex url_regex;
		
		public OpenUrlAction ()
		{
			url_regex = new Regex (UrlPattern, RegexOptions.Compiled);
		}
		
		public override string Name {
			get { return  Catalog.GetString ("Open Url"); }
		}
		
		public override string Description {
			get { return  Catalog.GetString ("Opens bookmarks and manually-typed Urls."); }
		}
		
		public override string Icon {
			get { return "web-browser"; }
		}
		
		public override IEnumerable<Type> SupportedItemTypes {
			get {
				yield return typeof (IUrlItem);
				yield return typeof (ITextItem);
			}
		}

		public override bool SupportsItem (Item item)
		{
			if (item is ITextItem)
				return url_regex.IsMatch ((item as ITextItem).Text);
			return true;
		}
		
		public override IEnumerable<Item> Perform (IEnumerable<Item> items, IEnumerable<Item> modItems)
		{
			IEnvironmentService env = Services.Environment;

			foreach (Item item in items) {
				if (item is IUrlItem)
					env.OpenUrl ((item as IUrlItem).Url);
				else if (item is ITextItem)
					env.OpenUrl ((item as ITextItem).Text);
			}
			return null;
		}
	}
}
