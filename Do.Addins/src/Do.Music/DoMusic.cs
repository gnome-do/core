//  DoMusic.cs
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
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Timers;



namespace Do.Addins.DoMusic
{
	
	public static class DoMusic
	{			
		private static DateTime lastReset = DateTime.MinValue;		
		private static int lastResetSourceCount = -1;
		
		private static List<IMusicSource> sources;					
		private static List<IItemSource> isources;
		private static List<InternalMusicItem> albums;
		private static List<InternalMusicItem> artists;
		private static List<InternalMusicItem> songs;	
		
		private static Dictionary<IMusicSource, List<IItem>> source_albums;
		private static Dictionary<IMusicSource, List<IItem>> source_artists;		

		static DoMusic ()
		{			
			//Console.WriteLine ("Loading DoMusic...");
			isources  = new List<IItemSource> ();
			sources   = new List<IMusicSource> ();
			resetPrivateData ();
		}
		
		private static void resetPrivateData ()
		{			
			albums    = new List<InternalMusicItem> ();
			artists   = new List<InternalMusicItem> ();
			songs     = new List<InternalMusicItem> ();									
			source_albums  = new Dictionary<IMusicSource,List<IItem>> ();
			source_artists = new Dictionary<IMusicSource,List<IItem>> ();	
		}
		
		public static void RegisterMusicSource (IMusicSource ims)
		{
			try {
				sources.Add (ims);
				ims.Initialize ();
				ims.internalSourceNum = sources.Count - 1;
				//Console.WriteLine ("Registering a music source: " + ims.Name + " with ID " + ims.internalSourceNum);
				loadContent (ims);
				if (Configuration.CurrentSource == null)
					Configuration.CurrentSource = ims;				
				UpdateItemSources ();	
			} catch (Exception e) {
				Console.Error.WriteLine (e.Message);
				Console.Error.WriteLine (e.StackTrace);
			}
		}
		
		public static void UnRegisterMusicSource (IMusicSource ims)
		{		
			if (sources.Contains(ims))
				sources.Remove (ims);
			if (Configuration.CurrentSource == ims)
				if (sources.Count != 0)
					Configuration.CurrentSource = sources[0];
			//We dropped a music source, so reload all InternalMusicItems
			lastReset = DateTime.MinValue;
			ReloadMusicSources ();
		}
		
		public static void RegisterItemSource (IItemSource iis)
		{
			isources.Add (iis);
		}
				
		public static void ReloadMusicSources ()
		{			
			/*
			 * This should NOT reload every time.
			 * Instead it should check a timer; and then if the timer is up
			 * ask the music source itself!  Only then should it
			 * actually reload.
			 */
			//Console.WriteLine(lastResetSourceCount + "," + sources.Count);
			
			if (lastReset != DateTime.MinValue && DateTime.Now <= lastReset.AddSeconds(60) && 
			    sources.Count == lastResetSourceCount) return;
			
			resetPrivateData ();
			
			foreach (IMusicSource ims in sources) {
				loadContent (ims);
			}
			
			lastReset = DateTime.Now;
			lastResetSourceCount = sources.Count;
		}
		
		public static void UpdateItemSources ()
		{
			foreach (IItemSource iss in isources)
				iss.UpdateItems ();					
		}
		
		public static bool IsSource (String name)
		{
			foreach (IMusicSource ims in sources) {
				if (ims.Name.Equals(name))
					return true;
			}
			return false;
		}
		
		public static List<IMusicSource> GetSourcesFor(InternalMusicItem item)
		{
			List<IMusicSource> r_sources = new List<IMusicSource>();			
			foreach (IMusicSource ims in sources) {
				if (item.IsInSource (ims.internalSourceNum)) {
				   r_sources.Add (ims);
				}
			}
			return r_sources;
		}
		
		public static List<IMusicSource> GetSources ()
		{
			return sources;
		}
		
		public static List<InternalMusicItem> GetAlbums ()
		{
			return albums;
		}
		
		public static List<InternalMusicItem> GetArtists ()
		{
			return artists;
		}
		
