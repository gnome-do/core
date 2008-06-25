//  MusicItems.cs
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

	public abstract class MusicItem
	{
		protected string name, cover, artist;
		protected IMusicSource source;

		public MusicItem (string name, string cover, string artist, IMusicSource source)
		{
			this.name = name;
			this.cover = cover;
			this.artist = artist;
			this.source = source;
		}

		public string Name { get { return name; } }
		public virtual string Description { get { return name; } }
		public virtual string Icon { get { return Cover ?? "gtk-cdrom"; } }
		public string Cover { get { return cover; }	}
		public string Artist { get { return artist; } } 
		public IMusicSource Source { get { return source; } }
		
	}

	public class Album : MusicItem
	{		
		string album;
		
		public Album  (string name, string cover, string artist, IMusicSource source):
			base  (name, cover, artist, source)
		{
			this.album = name;
		}
		
		public override string Description
		{
			get {
				return string.Format  ("All music by {0} in {1}",  artist, album);
			}
		}
		
		public override bool Equals (Object obj) {
			if (obj == null || !(obj is Album)) return false;
			
			Album a = (Album)obj;
			
			return (a.album == this.album && a.artist == this.artist && a.name == this.name);
		}
		
		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}

	}

	public class Artist : MusicItem
	{
		public Artist  (string artist, string cover, IMusicSource source):
			base  (artist, cover, artist, source)
		{}

		public override string Description
		{
			get {
				return string.Format  ("All music by {0}", artist);
			}
		}
		
		public override bool Equals (Object obj) 
		{
			if (obj == null || ! (obj is Artist)) return false;
			
			Artist a = (Artist)obj;
			return (a.artist == this.artist && a.name == this.name);
		}
		
		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}
	}

	public class Song : MusicItem
	{
		string album, file;
		string other;
		
		public Song  (string name, string artist, string album, string cover, string file, string other, IMusicSource source):
			base  (name, cover, artist, source)
		{
			this.file = file;
			this.album = album;
			this.other = other;
		}

		public override string Icon { get { return "gnome-mime-audio"; } }
		public override string Description
		{
			get {
				return string.Format  ("{0} - {1} - {2} ",  artist,  album, name);
			}
		}
		public virtual string Album { get { return album; } }
		public virtual string File { get { return file; } }
		public virtual string Other { get {return other; } }
		
		public override bool Equals (Object obj)
		{
			if (obj == null || ! (obj is Song))
				return false;
			Song s =  (Song)obj;
			
			return (s.album.Equals(this.album) && s.artist.Equals(this.artist) && s.name.Equals(this.name));
		}

		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}
	}			
}
