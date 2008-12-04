// DefaultObjects.cs
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

namespace Do.Addins
{
	
	public class NoResultsFoundObject : IObject
	{
		string query;
		
		public NoResultsFoundObject (string query)
		{
			this.query = query;
		}

		public string Icon { get { return "gtk-dialog-question"; } }
		public string Name { get { return Catalog.GetString (
				                   string.Format("No results for {0}", query)); } }

		public string Description
		{
			get {
				return string.Format (Catalog.GetString (
				                      string.Format("No results found for {0}", query)));
			}
		}
	}
	
	public class DefaultIconBoxObject : IObject
	{
		public string Icon { get { return "search"; } }
		public string Name { get { return ""; } }
		public string Description { get { return ""; } }
	}
	
	public class DefaultLabelBoxObject : IObject
	{
		public string Icon { get { return "search"; } }
		public string Name { get { return Catalog.GetString ("Type to begin searching"); } }
		public string Description { get { return Catalog.GetString ("Type to start searching."); } }
	}
}
