// LBIconBox.cs created with MonoDevelop
// User: dave at 1:10 PM 8/25/2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using Gtk;
using Gdk;

using Do.Core;
using Do.PluginLib;

namespace Do.UI
{

	public class IconBox : RoundedFrame
	{
		const string captionFormat = "{0}";
		const string highlightFormat = "<span weight=\"heavy\" background=\"#353045\">{0}</span>";
		
		protected bool isFocused;
		
		protected string caption;
		protected Pixbuf pixbuf, emptyPixbuf;
		protected int iconSize;
		
		protected VBox vbox;
		protected Gtk.Image image;
		protected Label label;
		
		protected double focusedTransparency = 0.4;
		protected double unfocusedTransparency = 0.1;
		
		public IconBox (int iconSize) : base ()
		{
			this.iconSize = iconSize;

			Build ();
		}
		
		protected virtual void Build ()
		{
			caption = "";
			pixbuf = emptyPixbuf;
			
			vbox = new VBox (false, 12);
			vbox.BorderWidth = 6;
			Add (vbox);
			vbox.Show ();
			
			emptyPixbuf = new Pixbuf (Colorspace.Rgb, true, 8, iconSize, iconSize);
			emptyPixbuf.Fill (uint.MinValue);
			
			image = new Gtk.Image ();
			vbox.PackStart (image, false, false, 0);
			image.Show ();
			
			label = new Label ();
			label.Ellipsize = Pango.EllipsizeMode.End;
			label.ModifyFg (StateType.Normal, Style.White);
			vbox.PackStart (label, false, false, 0);
			label.Show ();
			
			image.SetSizeRequest (iconSize, iconSize);
			label.SetSizeRequest (iconSize / 4 * 5, -1);
			// SetSizeRequest (iconSize * 2, iconSize * 2);
			
			DrawFrame = false;			
			DrawFill = true;
			FillColor = new Color (byte.MaxValue, byte.MaxValue, byte.MaxValue);
			
			Realized += OnRealized;
			UpdateFocus ();
		}
		
		public virtual void Clear () {
			Pixbuf = emptyPixbuf;
			Caption = "";
		}
		
		protected virtual void OnRealized (object o, EventArgs args)
		{
			UpdateFocus ();
		}
		
		public bool IsFocused {
			get { return isFocused; }
			set {
				isFocused = value;
				UpdateFocus ();
			}
		}
		
		public string Caption {
			get { return caption; }
			set {
				caption = (value == null ? "" : value);
				label.Markup = string.Format (captionFormat, caption);				
			}
		}
		
		public string Icon {
			set {
				Pixbuf = Util.Appearance.PixbufFromIconName (value, iconSize);
			}
		}
		
		public Pixbuf Pixbuf {
			get { return pixbuf; }
			set {
				pixbuf = (value == null ? emptyPixbuf : value);
				image.Pixbuf = pixbuf;
			}
		}
		
		public IObject DisplayObject {
			set {
				string name, icon;
				
				icon = null;
				name = "";
				if (value != null) {
					icon = value.Icon;
					name = value.Name;
				}
				Icon = icon;
				Caption = name;
			}
		}
		
		public string Highlight {
			set {
				string highlight;
				
				if (value != null) {
					highlight = Util.FormatCommonSubstrings (caption, value, highlightFormat);
				} else {
					highlight = caption;
				}
				Caption = highlight;
			}
		}
		
		protected virtual void UpdateFocus ()
		{
			FillAlpha = (isFocused ? focusedTransparency : unfocusedTransparency);
		}
		
	}
}
