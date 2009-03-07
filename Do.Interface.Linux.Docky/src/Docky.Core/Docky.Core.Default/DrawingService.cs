// DrawingService.cs
// 
// Copyright (C) 2009 GNOME Do
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

using Docky.Interface;

namespace Docky.Core.Default
{
	
	public class DrawingService  : IDrawingService
	{
		#region IDrawingService implementation 
		
		Pango.Layout GetThemedLayout ()
		{
			Pango.Layout layout = new Pango.Layout (DockWindow.Window.CreatePangoContext ());
			layout.FontDescription = DockWindow.Window.Style.FontDescription;
			return layout;
		}
		
		public Gdk.Rectangle TextPathAtPoint (TextRenderContext context)
		{
			Cairo.Context cr = context.Context;
			Gdk.Point point = context.LeftCenteredPoint;
			
			Pango.Layout layout = GetThemedLayout ();
			layout.Width = Pango.Units.FromPixels (context.MaximumWidth);
			layout.SetMarkup (context.Text);
			layout.Ellipsize = context.EllipsizeMode;
			layout.Alignment = context.Alignment;
			layout.Wrap = context.WrapMode;
			
			if (context.FontSize != 0)
				layout.FontDescription.Size = Pango.Units.FromPixels (context.FontSize);
			
			Pango.Rectangle rect1, rect2;
			layout.GetExtents (out rect1, out rect2);
			
			int transY = point.Y - Pango.Units.ToPixels (rect2.Height) / 2;
			cr.Translate (point.X, transY);
			Pango.CairoHelper.LayoutPath (cr, layout);
			cr.Translate (0 - point.X, 0 - transY);
			
			Gdk.Rectangle textArea = new Gdk.Rectangle (Pango.Units.ToPixels (rect2.X),
			                                            Pango.Units.ToPixels (rect2.Y),
			                                            Pango.Units.ToPixels (rect2.Width),
			                                            Pango.Units.ToPixels (rect2.Height));
			return textArea;
		}
		
		#endregion 

		#region IDisposable implementation 
		
		public void Dispose ()
		{
		}
		
		#endregion 
		
	}
}
