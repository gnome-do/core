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
		
		protected bool isFocused;
		protected bool transparent;
		
		protected string caption;
		protected Pixbuf pixbuf;
		
		protected VBox vbox;
		protected Gtk.Image image;
		protected Label label;
		
		public LBIconBox(string caption, Pixbuf pixbuf) : base ()
		{
			Build ();
			this.caption = caption;
			this.pixbuf = pixbuf;
		}
		
		protected virtual void Build ()
		{
			vbox = new VBox (false, 12);
			vbox.BorderWidth = 6;
			Add (vbox);
			vbox.Show ();
			
			image = new Gtk.Image ();
			vbox.PackStart (image, false, false, 0);
			image.Show ();
			
			label = new Label ();
			label.Ellipsize = Pango.EllipsizeMode.End;
			vbox.PackStart (label, false, false, 0);
			label.Show ();
			
			image.SetSizeRequest (Util.DefaultIconSize + 2, Util.DefaultIconSize + 2);
			label.SetSizeRequest (Util.DefaultIconSize * 2, -1);
			SetSizeRequest (Util.DefaultIconSize * 3, Util.DefaultIconSize * 2);
			
			Realized += OnRealized;
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
				label.Markup = string.Format ("<b>{0}</b>", caption);				
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
