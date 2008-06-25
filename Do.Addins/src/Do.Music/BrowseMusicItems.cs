// BrowseMusicItems.cs created with MonoDevelop
// User: zgold at 23:13 06/05/2008
// 
// Copyright (C) 2008 [Zach Goldberg]
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

using Do.Addins;
using Do.Universe;
using System;


namespace Do.Addins.DoMusic
{
	                                                                                
	class BrowseAllMusicItem : IItem                                                            
	{                                                                                                             
		private string name, description;                
		Artist artist;                                                                               
		public BrowseAllMusicItem (Artist artist)                                                                                  
		{                                                                                                     
			this.name = "Browse Music";
			this.description = "All songs by " + artist.Artist;
			this.artist = artist;                                                                         
		}                                                                                                     
		public Artist Artist { get { return artist; } }
		public string Name { get { return name; } }                                                           
		public string Description { get { return description; } }                                             
		public string Icon { get { return artist.Cover ?? "gtk-cdrom"; } }    
	}    	
}
