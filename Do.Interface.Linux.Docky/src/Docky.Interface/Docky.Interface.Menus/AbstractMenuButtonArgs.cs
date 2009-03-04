// AbstractMenuButtonArgs.cs
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

using Cairo;
using Gdk;
using Gtk;
using Mono.Unix;

using Docky.Core;
using Docky.Utilities;

using Do.Interface;
using Do.Interface.CairoUtils;
using Do.Platform;

namespace Docky.Interface.Menus
{
	public abstract class AbstractMenuButtonArgs : AbstractMenuArgs
	{
		const string FormatString = "<b>{0}</b>";
		const int WidthBuffer = 4;
		const int Height = 22;
		
		Gtk.Widget widget;
		bool hovered;
		
		public bool Dark { get; set; }
		
		public virtual double IconOpacity {
			get { return 1; }
		}
		
		public override Gtk.Widget Widget { 
			get {
				if (widget == null)
					widget = BuildWidget ();
				return widget;
			}
		}
		
		protected virtual string Description { get; set; }
		
		protected virtual string Icon { get; set; }
		
		public AbstractMenuButtonArgs ()
		{
			
		}
		
		public AbstractMenuButtonArgs (string description, string icon)
		{
			Description = GLib.Markup.EscapeText (Catalog.GetString (description));
			Icon = icon;
		}
		
		Widget BuildWidget ()
		{
			DrawingArea button = new DrawingArea ();
			
			button.ExposeEvent += HandleExposeEvent;
			button.EnterNotifyEvent += HandleEnterNotifyEvent;
			button.LeaveNotifyEvent += HandleLeaveNotifyEvent; 
			button.ButtonReleaseEvent += HandleButtonReleaseEvent; 
			
			button.AddEvents ((int) (EventMask.EnterNotifyMask | EventMask.LeaveNotifyMask | EventMask.ButtonReleaseMask));
			button.HeightRequest = Height;

			button.SetCompositeColormap ();
			
			return button;
		}
		
		void HandleButtonReleaseEvent(object o, ButtonReleaseEventArgs args)
		{
			Action ();
			base.OnActivated ();
		}

		void HandleLeaveNotifyEvent(object o, LeaveNotifyEventArgs args)
		{
			(o as Widget).QueueDraw ();
			hovered = false;
		}

		void HandleEnterNotifyEvent(object o, EnterNotifyEventArgs args)
		{
			(o as Widget).QueueDraw ();
			hovered = true;
		}

		void HandleExposeEvent(object o, ExposeEventArgs args)
		{
			using (Context cr = CairoHelper.Create (args.Event.Window)) {
				Gdk.Rectangle area = args.Event.Area;
				cr.AlphaFill ();
				LinearGradient lg = new LinearGradient (area.X, area.Y, area.X, area.Y + area.Height);
				
				Cairo.Color background = (Dark) ? DockPopupMenu.BackgroundColor.ShadeColor (.7) : DockPopupMenu.BackgroundColor;
				if (hovered) {
					Cairo.Color high = background
						    .ConvertToGdk ()
							.SetMinimumValue (25)
							.ConvertToCairo (background.A);
					
					lg.AddColorStop (0, high);
					lg.AddColorStop (1, background);
				} else {
					lg.AddColorStop (0, background);
					lg.AddColorStop (1, background);
				}
				cr.Pattern = lg;
				cr.Paint ();
				lg.Destroy ();
				
				Gdk.Point textPoint;
				int width;
				textPoint = new Gdk.Point (area.X + WidthBuffer + 25, area.Y + area.Height / 2);
				width = area.Width - WidthBuffer * 2 - 25;
				
				DockServices.DrawingService.TextPathAtPoint (cr, 
				                                             string.Format (FormatString, Description), 
				                                             textPoint,
				                                             width,
				                                             Pango.Alignment.Left);
				cr.Color = new Cairo.Color (1, 1, 1);
				cr.Fill ();
				
				Gdk.Pixbuf pbuf = GetPixbuf (Height - 8);
				CairoHelper.SetSourcePixbuf (cr, pbuf, WidthBuffer, (Height - pbuf.Height) / 2);
				cr.PaintWithAlpha (IconOpacity);
				pbuf.Dispose ();
			}
		}
		
		protected virtual Gdk.Pixbuf GetPixbuf (int size)
		{
			return IconProvider.PixbufFromIconName (Icon, size);
		}
		
		public abstract void Action ();
		
		public AbstractMenuButtonArgs AsDark ()
		{
			Dark = true;
			return this;
		}
		
		public override void Dispose ()
		{
			Widget.Destroy ();
			base.Dispose ();
		}

	}
}
