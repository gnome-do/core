// DefaultItems.cs
//
//  Copyright (C) 2008 Jason Smith
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
//
//

using System;
using Mono.Unix;

using Do.Universe;

namespace Do.Interface.Widgets
{
	
	public class NoResultsFoundItem : Item
	{
		string query;
		
		public NoResultsFoundItem (string query)
		{
			this.query = query;
		}

		public override string Icon { get { return "gtk-dialog-question"; } }
		public override string Name { get { return Catalog.GetString (string.Format("No results for {0}", query)); } }
		public override string Description {
			get {
				return string.Format (Catalog.GetString (string.Format("No results found for {0}", query)));
			}
		}
	}
	
	public class DefaultIconBoxItem : Item
	{
		public override string Icon { get { return "search"; } }
		public override string Name { get { return ""; } }
		public override string Description { get { return ""; } }
	}
	
	public class DefaultLabelBoxItem : Item
	{
		public override string Icon { get { return "search"; } }
		public override string Name { get { return Catalog.GetString ("Type to begin searching"); } }
		public override string Description { get { return ""; } }
	}
}
