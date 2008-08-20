// ShowCaseDrawingContext.cs
// 
// Copyright (C) 2008 GNOME-Do
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

using Do.Addins;
using Do.Universe;

namespace Do.UI
{
	public class ShowCaseDrawingContext
	{
		public IObject Main, Secondary, Tertiary;
		private string[] queries;
		private bool[] textMode;
		
		public string[] Queries {
			get {
				return queries ?? queries = new string[3];
			}
			set {
				queries = value;
			}
		}
		
		public bool[] TextMode {
			get {
				return textMode ?? textMode = new bool[3];
			}
			set {
				textMode = value;
			}
		}
		
		public IObject GetObjectForPane (Pane pane)
		{
			switch (pane) {
			case Pane.First:
				return Main;
			case Pane.Second:
				return Secondary;
			case Pane.Third:
				return Tertiary;
			}
			
			return null;
		}
		
		public void SetObjectForPane (Pane pane, IObject item)
		{
			switch (pane) {
			case Pane.First:
				Main = item;
				break;
			case Pane.Second:
				Secondary = item;
				break;
			case Pane.Third:
				Tertiary = item;
				break;
			}
		}
		
		public ShowCaseDrawingContext (IObject Main, IObject Secondary, IObject Tertiary, Pane focus)
		{
			this.Main = Main;
			this.Secondary = Secondary;
			this.Tertiary = Tertiary;
			Queries = new string [3];
		}
	}
}
