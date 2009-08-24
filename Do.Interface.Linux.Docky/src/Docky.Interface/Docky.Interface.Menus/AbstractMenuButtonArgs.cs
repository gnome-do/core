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
		const string FormatString = "{0}";
		const int WidthBuffer = 4;
		const int Height = 22;
		
		Gtk.Widget widget;
		bool hovered, tooltip;
		string description, icon;
		
		public bool Dark { get; set; }
		public bool Disabled { get; set; }
		
		public virtual double IconOpacity {
			get { return 1; }
		}
		
		public override Gtk.Widget Widget {
			get { return widget; } 
		}
		
		private Gdk.Pixbuf Pixbuf { get; set; }
		
		protected string Description { 
			get {
				return description;
			}
			set {
				description = value;
				BuildWidget ();
			}
		}
		
		protected string Icon { 
			get {
				return icon;
			}
			set {
				icon = value;
				BuildWidget ();
			}
		}
		
		protected bool UseTooltip {
			get {
				return tooltip;
			}
			set {
				
				tooltip = value;
				BuildWidget ();
			}
		}
		
		public AbstractMenuButtonArgs ()
		{
		}
		
		public AbstractMenuButtonArgs (string description, string icon)
		{
			this.description = GLib.Markup.EscapeText (Catalog.GetString (description));
			this.icon = icon;
			
			BuildWidget ();
		}
		
		void BuildWidget ()
		{
			if (widget != null)
				widget.Destroy ();
			
			DrawingArea button = new DrawingArea ();
			
			button.ExposeEvent += HandleExposeEvent;
			button.EnterNotifyEvent += HandleEnterNotifyEvent;
			button.LeaveNotifyEvent += HandleLeaveNotifyEvent; 
			button.ButtonReleaseEvent += HandleButtonReleaseEvent; 
			
			button.AddEvents ((int) (EventMask.EnterNotifyMask | EventMask.LeaveNotifyMask | EventMask.ButtonReleaseMask));
			button.HeightRequest = Height;

			button.SetCompositeColormap ();
			if (UseTooltip)
				button.TooltipText = Description;
			
			widget = button;
			
			if (Pixbuf != null)
				Pixbuf.Dispose ();
			
			Pixbuf = GetPixbuf (Height - 8);
		}
		
		void HandleButtonReleaseEvent(object o, ButtonReleaseEventArgs args)
		{
			if (!Disabled) {
				Action ();
				base.OnActivated ();
			}
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
				if (hovered && !Disabled) {
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
				
				int width = area.Width - WidthBuffer * 2 - 25;
				
				if (!string.IsNullOrEmpty (Description)) {
					TextRenderContext renderContext = new TextRenderContext (cr, string.Format (FormatString, Description), width);
					
					renderContext.LeftCenteredPoint = new Gdk.Point (area.X + WidthBuffer + 25, area.Y + area.Height / 2);
					renderContext.Alignment = Pango.Alignment.Left;
					renderContext.EllipsizeMode = Pango.EllipsizeMode.End;
					
					DockServices.DrawingService.TextPathAtPoint (renderContext);
					
					cr.Color = new Cairo.Color (1, 1, 1, Disabled ? 0.5 : 1);
					cr.Fill ();
				}
				
				if (Pixbuf != null) {
					CairoHelper.SetSourcePixbuf (cr, Pixbuf, WidthBuffer, (Height - Pixbuf.Height) / 2);
					cr.PaintWithAlpha (Disabled ? IconOpacity / 2 : IconOpacity);
				}
			}
		}
		
		protected virtual Gdk.Pixbuf GetPixbuf (int size)
		{
			if (!string.IsNullOrEmpty (Icon))
				return IconProvider.PixbufFromIconName (Icon, size);
			return null;
		}
		
		public abstract void Action ();
		
		public AbstractMenuButtonArgs AsDisabled ()
		{
			Disabled = true;
			return this;
		}
		
		public AbstractMenuButtonArgs AsDark ()
		{
			Dark = true;
			return this;
		}
		
		public override void Dispose ()
		{
			if (Widget != null)
				Widget.Destroy ();
			if (Pixbuf != null)
				Pixbuf.Dispose ();
			base.Dispose ();
		}

	}
}