		private static void loadContent (IMusicSource ims) 
		{
			List<Album> s_albums;
			List<Artist> s_artists;
			List<Song> s_songs;
			try {
				ims.LoadMusicList (out s_artists, out s_albums, out s_songs);
			} catch (Exception e) {
				DoMusic.UnRegisterMusicSource (ims);
				Console.Error.WriteLine("Error loading music from " + ims.Name +".  Error: " + e.Message);
				return;
			}
			
			if (s_artists == null || s_albums == null || s_songs == null) return;

			List<IItem> ims_artists =  new List<IItem>();
			List<IItem> ims_albums  =  new List<IItem>();
			
			//Can't help doing the same code three times here... wish i could see a better way			
			foreach (Artist a in s_artists) {
				InternalMusicItem ia = null;

				//FIX THIS, you are doing the search twice simply for a null check
				if (artists.Find (delegate (InternalMusicItem ss) {return ss.Item.Equals(a);}) != null) {
					ia = artists.Find (delegate (InternalMusicItem i) {
						return i.Item.Equals (a);
					});
					
					ia.addItem (a, ims);
					ia.SetInSource (ims.internalSourceNum);
				} else {
					ia = new InternalMusicItem (a, ims, sources.Count);
					ia.SetInSource (ims.internalSourceNum);
					artists.Add (ia);
				}					
				ims_artists.Add (ia);
			}
			
			foreach (Album a in s_albums) {
				InternalMusicItem ia = null;
				if (albums.Find (delegate (InternalMusicItem ss) {return ss.Item.Equals(a);}) != null) {
					ia = albums.Find (delegate (InternalMusicItem i) { 
						return i.Item.Equals (a);
					});
					
					ia.addItem (a, ims);
					ia.SetInSource (ims.internalSourceNum);
				} else {
					ia = new InternalMusicItem (a, ims, sources.Count);
					ia.SetInSource (ims.internalSourceNum);						
					albums.Add (ia);
				}
				ims_albums.Add (ia);
			}
			
			foreach (Song s in s_songs) {
				InternalMusicItem ia = null;
				if (songs.Find (delegate (InternalMusicItem i) {return i.Item.Equals(s);}) != null) {
					ia = songs.Find (delegate (InternalMusicItem i) { 
						return i.Item.Equals (s); 
					});
					
					ia.addItem (s, ims);
					ia.SetInSource (ims.internalSourceNum);
				} else {
					ia = new InternalMusicItem (s, ims, sources.Count);
					ia.SetInSource (ims.internalSourceNum);
					songs.Add (ia);
				}	
			}
			
			source_artists.Add (ims, ims_artists);
			source_albums.Add (ims, ims_albums);
		}
		
		public static List<Song> LoadSongsFor (MusicItem item, IMusicSource ims)                                     
		{                                                                                                     			                                                                                                                                                                                                                                                                                                                                                    
			List<Song> r_songs = new List<Song> ();
			foreach (InternalMusicItem ima in songs) {
				Song song = (Song) ima.Item;
				if (item is Album)                                                        
					if (item.Name != song.Album || item.Artist != song.Artist ) continue;                              
					                                                            
				if (item is Artist)                                                       
					if (item.Name != song.Artist) continue;                             
					                                                                        				                                                                                
				if (item is Song)
					if (item != song) {
						continue;
					} else {
						r_songs.Add ((Song)ima.GetItem (ims));
						return r_songs;  //Shortcut to the end, we found the song we wanted
					}
				r_songs.Add ((Song)ima.GetItem (ims));                                 
			}                                                                                             			                                                                                             
			return r_songs;                                                                               
		}    
		
		public static List<InternalMusicItem> LoadSongsFor (MusicItem item)                                     
         {                                                                                                     			                                                                                                                                                                                                                                                                                                                                                    
			List<InternalMusicItem> r_songs = new List<InternalMusicItem> ();                                           
			foreach (InternalMusicItem ima in songs) {
				Song song = (Song) ima.Item;
				
				if (item is Album)                                                        
					if (item.Name != song.Album || item.Artist != song.Artist ) continue;                              
					                                                            
				if (item is Artist)                                                       
					if (item.Name != song.Artist) continue;                             
					                                                                        				                                                                                
				if (item is Song)
					if (item != song)
						continue;
					else {
						r_songs.Add (ima);
						return r_songs;  //Shortcut to the end, we found the song we wanted
					}
				r_songs.Add (ima);                                 
			}                                                                                             			                                                                                             
			return r_songs;                                                                               
		}       
		
		public static List<IItem> ItemsFromSource (IMusicSource ims)
		{
			List<IItem> items = new List<IItem> ();
			items.AddRange (source_artists[ims]);		
			items.AddRange (source_albums[ims]);
			return items;
		}
	}
}
