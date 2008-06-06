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

using Do.Addins;
using Do.Universe;

namespace Do.UI
{
	public class IconBox : Frame
	{
		const string CaptionFormat = "{0}";
		const string HighlightFormat = "<span weight=\"bold\" underline=\"single\">{0}</span>";

		protected bool focused;

		protected string caption, icon_name;
		protected Pixbuf pixbuf, empty_pixbuf;
		protected int icon_size;

		protected VBox vbox;
		protected Gtk.Image image;
		protected Label label;

		protected float focused_transparency = 0.4f;
		protected float unfocused_transparency = 0.1f;

		public IconBox (int icon_size) : base ()
		{
			this.icon_size = icon_size;
			Build ();
		}
		
		protected virtual void Build ()
		{
			Alignment label_align;

			caption = "";
			pixbuf = empty_pixbuf;

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
			Pixbuf = empty_pixbuf;
			Caption = "";
		}

		protected virtual void OnRealized (object o, EventArgs args)
		{
			UpdateFocus ();
		}

		public bool IsFocused
		{
			get { return focused; }
			set {
				focused = value;
				UpdateFocus ();
			}
		}

		public string Caption
		{
			get { return caption; }
			set {
				caption = value ?? "";
				caption = caption.Replace ("\n", " ");
				label.Markup = string.Format (CaptionFormat, Util.Appearance.MarkupSafeString (caption));
			}
		}

		public string Icon
		{
			set {
				icon_name = value;
				Pixbuf = IconProvider.PixbufFromIconName (value, icon_size);
			}
		}

		public Pixbuf Pixbuf
		{
			get { return pixbuf; }
			set {
				pixbuf = value ?? empty_pixbuf;
				image.Pixbuf = pixbuf;
			}
		}

		public IObject DisplayObject
		{
			set {
				string name, icon;

				icon = null;
				name = null;
				if (value != null) {
					icon = value.Icon;
					name = value.Name;
				}				
				Icon = icon;
				Caption = name;
			}
		}

		public string Highlight
		{
			set {
				string highlight;

				if (value != null) {
					highlight = Util.FormatCommonSubstrings (caption, value, HighlightFormat);
				} else {
					highlight = caption;
				}
				Caption = highlight;
			}
		}

		protected virtual void UpdateFocus ()
		{
			FrameAlpha = FillAlpha = (focused ? focused_transparency : unfocused_transparency);
		}

	}
}
