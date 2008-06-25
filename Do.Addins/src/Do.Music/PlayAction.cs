//  PlayAction.cs
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
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using Do.Universe;

namespace Do.Addins.DoMusic
{

	public class DoMusicPlayAction : AbstractAction
	{

		public override string Name	{get {return "Play Music";} }
		
		public override string Description {get {return "Play a song via a DoMusic Music Source";} }
		
		public override string Icon {get {return "player_play";} }
		
		public override Type[] SupportedItemTypes {	
			get {
				return new Type[] {typeof (InternalMusicItem)};
			}
		}

		public override bool SupportsItem (IItem item) {return (item is InternalMusicItem);}
		
		public override Type[] SupportedModifierItemTypes
		{
			get {
				if (Configuration.AllSources) {
					return new Type[] {typeof (DoMusicPlayModifier)};
				} else {
					return null;
				}
			}
		}
				
		public override IItem[] DynamicModifierItemsForItem (IItem item)
		{
			if (!Configuration.AllSources) return null;
					
			List<IItem> items = new List<IItem> ();
			if (item is InternalMusicItem) {
				InternalMusicItem imi = item as InternalMusicItem;
				MusicItem i = imi.Item;
				foreach (IMusicSource ims in DoMusic.GetSourcesFor (item as InternalMusicItem)) {				
					bool enqueue = false;
					if (i is Album)
						if (ims.EnablePlayAlbum)
							enqueue = true;
					if (i is Artist)
						if (ims.EnablePlayArtist)
							enqueue = true;
					if (i is Song)
						if (ims.EnablePlaySong)
							enqueue = true;
					
					if (enqueue && ims.IsAvailable () && imi.IsInSource (ims.internalSourceNum))
						items.Add (new DoMusicPlayModifier(ims.Name, ims.Description, ims.Icon,  ims));					
				}
			}
			return items.ToArray ();
		}			
		public override IItem[] Perform (IItem[] items, IItem[] modifierItems)
		{
			if (items.Length == 0)
				return null;
					
			InternalMusicItem iitem = items[0] as InternalMusicItem;
			IMusicSource ims = null;
					
			if (Configuration.AllSources) {				
				DoMusicPlayModifier pm = modifierItems[0] as DoMusicPlayModifier;
				ims = pm.Source;
			} else {
				ims = Configuration.CurrentSource;
			}
					
			//WHY!?!?!?!
			//FOR THE LOVE OF ALL THAT IS HOLY!!!
			MusicItem item = ((InternalMusicItem)iitem).GetItem (ims);
					
			if (!ims.IsAvailable ()) {
				//Console.WriteLine(ims.Name + " seems to be unavailable!  Not executing action.");
				return null;
			}
					
			if (item is Artist)
				ims.PlayArtist (item as Artist);
			else if (item is Album)
				ims.PlayAlbum (item as Album);
			else if (item is Song)
				ims.PlaySong (item as Song);
					
			return null;
		}
	}
}
