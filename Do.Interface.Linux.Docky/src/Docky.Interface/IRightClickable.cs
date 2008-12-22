// IRightClickable.cs
// 
// Copyright (C) 2008 GNOME Do
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

using System;
using System.Collections.Generic;

namespace Docky.Interface
{
	public class MenuArgs
	{
		public EventHandler Handler {
			get; private set;
		}
		
		public string Description {
			get; private set;
		}
		
		public string Icon {
			get; private set;
		}
		
		public bool Sensitive {
			get; private set; 
		}
		
		public MenuArgs (EventHandler handler, string description, string icon, bool sensitive)
		{
			Handler = handler;
			Description = description;
			Icon = icon;
			Sensitive = sensitive;
		}
	}
	
	public class SeparatorMenuArgs : MenuArgs
	{
		public SeparatorMenuArgs () : base (null, "Separator", null, true)
		{
		}
	}
	
	public interface IRightClickable
	{
		IEnumerable<MenuArgs> GetMenuItems ();
	}
}
