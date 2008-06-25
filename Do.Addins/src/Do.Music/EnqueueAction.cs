//  EnqueueAction.cs
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

using Do.Universe;
using System;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;


namespace Do.Addins.DoMusic
{

	public class DoMusicEnqueueAction : AbstractAction
	{
		public override string Name	{get {return "Enqueue Music";}	}
		public override string Description {get {return "Enqueue a song via a DoMusic Music Source";} }
		public override string Icon	{get {return "list-add";} }
		public override Type[] SupportedItemTypes {		
			get {
				return new Type[] { typeof (InternalMusicItem)};
			}
		}

		public override bool SupportsItem (IItem item)
		{
			return (item is InternalMusicItem);
		}
		
		public override Type[] SupportedModifierItemTypes
		{
			get {
				return new Type[] {typeof (DoMusicEnqueueModifier)};
			}
		}
				
		public override IItem[] DynamicModifierItemsForItem (IItem item)
		{
			List<IItem> items = new List<IItem> ();
			if (item is InternalMusicItem) {
				InternalMusicItem imi = item as InternalMusicItem;
				MusicItem i = imi.Item;
				foreach (IMusicSource ims in DoMusic.GetSourcesFor ((InternalMusicItem) item)) {
					bool enqueue = false;
					if (i is Album)
						if (ims.EnableEnqueueAlbum)
							enqueue = true;
					if (i is Artist)
						if (ims.EnableEnqueueArtist)
							enqueue = true;
					if (i is Song)
						if (ims.EnableEnqueueSong)
							enqueue = true;
					
					if (enqueue && ims.IsAvailable () && imi.IsInSource (ims.internalSourceNum))
						items.Add (new DoMusicEnqueueModifier (ims.Name, ims.Description, ims.Icon, ims));					
				}
			}
			return items.ToArray ();
		}
	
		public override IItem[] Perform (IItem[] items, IItem[] modifierItems)
		{
			if (items == null || modifierItems == null || 
				items.Length == 0 || modifierItems.Length == 0)
				return null;
	
			InternalMusicItem iitem = items[0] as InternalMusicItem;
			DoMusicEnqueueModifier pm = modifierItems[0] as DoMusicEnqueueModifier;
			IMusicSource ims = pm.Source;
			
			//why? typecasting seems useless here
			MusicItem item = ((InternalMusicItem)iitem).GetItem(ims);		
					
			if(!ims.IsAvailable()){
				Console.WriteLine(ims.Name + " seems to be unavailable!  Not executing action.");
				return null;
			}
				
			if (item is Artist)
				ims.EnqueueArtist (item as Artist);
			else if (item is Album)
				ims.EnqueueAlbum (item as Album);
			else if (item is Song)
				ims.EnqueueSong (item as Song);

			return null;
		}
	}
}
