//  GNOMEBookmarkItem.cs
//
//  GNOME Do is the legal property of its developers, whose names are too numerous
//  to list here.  Please refer to the COPYRIGHT file distributed with this
//  source distribution.
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

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using Mono.Unix;

using Do.Platform;
using Do.Universe;

namespace Do.Universe.Linux {

	class GNOMEBookmarkItem : Item, IUriItem
	{  
		string uri, icon, name;

		public GNOMEBookmarkItem (string name, string uri)
		{
			this.name = name;
			this.uri = uri;

			icon = "user-bookmarks";
		}

		public GNOMEBookmarkItem (string name, string uri, string icon)
			: this (name, uri)
		{
			this.icon = icon;
		}

		public override string Name  {
			get { return name; }
		}		
		
		public override string Description  { 
			get { return Uri; } 
		}

		public string Uri {
			get { return uri; }
		}		
		
		public override string Icon  {
			get { return icon; }
		}
		
	}
}
