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

using Wnck;

namespace Do.Interface.Wink
{
	
	
	public static class ScreenUtils
	{
		public bool DesktopShow (Screen screen)
		{
			return screen.ShowingDesktop;
		}
		
		public static void ShowDesktop (Screen screen)
		{
			if (!screen.ShowingDesktop)
				screen.ToggleShowingDesktop (true);
		}
		
		public static void UnshowDesktop (Screen screen)
		{
			if (screen.ShowingDesktop)
				screen.ToggleShowingDesktop (false);
		}
		
		public static IEnumerable<Window> WorkspaceWindows (Workspace workspace)
		{
			foreach (Window window in WindowUtils.GetWindows ()) {
				if (window.Workspace == workspace)
					yield return window;
			}
		}
	}
}
