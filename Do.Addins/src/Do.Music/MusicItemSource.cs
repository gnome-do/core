//  MusicItemSource.cs
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
using System.IO;
using System.Collections.Generic;

using Do.Addins;
using Do.Universe;

namespace Do.Addins.DoMusic
{

	public class MusicItemSource : AbstractItemSource
	{
		List<IItem> items;
		List<InternalMusicItem> albums;
		List<InternalMusicItem> artists;

		public MusicItemSource ()
		{
			items = new List<IItem> ();
			DoMusic.RegisterItemSource (this);
			UpdateItems ();
		}

		public override string Name { get { return "Do Music"; } }
		public override string Description { get { return "Provides access to artists and albums from Do Music Plugins."; } }
		public override string Icon { get { return "gnome-audio"; } }

		public override Type[] SupportedItemTypes {
			get {
				return new Type[] {
					typeof  (InternalMusicItem),
					typeof (BrowseAllMusicItem)
				};
			}
		}

		public override ICollection<IItem> Items { get { return items; } }

		public override ICollection<IItem> ChildrenOfItem  (IItem parent) {
			List<IItem> children = new List<IItem> ();
			
			if (parent is InternalMusicItem) {		
				InternalMusicItem ima = (InternalMusicItem) parent;
				MusicItem item = ima.Item;					
				//ARTIST
				if (item is Artist) {	
					//Show all the albums then an 'all music by this artist' button
					foreach  (InternalMusicItem album in AllAlbumsBy (item as Artist))
						children.Add (album);
					children.Add (new BrowseAllMusicItem (item as Artist));
				} else if (item is Album) {
					foreach (InternalMusicItem song in DoMusic.LoadSongsFor (item as Album)) {						
						children.Add (song);
					}
				}
			} else if (parent is BrowseAllMusicItem) {
				//Console.WriteLine("All Music");
				foreach  (InternalMusicItem song in DoMusic.LoadSongsFor ((parent as BrowseAllMusicItem).Artist))
					children.Add (song);
			}
			return children;
		}

		public override void UpdateItems ()
		{
			items.Clear ();	
			DoMusic.ReloadMusicSources ();
			if (!Configuration.AllSources)
					items.Add(new EnableAllSources());
				
			albums = DoMusic.GetAlbums ();
			artists = DoMusic.GetArtists ();			
			foreach (IItem album in albums)
				items.Add (album);			
			foreach (IItem artist in artists)
				items.Add (artist);					
		}

		protected List<InternalMusicItem> AllAlbumsBy (Artist artist)
		{
			return albums.FindAll (delegate (InternalMusicItem album) {
				return album.Artist == artist.Name;
			});
		}			
	}
}
