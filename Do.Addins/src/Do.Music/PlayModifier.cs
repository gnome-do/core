// PlayModifier.cs created with MonoDevelop
// User: zgold at 14:13 06/07/2008
//
//  GNOME Do is the legal property of its developers, whose names are too numerous
//  to list here.  Please refer to the COPYRIGHT file distributed with this
//  source distribution.
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.


using Do.Addins;
using Do.Universe;
using System;


namespace Do.Addins.DoMusic
{		
	public class DoMusicPlayModifier : IItem
	{
		private string name;
		private string description;
		private string icon;
		private IMusicSource ims;
		
		public DoMusicPlayModifier (string name, string description, string icon, IMusicSource ims)
		{
			this.name = name;
			this.description = description;
			this.icon = icon;
			this.ims = ims;
		}
		
		public IMusicSource Source {get {return ims;} } 
		
		public string Name {
			get {
				return "Play in " + name;				
			}
		}
		
		public string Description {
			get {
				return description;
			}
		}
		
		public string Icon {
			get {
				return icon;
			}
		}			
	}
}
