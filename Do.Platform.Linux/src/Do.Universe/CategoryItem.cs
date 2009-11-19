//  
//  Copyright (C) 2009 GNOME Do
// 
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
// 
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
// 

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

using Gnome;
using Mono.Unix;

using Do.Universe;
using Do.Platform;

namespace Do.Universe.Linux
{
	
	public class CategoryItem : Item
	{
		static Dictionary<string, CategoryItem> Instances { get; set; }

		static CategoryItem ()
		{
			Instances = new Dictionary<string, CategoryItem> ();
		}
		
		public static bool ContainsCategory (string category)
		{
			return Instances.ContainsKey (category.ToLower ());
		}
		
		public static CategoryItem GetCategoryItem (string category)
		{
			string lowCat = category.ToLower ();
			lock (Instances)
			{
				if (!Instances.ContainsKey (lowCat)) {
					CategoryItem item = new CategoryItem (category);
					Instances [lowCat] = item;
				}
			}
			return Instances [lowCat];
		}
		
		string name, descritpion, category;
		
		public override string Description {
			get { return descritpion; }
		}

		public override string Icon {
			get { return "applications-other"; }
		}

		public override string Name {
			get { return name; }
		}
		
		public string Category {
			get { return category; }
		}
		
		protected CategoryItem (string category)
		{
			this.category = category;
			name = string.Format (Catalog.GetString ("{0} Application Category"), category);
			descritpion = string.Format (Catalog.GetString ("Applications in the {0} category"), category);
		}
	}
}
