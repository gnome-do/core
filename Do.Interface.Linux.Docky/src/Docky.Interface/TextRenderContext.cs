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

using Cairo;
using Gdk;
using Pango;

namespace Docky.Interface
{
	
	
	public struct TextRenderContext
	{
		public Pango.Alignment Alignment { get; set; }
		
		public Cairo.Context Context { get; set; }
		
		public Pango.EllipsizeMode EllipsizeMode { get; set; }
		
		public Gdk.Point LeftCenteredPoint { get; set; }
		
		public int MaximumWidth { get; set; }
		
		public string Text { get; set; }
		
		public Pango.WrapMode WrapMode { get; set; }
		
		public TextRenderContext (Cairo.Context cr, string text, int width)
		{
			Context = cr;
			Text = text;
			Alignment = Alignment.Left;
			MaximumWidth = width;
			WrapMode = WrapMode.WordChar;
			EllipsizeMode = EllipsizeMode.End;
		}
	}
}
