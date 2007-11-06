//  RhythmboxPlayCommand.cs
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
using System.Diagnostics;

using RhythmboxAddin;

namespace Do.Universe
{

	public class RhythmboxPlayCommand : ICommand
	{
		
		public RhythmboxPlayCommand ()
		{
		}
		
		public string Name {
			get { return "Play"; }
		}
		
		public string Description {
			get { return "Play an Item in Rhythmbox."; }
		}
		
		public string Icon {
			get { return "player_play"; }
		}
		
		public Type[] SupportedItemTypes {
			get {
				return new Type[] {
					typeof (DirectoryFileItem),
				};
			}
		}
		
		public Type[] SupportedModifierItemTypes {
			get { return null; }
		}

		public bool SupportsItem (IItem item) {
			return true;
		}
		
		public bool SupportsModifierItemForItems (IItem[] items, IItem modItem)
		{
			return false;
		}
		
		public void Perform (IItem[] items, IItem[] modifierItems)
		{
			Rhythmbox.StartIfNeccessary ();
			
			Rhythmbox.Client ("--clear-queue --no-present", true);
			foreach (IItem item in items) {
				string music;
				
				if (item is DirectoryFileItem) {
					music = (item as DirectoryFileItem).URI;
				} else {
					continue;
				}
				Rhythmbox.Client (string.Format ("--enqueue \"{0}\" --no-present", music));
			}
			Rhythmbox.Client ("--next --play --no-present");
		}
	}
}
