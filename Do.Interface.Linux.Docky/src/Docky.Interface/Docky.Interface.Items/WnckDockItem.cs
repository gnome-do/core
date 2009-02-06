//  
//  Copyright (C) 2009 GNOME Do
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
// 

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Gdk;
using Cairo;
using Mono.Unix;

using Do.Platform;
using Do.Universe;
using Do.Universe.Common;
using Do.Interface;
using Do.Interface.CairoUtils;

using Docky.Interface.Menus;
using Docky.Utilities;

using Wnck;

namespace Docky.Interface
{
	
	
	public abstract class WnckDockItem : AbstractDockItem
	{
		int last_raised;
		TimeSpan scroll_rate = new TimeSpan (0, 0, 0, 0, 300);
		DateTime last_scroll = new DateTime (0);
		
		protected abstract IEnumerable<Wnck.Application> Applications { get; }
		
		protected IEnumerable<Wnck.Window> VisibleWindows {
			get { return Applications.SelectMany (a => a.Windows).Where (w => !w.IsSkipTasklist); }
		}
		
		public WnckDockItem() : base ()
		{
			last_raised = 0;
		}
		
		public override void Scrolled (Gdk.ScrollDirection direction)
		{
			if (WindowCount < 1 || (DateTime.UtcNow - last_scroll) < scroll_rate) return;
			
			last_scroll = DateTime.UtcNow;
			
			// This block will make sure that if we're scrolling on an app that is already active
			// that when we scroll we move on the next window instead of appearing to do nothing
			Wnck.Window focused = VisibleWindows.Where (w => w.IsActive).FirstOrDefault ();
			if (focused != null) {
				for (; last_raised < WindowCount - 1; last_raised++) {
					if (VisibleWindows.ElementAt (last_raised).Pid == focused.Pid)
						break;
				}
			}
			
			switch (direction) {
			case ScrollDirection.Up:
			case ScrollDirection.Right: 
				last_raised++; 
				break;
			case ScrollDirection.Down:
			case ScrollDirection.Left: 
				last_raised--; 
				break;
			}
			
			KeepLastRaiseInBounds ();
			
			WindowControl.FocusWindows (VisibleWindows.ElementAt (last_raised));
		}
		
		void KeepLastRaiseInBounds ()
		{
			if (WindowCount <= last_raised)
				last_raised = 0;
			else if (last_raised < 0)
				last_raised = WindowCount - 1;
		}
	}
}
