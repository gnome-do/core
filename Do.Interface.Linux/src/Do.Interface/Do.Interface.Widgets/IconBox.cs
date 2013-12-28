/* IconBox.cs
 *
 * GNOME Do is the legal property of its developers. Please refer to the
 * COPYRIGHT file distributed with this source distribution.
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;

using Gtk;
using Gdk;

using Do.Universe;
using Do.Platform.Linux;
using Do.Interface;

namespace Do.Interface.Widgets
{
	public class IconBox : Frame
	{
		const string CaptionFormat = "{0}";
		const string HighlightFormat = "<span weight=\"bold\" underline=\"single\">{0}</span>";

		protected bool focused, textOverlay;

		protected string caption, icon_name, highlight;
		protected Pixbuf empty_pixbuf;
		protected int icon_size;
		
		protected VBox vbox;
		protected Gtk.Image image;
		protected Label label;
		protected Gdk.Pixbuf overlay_pixbuf;

		protected float focused_fill_transparency = 0.4f;
		protected float unfocused_fill_transparency = 0.1f;
		protected float focused_frame_transparency = 0.3f;
		protected float unfocused_frame_transparency = 0.075f;

		public IconBox (int icon_size) : base ()
		{
			this.icon_size = icon_size;
			overlay_pixbuf = IconProvider.PixbufFromIconName ("gnome-mime-text", icon_size);
			Build ();
		}
		
		protected virtual void Build ()
		{
			Alignment label_align;

			caption = "";
			highlight = "";
			
			vbox = new VBox (false, 4);
			vbox.BorderWidth = 6;
			Add (vbox);
			
			vbox.Show ();

			empty_pixbuf = new Pixbuf (Colorspace.Rgb, true, 8, icon_size, icon_size);
			empty_pixbuf.Fill (uint.MinValue);

			image = new Gtk.Image ();
			vbox.PackStart (image, false, false, 0);
			image.Show ();

			label = new Label ();
			label.Ellipsize = Pango.EllipsizeMode.End;
			label.ModifyFg (StateType.Normal, Style.White);
			label_align = new Alignment (1.0F, 0.0F, 0, 0);
			label_align.SetPadding (0, 2, 2, 2);
			label_align.Add (label);
			vbox.PackStart (label_align, false, false, 0);
			label.Show ();
			label_align.Show ();

			image.SetSizeRequest (icon_size, icon_size);
			label.SetSizeRequest (icon_size / 4 * 5, -1);
			// SetSizeRequest (icon_size * 2, icon_size * 2);

			DrawFrame = DrawFill = true;
			FrameColor = FillColor = new Color (byte.MaxValue, byte.MaxValue, byte.MaxValue);

			Realized += OnRealized;
			UpdateFocus ();
		}

		public virtual void Clear ()
		{
			Pixbuf = null;
			highlight = "";
			Caption = "";
			icon_name = "";
			TextOverlay = false;
		}

		protected virtual void OnRealized (object o, EventArgs args)
		{
			UpdateFocus ();
		}

		public bool IsFocused
		{
			get { return focused; }
			set {
				if (focused == value) return;
				focused = value;
				UpdateFocus ();
			}
		}
		
		public virtual bool TextOverlay
		{
			get { return textOverlay; }
			set {
				if (textOverlay == value)
					return;
				
				textOverlay = value;
				if (value) {
					FillAlpha = focused_fill_transparency;
					FrameAlpha = focused_frame_transparency;
					FillColor = FrameColor = new Color (0x00, 0x00, 0x00);
					image.Hide ();
					label.Ellipsize = Pango.EllipsizeMode.None;
					label.LineWrapMode = Pango.WrapMode.WordChar;
					label.LineWrap = true;
					highlight = "";
				} else {
					FillColor = FrameColor = new Color (0xff, 0xff, 0xff);
					image.Show ();
					label.Wrap = false;
					label.Ellipsize = Pango.EllipsizeMode.End;
				}
			}
		}

		public string Caption
		{
			get { return caption; }
			set {
				caption = GLib.Markup.EscapeText (value ?? "");
				caption = caption.Replace ("\n", " ");
				UpdateLabel ();
			}
		}
		
		private void UpdateLabel ()
		{
				int lines = label.Layout.LineCount;
				label.Markup = string.Format (CaptionFormat, 
				                              Util.FormatCommonSubstrings (caption, highlight, HighlightFormat));
				if (lines != label.Layout.LineCount && LinesChanged != null)
					LinesChanged (label.Layout.LineCount, new EventArgs ());
		}

		public string Icon
		{
			get {
				return icon_name;
			}
			set {
				if (value == null || textOverlay) return;
				icon_name = value;
				using (Gdk.Pixbuf pix = IconProvider.PixbufFromIconName (value, icon_size)) {
					Pixbuf = pix;
				}
			}
		}

		public Pixbuf Pixbuf
		{
			set {
				image.Pixbuf = value ?? empty_pixbuf;
			}
		}

		public Do.Universe.Item DisplayObject
		{
			set {
				string name, icon;

				icon = null;
				name = null;
				if (value != null) {
					icon = value.Icon;
					name = value.Name;
					
					if (name == Caption && icon == this.Icon)
						return;
				}
				
				Icon = icon;
				Caption = name;
			}
		}

		public string Highlight
		{
			set {
				highlight = value ?? "";
				UpdateLabel ();
			}
		}

		protected virtual void UpdateFocus ()
		{
			FillAlpha = focused ? focused_fill_transparency : unfocused_fill_transparency;
			FrameAlpha = focused ? focused_frame_transparency : unfocused_frame_transparency;
		}
		
		protected override void PaintFill ()
		{
			if (!textOverlay) {
				base.PaintFill ();
				return;
			}
			// Gtk doesn't allow stacking elements, so we can't use a standard GTK interface here
			// To work around this we will instead draw our own icon and gtk will be none the wiser.
			// We are very clever.
			
			double r, g, b;
			
			r = (double) fillColor.Red / ushort.MaxValue;
			g = (double) fillColor.Green / ushort.MaxValue;
			b = (double) fillColor.Blue / ushort.MaxValue;
			
			cairo.Save ();
			GetFrame (cairo);
			
			Gdk.CairoHelper.SetSourcePixbuf (cairo, 
			                                 overlay_pixbuf, 
			                                 (int) (width / 2) - (int) (overlay_pixbuf.Width / 2) + x, 
			                                 (int) (height / 2) - (int) (overlay_pixbuf.Height / 2) + y);
			cairo.PaintWithAlpha (fillAlpha);
			
			cairo.SetSourceRGBA (r, g, b, fillAlpha);
			cairo.FillPreserve ();
			
			cairo.Restore ();
		}

		public event EventHandler LinesChanged;
	}
}
