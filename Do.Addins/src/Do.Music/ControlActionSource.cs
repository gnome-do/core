//  ControlActionSource.cs
//  Zach Goldberg
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Do.Addins.DoMusic
{	
	public class ControlActionHost : IItem
	{
		private string name;
		private string icon;
		private IMusicSource ims;
		
		public ControlActionHost (string name, string icon, IMusicSource ims)
		{
			this.name = name;
			this.icon = icon;
			this.ims = ims;
		}
		public virtual string Name { get { return name; } }
		public virtual string Description { get { return name; } }
		public virtual string Icon { get { return icon; } }
		public virtual IMusicSource Source { get { return ims; } } 
	}
	
	
	public class ControlActionSource : AbstractItemSource
	{
		private List<IItem> items;	
		
		public override string Icon { get { return "gtk-cdrom" ; } }
		public override string Description { get { return ""; } } 
		
		public override string Name {
			get {
				if (Configuration.AllSources)
					return "Music Control (All Sources)";
				else
					return "Music Control (" + Configuration.CurrentSource.SourceName + ")" ;
			}
		}
		
		public override Type[] SupportedItemTypes {
			get {
				return new Type[] {
					typeof  (ControlActionHost)
				};
			}
		}		
		
		public override ICollection<IItem> Items { get { return items; } }
		
		public ControlActionSource ()
		{
			items = new List<IItem> ();
			DoMusic.RegisterItemSource (this);
			UpdateItems ();
		}
		
		public override void UpdateItems ()
		{			
			items.Clear ();
			if (Configuration.AllSources) {
				foreach (IMusicSource ims in DoMusic.GetSources ())
					items.Add (new ControlActionHost (ims.SourceName + " Control", ims.Icon, ims));
			} else {
				items.Add (new ControlActionHost (Configuration.CurrentSource + " Control", 
				                                  Configuration.CurrentSource.Icon, 
				                                  Configuration.CurrentSource));
			}
		
		}
		
		public override ICollection<IItem> ChildrenOfItem (IItem i)
		{
			return null;
		}
	}
}