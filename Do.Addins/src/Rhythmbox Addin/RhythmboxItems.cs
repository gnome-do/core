//  RhythmboxItems.cs
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

using Do.Addins;
using RhythmboxAddin;

namespace Do.Universe
{
	
	
	public class RhythmboxRunnableItem : IRunnableItem
	{
		public static readonly RhythmboxRunnableItem[] DefaultItems =
		  new RhythmboxRunnableItem[] {
			
			new RhythmboxRunnableItem ("Play",
		                               "Play Current Track in Rhythmbox",
			                           "player_play",
			                           "--play"),
			
			new RhythmboxRunnableItem ("Pause",
			                           "Pause Rhythmbox Playback",
			                           "player_pause",
			                           "--play-pause"),
			
			new RhythmboxRunnableItem ("Next",
			                           "Play Next Track in Rhythmbox",
			                           "player_end",
			                           "--next"),
			
			new RhythmboxRunnableItem ("Previous",
			                           "Play Previous Track in Rhythmbox",
			                           "player_start",
			                           "--previous"),
			
			new RhythmboxRunnableItem ("Show Current Track",
			                           "Show Notification of Current Track in Rhythmbox",
			                           "gnome-mime-audio",
			                           "--notify"),
			
			new RhythmboxRunnableItem ("Mute",
			                           "Mute Rhythmbox Playback",
			                           "audio-volume-muted",
			                           "--mute"),
			
			new RhythmboxRunnableItem ("Unmute",
			                           "Unmute Rhythmbox Playback",
			                           "audio-volume-high",
			                           "--unmute"),
			
			new RhythmboxRunnableItem ("Volume Up",
			                           "Increase Rhythmbox Playback Volume",
			                           "audio-volume-high",
			                           "--volume-up"),
			
			new RhythmboxRunnableItem ("Volume Down",
			                           "Decrease Rhythmbox Playback Volume",
			                           "audio-volume-low",
			                           "--volume-down"),
		};
		
		string name, description, icon, command;
		
		public RhythmboxRunnableItem (string name, string description, string icon, string command)
		{
			this.name = name;
			this.description = description;
			this.icon = icon;
			this.command = command;
		}
		
		public string Name { get { return name; } }
		public string Description { get { return description; } }
		public string Icon { get { return icon; } }
		
		public void Run ()
		{
			new Thread ((ThreadStart) delegate {
				Rhythmbox.StartIfNeccessary ();
				Rhythmbox.Client (command);
			}).Start ();
		}
		
	}
}
