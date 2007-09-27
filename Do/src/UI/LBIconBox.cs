// LBIconBox.cs created with MonoDevelop
// User: dave at 1:10 PM 8/25/2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using Gtk;
using Gdk;

using Do.Core;

namespace Do.UI
{

	public class LBIconBox : LBFrame
	{
		const string captionFormat = "<small>{0}</small>";
		
		protected bool isFocused;
		protected bool transparent;
		
		protected string caption;
		protected Pixbuf pixbuf, empty_pixbuf;
		protected int icon_size;
		
		protected VBox vbox;
		protected Gtk.Image image;
		protected Label label;
		
		public LBIconBox(int icon_size) : base ()
		{
			this.icon_size = icon_size;
			caption = "";
			pixbuf = empty_pixbuf;
			
			Build ();
		}
		
		protected virtual void Build ()
		{
			vbox = new VBox (false, 12);
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
			vbox.PackStart (label, false, false, 0);
			label.Show ();
			
			image.SetSizeRequest (icon_size, icon_size);
			// TODO: change 12 to -1
			label.SetSizeRequest (icon_size / 4 * 5, 12);
			// SetSizeRequest (icon_size * 2, icon_size * 2);
			
			Realized += OnRealized;
			UpdateHighlight ();
		}
		
		public virtual void Clear () {
			Pixbuf = empty_pixbuf;
			Caption = "";
		}
		
		protected virtual void OnRealized (object o, EventArgs args)
		{
			UpdateHighlight ();
		}
		
		public bool IsFocused {
			get { return isFocused; }
			set {
				isFocused = value;
				// IsFocus = isFocused;
				UpdateHighlight ();
			}
		}
		
		public bool Transparent {
			get { return transparent; }
			set {
				SetTransparent (value);
			}
		}
		
		public string Caption {
			get { return caption; }
			set {
				caption = value;
				label.Markup = string.Format (captionFormat, caption);				
			}
		}
		
		public Pixbuf Pixbuf {
			get { return pixbuf; }
			set {
				pixbuf = value;
				image.Pixbuf = pixbuf;
			}
		}
		
		protected virtual void UpdateHighlight ()
		{
			if (transparent) {
				FillAlpha = (ushort) ((isFocused ? 0.6 : 0.2) * ushort.MaxValue);
			} else {
				FillColor = Style.Base (isFocused ? StateType.Selected : StateType.Normal);
				Fill = true;
			}
		}
		
		protected virtual void SetTransparent (bool transparent)
		{
			this.transparent = transparent;
			if (transparent) {
				Frame = false;
				FillColor = new Color (byte.MaxValue, byte.MaxValue, byte.MaxValue);
				Fill = true;
				label.ModifyFg (StateType.Normal, Style.White);
			} else {
				FrameColor = Style.Foreground (StateType.Selected);
				label.ModifyFg (StateType.Normal, Style.Foreground (StateType.Normal));
			}
			UpdateHighlight ();
		}
	}
}
