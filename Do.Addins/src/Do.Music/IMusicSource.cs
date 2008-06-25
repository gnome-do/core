// IMusicSource.cs created with MonoDevelop
// User: zgold at 20:09 06/04/2008
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

using Do.Addins;
using Do.Universe;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Do.Addins.DoMusic
{

	public abstract class IMusicSource : AbstractItemSource, IItem
	{	    					
		public IMusicSource ()
		{
			//Tell DoMusic we exist so it'll ask us for items and such things
			DoMusic.RegisterMusicSource (this);
			//host = new ControlActionHost(this.Name + " Control", this.Icon, this);
			DoMusic.RegisterItemSource (this);		
			
			OnUnload += UnloadPlugin;
		}
		
		public override void UpdateItems () {}
		public override ICollection<IItem> ChildrenOfItem (IItem i) {return null;}
		public override ICollection<IItem> Items {get {return DoMusic.ItemsFromSource (this); } }
		public override Type[] SupportedItemTypes {get {return new Type[] {typeof (ControlActionHost)}; } }		
		
		//End AA Nonsense
		public override string Name { get { return SourceName + " Music Source"; } }
		public abstract string SourceName { get; }
		public override abstract string Icon { get; }
		public override string Description { get { return Name; } }

		private bool enableEnqueueArtist = true;
		private bool enableEnqueueAlbum  = true;
		private bool enableEnqueueSong   = true;
		private bool enablePlayArtist    = true;
		private bool enablePlayAlbum     = true;
		private bool enablePlaySong      = true;
			
		public bool EnableEnqueueArtist  {
				get { return enableEnqueueArtist; }
				set { enableEnqueueArtist = value; }
		}
		
		public bool EnableEnqueueAlbum  {
				get { return enableEnqueueAlbum; }
				set { enableEnqueueAlbum = value; }
		}

		public bool EnableEnqueueSong  {
				get { return enableEnqueueSong; }
				set { enableEnqueueSong = value; }
		}

		public bool EnablePlayArtist  {
				get { return enablePlayArtist; }
				set { enablePlayArtist = value; }
		}

		public bool EnablePlayAlbum  {
				get { return enablePlayAlbum; }
				set { enablePlayAlbum = value; }
			}
			
		public bool EnablePlaySong  {
				get { return enablePlaySong; }
				set { enablePlaySong = value; }
		}			
			
		public bool ShouldUpdate(){
			return false;
		}

		public int internalSourceNum;
		
		public abstract void Initialize ();
		
		public abstract bool IsAvailable ();
					
		public abstract void LoadMusicList (out List<Artist> artists, 
		                                    out List<Album> albums, 
		                                    out List<Song> songs);			
		
		public abstract void Play ();
		
		public abstract void PauseResume ();
		
		public abstract void Next ();
		
		public abstract void Prev ();
		
		public abstract void PlayArtist (Artist a);
		
		public abstract void PlayAlbum (Album a);			
		
		public abstract void PlaySong (Song song);
		
		public abstract void EnqueueSong (Song song);
		
		public abstract void EnqueueAlbum (Album album);

		public abstract void EnqueueArtist (Artist artist);
		
		public void UnloadPlugin ()
		{
			DoMusic.UnRegisterMusicSource (this);
		}
	}
}
