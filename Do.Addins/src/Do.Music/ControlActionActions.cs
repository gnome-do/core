// ControlActionActions.cs
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

using Do.Universe;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Do.Addins.DoMusic
{
	/// <summary>
	/// PLAY
	/// </summary>
	public class ControlActionPlay : AbstractAction
	{

		public override string Name {get {return "Play Music";} }
		public override string Description {get {return "Play Music via a Do Music Music Source";} }
		public override string Icon	{get {return "player_play";} }		
		
		public override Type[] SupportedItemTypes {
			get {
				return new Type[] {typeof (ControlActionHost) }; 
			} 
		}

		public override bool SupportsItem (IItem item)
		{
			if (item is ControlActionHost){
				ControlActionHost ca = item as ControlActionHost;
				if (Configuration.AllSources || Configuration.CurrentSource.SourceName == ca.Source.SourceName)				
					return true;
			}
			return false;
		}
	
		public override IItem[] Perform (IItem[] items, IItem[] modifierItems)
		{			
			ControlActionHost iitem = items[0] as ControlActionHost;
			iitem.Source.Play();			
			return null;
		}
	}
	
	/// <summary>
	/// PAUSE
	/// </summary>
	public class ControlActionPauseResume : AbstractAction
	{

		public override string Name {get {return "Pause/Resume Music";} }
		public override string Description {get {return "Pause / Resume Music via a Do Music Music Source";} }
		public override string Icon	{get {return "player_pause";} }		
		
		public override Type[] SupportedItemTypes  { 
			get {	
				return new Type[] {typeof (ControlActionHost),}; 
			} 
		}

		public override bool SupportsItem (IItem item)
		{
			if (item is ControlActionHost) {
				ControlActionHost ca = item as ControlActionHost;
				
				return (Configuration.AllSources || 
				    Configuration.CurrentSource.SourceName == ca.Source.SourceName);
			}
			return false;
		}
		
		public override IItem[] Perform (IItem[] items, IItem[] modifierItems)
		{			
			(items[0] as ControlActionHost).Source.PauseResume ();
			return null;
		}
	}
	
	/// <summary>
	/// NEXT
	/// </summary>
	public class ControlActionNext : AbstractAction
	{
		public override string Name {get {return "Next Song";} }
		public override string Description {get {return "Go to the next song in a Do Music Music Source";} }
		public override string Icon {get {return "player_end";} }
		
		public override Type[] SupportedItemTypes {
			get {
				return new Type[] {typeof (ControlActionHost),};	
			}		
		}

		public override bool SupportsItem (IItem item)
		{
			if (item is ControlActionHost) {
				ControlActionHost ca = item as ControlActionHost;
				return (Configuration.AllSources || 
				        Configuration.CurrentSource.SourceName == ca.Source.SourceName)	;			
			}
			return false;
		}
		
	
		public override IItem[] Perform (IItem[] items, IItem[] modifierItems)
		{			
			(items[0] as ControlActionHost).Source.Next ();
			return null;
		}
	}
	
	/// <summary>
	/// PREVIOUS
	/// </summary>
	public class ControlActionPrevious : AbstractAction
	{
		public override string Name {get {return "Previous Song";} }
		public override string Description {get {return "Go to the previous song in a Do Music Music Source";} }
		public override string Icon	{get {return "player_start"; } }	
		
		public override Type[] SupportedItemTypes  { 
			get {	
				return new Type[] {typeof (ControlActionHost),};	
			}		
		}

		public override bool SupportsItem (IItem item)
		{
			if (item is ControlActionHost){
				ControlActionHost ca = item as ControlActionHost;
				return (Configuration.AllSources || 
				        Configuration.CurrentSource.SourceName == ca.Source.SourceName)	;			
			}
			return false;
		}
		
		public override IItem[] Perform (IItem[] items, IItem[] modifierItems)
		{			
			(items[0] as ControlActionHost).Source.Prev ();
			return null;
		}
	}
}
