// BezelDefaults.cs
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

namespace Do.Interface.AnimationBase
{
	public interface IBezelDefaults
	{
		int WindowBorder { get; }
		int WindowRadius { get; }
		string HighlightFormat { get; }
		bool RenderDescriptionText { get; }
	}
	
	public class HUDBezelDefaults : IBezelDefaults
	{
		
		public int WindowBorder {
			get {
				return 21;
			}
		}

		public int WindowRadius {
			get {
				return 6;
			}
		}

		public string HighlightFormat {
			get {
				return "<span foreground=\"#5599ff\">{0}</span>";
			}
		}
		
		public bool RenderDescriptionText {
			get {
				return true;
			}
		}
	}
	
	public class ClassicBezelDefaults : IBezelDefaults
	{
		
		public int WindowBorder {
			get {
				return 17;
			}
		}

		public int WindowRadius {
			get {
				return 20;
			}
		}

		public string HighlightFormat {
			get {
				return "<span underline=\"single\">{0}</span>";
			}
		}
		
		public bool RenderDescriptionText {
			get {
				return true;
			}
		}
	}
}
