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
using System.Linq;

using Docky.Core;
using Docky.Interface;
using Docky.Utilities;

using Mono.Unix;

namespace Docky.Interface.Menus
{
	
	
	public class ToggleDockletMenuButtonArgs : AbstractMenuButtonArgs
	{
		AbstractDockletItem dock_item;
		
		public ToggleDockletMenuButtonArgs (AbstractDockletItem dockitem) : base ()
		{
			dock_item = dockitem;
			
			Description = Catalog.GetString ("Show") + " " + dockitem.Name;
			if (DockServices.DockletService.ActiveDocklets.Contains (dockitem))
				Icon = DoDockItem.EnableIcon;
			else
				Icon = DoDockItem.DisableIcon;
		}
		
		public override void Action ()
		{
			DockServices.DockletService.ToggleDocklet (dock_item);
		}

	}
}
