//
// SearchEntry.cs
//
// Author:
//   Aaron Bockover <abockover@novell.com>
//   Gabriel Burt <gburt@novell.com>
//
// Copyright (C) 2007 Novell, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
 
using System;
using Gtk;

namespace Do.UI
{
    public class SearchEntry : EventBox
    {
        private HBox box;
        private Entry entry;
        private HoverImageButton clear_button;

        private uint changed_timeout_id = 0;
        
        private string empty_message;
        private bool ready = false;

        private event EventHandler entry_changed;

        public event EventHandler Changed {
            add { entry_changed += value; }
            remove { entry_changed -= value; }
        }

        public event EventHandler Activated {
            add { entry.Activated += value; }
            remove { entry.Activated -= value; }
        }

        public SearchEntry()
        {
            AppPaintable = true;

            BuildWidget();
            
            NoShowAll = true;
        }
            
        private void BuildWidget()
        {
            box = new HBox();
            entry = new FramelessEntry(this);
			clear_button = new HoverImageButton(IconSize.Menu, new string [] { "edit-clear", Stock.Clear });

			box.PackStart(entry, true, true, 0);
            box.PackStart(clear_button, false, false, 0);

            Add(box);
            box.ShowAll();

            entry.StyleSet += OnInnerEntryStyleSet;
            entry.StateChanged += OnInnerEntryStateChanged;
            entry.FocusInEvent += OnInnerEntryFocusEvent;
            entry.FocusOutEvent += OnInnerEntryFocusEvent;
            entry.Changed += OnInnerEntryChanged;

            clear_button.Image.Xpad = 2;
            clear_button.CanFocus = false;

            clear_button.ButtonReleaseEvent += OnButtonReleaseEvent;
            clear_button.Clicked += OnClearButtonClicked;

            clear_button.Visible = false;
        }

        private void ShowHideButtons()
        {
            clear_button.Visible = entry.Text.Length > 0;
        }

        private void OnInnerEntryChanged(object o, EventArgs args)
        {
            ShowHideButtons();

            if(changed_timeout_id > 0) {
                GLib.Source.Remove(changed_timeout_id);
            }

            if (Ready)
                changed_timeout_id = GLib.Timeout.Add(25, OnChangedTimeout);
        }

        private bool OnChangedTimeout()
        {
            OnChanged();
            return false;
        }

        private void UpdateStyle ()
        {
            Gdk.Color color = entry.Style.Base (entry.State);
            clear_button.ModifyBg (entry.State, color);
            
            box.BorderWidth = (uint)entry.Style.XThickness;
        }
        
        private void OnInnerEntryStyleSet (object o, StyleSetArgs args)
        {
            UpdateStyle ();
        }
        
        private void OnInnerEntryStateChanged (object o, EventArgs args)
        {
            UpdateStyle ();
        }
        
        private void OnInnerEntryFocusEvent(object o, EventArgs args)
        {
            QueueDraw();
        }

        private void OnButtonReleaseEvent(object o, ButtonReleaseEventArgs args)
        {
            if(args.Event.Button != 1) {
                return;
            }

            entry.HasFocus = true;
        }

        private void OnClearButtonClicked(object o, EventArgs args)
        {
            entry.Text = String.Empty;
        }

        protected override bool OnExposeEvent(Gdk.EventExpose evnt)
        {
            PropagateExpose(Child, evnt);
            Style.PaintShadow(entry.Style, GdkWindow, StateType.Normal, 
                ShadowType.In, evnt.Area, entry, "entry",
                0, 0, Allocation.Width, Allocation.Height); 
            return true;
        }

        protected override void OnShown()
        {
            base.OnShown();
            ShowHideButtons();
        }

        protected virtual void OnChanged()
        {
            if(!Ready) {
                return;
            }

            EventHandler handler = entry_changed;
            if(handler != null) {
                handler(this, EventArgs.Empty);
            }
        }

        public string EmptyMessage {
            get { return empty_message; }
            set {
                empty_message = value;
                entry.QueueDraw();
            }
        }

        public string Query {
            get { return entry.Text.Trim(); }
            set { entry.Text = value.Trim(); }
        }

        public bool IsQueryAvailable {
            get { return Query != null && Query != String.Empty; }
        }

        public bool Ready {
            get { return ready; }
            set { ready = value; }
        }
        
        public new bool HasFocus {
            get { return entry.HasFocus; }
            set { entry.HasFocus = true; }
        }

        
        public Entry InnerEntry {
            get { return entry; }
        }

        private class FramelessEntry : Entry
        {
            private Gdk.Window text_window;
            private SearchEntry parent;
            private Pango.Layout layout;
            private Gdk.GC text_gc;

            public FramelessEntry(SearchEntry parent) : base()
            {
                this.parent = parent;
                HasFrame = false;
                
                layout = new Pango.Layout(PangoContext);
                layout.FontDescription = PangoContext.FontDescription.Copy();

                parent.StyleSet += OnParentStyleSet;
                WidthChars = 1;
            }

            private void OnParentStyleSet(object o, EventArgs args)
            {
                RefreshGC();
                QueueDraw();
            }

            private void RefreshGC()
            {
                if(text_window == null) {
                    return;
                }

                text_gc = new Gdk.GC(text_window);
                text_gc.Copy(Style.TextGC(StateType.Normal));
                //Gdk.Color color_a = parent.Style.Base(StateType.Normal);
                //Gdk.Color color_b = parent.Style.Text(StateType.Normal);
				text_gc.RgbFgColor = new Gdk.Color (0,0,0);
                //text_gc.RgbFgColor = Hyena.Gui.GtkUtilities.ColorBlend(color_a, color_b);
            }
			
            protected override bool OnExposeEvent(Gdk.EventExpose evnt)
            {
                // The Entry's GdkWindow is the top level window onto which
                // the frame is drawn; the actual text entry is drawn into a
                // separate window, so we can ensure that for themes that don't
                // respect HasFrame, we never ever allow the base frame drawing
                // to happen
                if(evnt.Window == GdkWindow) {
                    return true;
                }

                bool ret = base.OnExposeEvent(evnt);

                if(text_gc == null || evnt.Window != text_window) {
                    text_window = evnt.Window;
                    RefreshGC();
                }

                if(Text.Length > 0 || HasFocus || parent.EmptyMessage == null) {
                    return ret;
                }

                int width, height;
                layout.SetMarkup(parent.EmptyMessage);
                layout.GetPixelSize(out width, out height);
                evnt.Window.DrawLayout(text_gc, 2, (SizeRequest().Height - height) / 2, layout);

                return ret;
            }
        }
    }
}
