//  InternalMusicItem.cs
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

using System;
using System.Collections.Generic;

using Do.Addins;
using Do.Universe;

namespace Do.Addins.DoMusic
{

	public class InternalMusicItem : IItem
	{
		private Dictionary<IMusicSource, MusicItem> items;
		private bool[] sources;
		private string name;
		private string artist;
		
		public InternalMusicItem (MusicItem i, IMusicSource ims, int numSources) 
		{
			items = new Dictionary<IMusicSource,MusicItem> ();
			items.Add (ims,i);
			this.sources = new bool[numSources];
			this.name = i.Name;
			this.artist = i.Artist;
		}
		
		public void addItem (MusicItem i, IMusicSource ims)
		{
			if (!items.ContainsKey (ims))
				items.Add (ims, i);			
		}
			
		public bool IsInSource (int sourceNum)
		{ 
			if (sourceNum > sources.Length-1)
				return false;
			return sources[sourceNum];
		}
		
		public void SetInSource (int sourceNum)
		{
			if (sourceNum >= sources.Length) {
				bool[] n_sources = new bool[sourceNum+1];
				sources.CopyTo (n_sources,0);
				sources = n_sources;
			}
			
			sources[sourceNum] = true; 
		} 
		
		public MusicItem GetItem (IMusicSource ims)
		{
			return items[ims];
		}
		
		public virtual string Name { get { return name; } }
		public virtual string Description { get { return name; } }
		public virtual string Icon { get { return this.Cover ?? "gtk-cdrom"; } }
		public virtual string Cover { 
			get {
				foreach (MusicItem i in items.Values) {
					if (i.Cover != null)
						return i.Cover;
				}
				return null;
			} 
		}
		public virtual string Artist { get { return artist; } }
		
		/// <value>
		/// DEPRECATED.  Be very vareful about using this.  Only use it if your using to compare
		/// using .equals to something else.  In that case your comparing based on the item's artist
		/// and name etc., which will be the same for all items in this internal music item.
		/// </value>
		public MusicItem Item { 
			get { 
				foreach(MusicItem i in items.Values) { 
					return i; 
				} return null; 
			} 
		} //even the for loop is a shitty way of doing this 
	}			
}
