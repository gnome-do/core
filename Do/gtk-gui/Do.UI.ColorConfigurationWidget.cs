// ------------------------------------------------------------------------------
//  <autogenerated>
//      This code was generated by a tool.
//      Mono Runtime Version: 2.0.50727.42
// 
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
//  </autogenerated>
// ------------------------------------------------------------------------------

namespace Do.UI {
    
    
    public partial class ColorConfigurationWidget {
        
        private Gtk.VBox vbox2;
        
        private Gtk.Frame frame1;
        
        private Gtk.Alignment GtkAlignment;
        
        private Gtk.Alignment alignment2;
        
        private Gtk.VBox vbox1;
        
        private Gtk.HBox hbox2;
        
        private Gtk.Label theme_lbl;
        
        private Gtk.ComboBox theme_combo;
        
        private Gtk.CheckButton pin_check;
        
        private Gtk.VBox vbox3;
        
        private Gtk.Table table2;
        
        private Gtk.ComboBox background_combo;
        
        private Gtk.Button clear_radius;
        
        private Gtk.Label label4;
        
        private Gtk.Label label5;
        
        private Gtk.Label label6;
        
        private Gtk.Label label7;
        
        private Gtk.ComboBox outline_combo;
        
        private Gtk.SpinButton radius_spin;
        
        private Gtk.CheckButton shadow_check;
        
        private Gtk.ComboBox title_combo;
        
        private Gtk.HBox hbox1;
        
        private Gtk.Label label8;
        
        private Gtk.ColorButton background_colorbutton;
        
        private Gtk.Button clear_background;
        
        private Gtk.VBox vbox4;
        
        private Gtk.Frame preview_frame;
        
        private Gtk.Alignment preview_align;
        
        private Gtk.Label GtkLabel4;
        
        private Gtk.Label GtkLabel5;
        
        protected virtual void Build() {
            Stetic.Gui.Initialize(this);
            // Widget Do.UI.ColorConfigurationWidget
            Stetic.BinContainer.Attach(this);
            this.Name = "Do.UI.ColorConfigurationWidget";
            // Container child Do.UI.ColorConfigurationWidget.Gtk.Container+ContainerChild
            this.vbox2 = new Gtk.VBox();
            this.vbox2.Name = "vbox2";
            this.vbox2.Spacing = 6;
            // Container child vbox2.Gtk.Box+BoxChild
            this.frame1 = new Gtk.Frame();
            this.frame1.Name = "frame1";
            this.frame1.ShadowType = ((Gtk.ShadowType)(0));
            // Container child frame1.Gtk.Container+ContainerChild
            this.GtkAlignment = new Gtk.Alignment(0F, 0F, 1F, 1F);
            this.GtkAlignment.Name = "GtkAlignment";
            this.GtkAlignment.LeftPadding = ((uint)(25));
            this.GtkAlignment.TopPadding = ((uint)(5));
            this.GtkAlignment.RightPadding = ((uint)(5));
            this.GtkAlignment.BottomPadding = ((uint)(10));
            // Container child GtkAlignment.Gtk.Container+ContainerChild
            this.alignment2 = new Gtk.Alignment(0.5F, 0.5F, 1F, 1F);
            this.alignment2.Name = "alignment2";
            // Container child alignment2.Gtk.Container+ContainerChild
            this.vbox1 = new Gtk.VBox();
            this.vbox1.Name = "vbox1";
            this.vbox1.Spacing = 6;
            // Container child vbox1.Gtk.Box+BoxChild
            this.hbox2 = new Gtk.HBox();
            this.hbox2.Name = "hbox2";
            this.hbox2.Spacing = 6;
            // Container child hbox2.Gtk.Box+BoxChild
            this.theme_lbl = new Gtk.Label();
            this.theme_lbl.Name = "theme_lbl";
            this.theme_lbl.LabelProp = Mono.Unix.Catalog.GetString("_Theme:");
            this.theme_lbl.UseUnderline = true;
            this.hbox2.Add(this.theme_lbl);
            Gtk.Box.BoxChild w1 = ((Gtk.Box.BoxChild)(this.hbox2[this.theme_lbl]));
            w1.Position = 0;
            w1.Expand = false;
            w1.Fill = false;
            // Container child hbox2.Gtk.Box+BoxChild
            this.theme_combo = Gtk.ComboBox.NewText();
            this.theme_combo.AppendText(Mono.Unix.Catalog.GetString("Glass Frame"));
            this.theme_combo.AppendText(Mono.Unix.Catalog.GetString("Mini"));
            this.theme_combo.Name = "theme_combo";
            this.theme_combo.Active = 0;
            this.hbox2.Add(this.theme_combo);
            Gtk.Box.BoxChild w2 = ((Gtk.Box.BoxChild)(this.hbox2[this.theme_combo]));
            w2.Position = 1;
            w2.Expand = false;
            w2.Fill = false;
            this.vbox1.Add(this.hbox2);
            Gtk.Box.BoxChild w3 = ((Gtk.Box.BoxChild)(this.vbox1[this.hbox2]));
            w3.Position = 0;
            w3.Expand = false;
            w3.Fill = false;
            // Container child vbox1.Gtk.Box+BoxChild
            this.pin_check = new Gtk.CheckButton();
            this.pin_check.CanFocus = true;
            this.pin_check.Name = "pin_check";
            this.pin_check.Label = Mono.Unix.Catalog.GetString("Always show results window");
            this.pin_check.DrawIndicator = true;
            this.pin_check.UseUnderline = true;
            this.vbox1.Add(this.pin_check);
            Gtk.Box.BoxChild w4 = ((Gtk.Box.BoxChild)(this.vbox1[this.pin_check]));
            w4.Position = 1;
            w4.Expand = false;
            w4.Fill = false;
            // Container child vbox1.Gtk.Box+BoxChild
            this.vbox3 = new Gtk.VBox();
            this.vbox3.Name = "vbox3";
            this.vbox3.Spacing = 6;
            // Container child vbox3.Gtk.Box+BoxChild
            this.table2 = new Gtk.Table(((uint)(4)), ((uint)(7)), false);
            this.table2.Name = "table2";
            this.table2.BorderWidth = ((uint)(15));
            // Container child table2.Gtk.Table+TableChild
            this.background_combo = Gtk.ComboBox.NewText();
            this.background_combo.AppendText(Mono.Unix.Catalog.GetString("Default"));
            this.background_combo.AppendText(Mono.Unix.Catalog.GetString("HUD"));
            this.background_combo.AppendText(Mono.Unix.Catalog.GetString("Classic"));
            this.background_combo.Name = "background_combo";
            this.background_combo.Active = 0;
            this.table2.Add(this.background_combo);
            Gtk.Table.TableChild w5 = ((Gtk.Table.TableChild)(this.table2[this.background_combo]));
            w5.TopAttach = ((uint)(1));
            w5.BottomAttach = ((uint)(2));
            w5.LeftAttach = ((uint)(2));
            w5.RightAttach = ((uint)(3));
            w5.XOptions = ((Gtk.AttachOptions)(4));
            w5.YOptions = ((Gtk.AttachOptions)(4));
            // Container child table2.Gtk.Table+TableChild
            this.clear_radius = new Gtk.Button();
            this.clear_radius.CanFocus = true;
            this.clear_radius.Name = "clear_radius";
            this.clear_radius.UseStock = true;
            this.clear_radius.UseUnderline = true;
            this.clear_radius.Label = "gtk-clear";
            this.table2.Add(this.clear_radius);
            Gtk.Table.TableChild w6 = ((Gtk.Table.TableChild)(this.table2[this.clear_radius]));
            w6.LeftAttach = ((uint)(6));
            w6.RightAttach = ((uint)(7));
            w6.XOptions = ((Gtk.AttachOptions)(4));
            w6.YOptions = ((Gtk.AttachOptions)(4));
            // Container child table2.Gtk.Table+TableChild
            this.label4 = new Gtk.Label();
            this.label4.Name = "label4";
            this.label4.Xalign = 0F;
            this.label4.LabelProp = Mono.Unix.Catalog.GetString("Title Bar Style:");
            this.table2.Add(this.label4);
            Gtk.Table.TableChild w7 = ((Gtk.Table.TableChild)(this.table2[this.label4]));
            w7.LeftAttach = ((uint)(1));
            w7.RightAttach = ((uint)(2));
            w7.XOptions = ((Gtk.AttachOptions)(4));
            w7.YOptions = ((Gtk.AttachOptions)(4));
            // Container child table2.Gtk.Table+TableChild
            this.label5 = new Gtk.Label();
            this.label5.Name = "label5";
            this.label5.Xalign = 0F;
            this.label5.LabelProp = Mono.Unix.Catalog.GetString("Background Style:");
            this.table2.Add(this.label5);
            Gtk.Table.TableChild w8 = ((Gtk.Table.TableChild)(this.table2[this.label5]));
            w8.TopAttach = ((uint)(1));
            w8.BottomAttach = ((uint)(2));
            w8.LeftAttach = ((uint)(1));
            w8.RightAttach = ((uint)(2));
            w8.XOptions = ((Gtk.AttachOptions)(4));
            w8.YOptions = ((Gtk.AttachOptions)(4));
            // Container child table2.Gtk.Table+TableChild
            this.label6 = new Gtk.Label();
            this.label6.Name = "label6";
            this.label6.Xalign = 0F;
            this.label6.LabelProp = Mono.Unix.Catalog.GetString("Outline Style:");
            this.table2.Add(this.label6);
            Gtk.Table.TableChild w9 = ((Gtk.Table.TableChild)(this.table2[this.label6]));
            w9.TopAttach = ((uint)(2));
            w9.BottomAttach = ((uint)(3));
            w9.LeftAttach = ((uint)(1));
            w9.RightAttach = ((uint)(2));
            w9.XOptions = ((Gtk.AttachOptions)(4));
            w9.YOptions = ((Gtk.AttachOptions)(4));
            // Container child table2.Gtk.Table+TableChild
            this.label7 = new Gtk.Label();
            this.label7.Name = "label7";
            this.label7.Xalign = 0F;
            this.label7.LabelProp = Mono.Unix.Catalog.GetString("Rounding Radius:");
            this.table2.Add(this.label7);
            Gtk.Table.TableChild w10 = ((Gtk.Table.TableChild)(this.table2[this.label7]));
            w10.LeftAttach = ((uint)(4));
            w10.RightAttach = ((uint)(5));
            w10.XOptions = ((Gtk.AttachOptions)(4));
            w10.YOptions = ((Gtk.AttachOptions)(4));
            // Container child table2.Gtk.Table+TableChild
            this.outline_combo = Gtk.ComboBox.NewText();
            this.outline_combo.AppendText(Mono.Unix.Catalog.GetString("Default"));
            this.outline_combo.AppendText(Mono.Unix.Catalog.GetString("HUD"));
            this.outline_combo.AppendText(Mono.Unix.Catalog.GetString("Classic"));
            this.outline_combo.Name = "outline_combo";
            this.outline_combo.Active = 0;
            this.table2.Add(this.outline_combo);
            Gtk.Table.TableChild w11 = ((Gtk.Table.TableChild)(this.table2[this.outline_combo]));
            w11.TopAttach = ((uint)(2));
            w11.BottomAttach = ((uint)(3));
            w11.LeftAttach = ((uint)(2));
            w11.RightAttach = ((uint)(3));
            w11.XOptions = ((Gtk.AttachOptions)(4));
            w11.YOptions = ((Gtk.AttachOptions)(4));
            // Container child table2.Gtk.Table+TableChild
            this.radius_spin = new Gtk.SpinButton(0, 25, 1);
            this.radius_spin.CanFocus = true;
            this.radius_spin.Name = "radius_spin";
            this.radius_spin.Adjustment.PageIncrement = 10;
            this.radius_spin.ClimbRate = 1;
            this.radius_spin.Numeric = true;
            this.radius_spin.Value = 6;
            this.table2.Add(this.radius_spin);
            Gtk.Table.TableChild w12 = ((Gtk.Table.TableChild)(this.table2[this.radius_spin]));
            w12.LeftAttach = ((uint)(5));
            w12.RightAttach = ((uint)(6));
            w12.XOptions = ((Gtk.AttachOptions)(4));
            w12.YOptions = ((Gtk.AttachOptions)(4));
            // Container child table2.Gtk.Table+TableChild
            this.shadow_check = new Gtk.CheckButton();
            this.shadow_check.CanFocus = true;
            this.shadow_check.Name = "shadow_check";
            this.shadow_check.Label = Mono.Unix.Catalog.GetString("Draw Shadow");
            this.shadow_check.DrawIndicator = true;
            this.shadow_check.UseUnderline = true;
            this.table2.Add(this.shadow_check);
            Gtk.Table.TableChild w13 = ((Gtk.Table.TableChild)(this.table2[this.shadow_check]));
            w13.TopAttach = ((uint)(3));
            w13.BottomAttach = ((uint)(4));
            w13.LeftAttach = ((uint)(1));
            w13.RightAttach = ((uint)(2));
            w13.XOptions = ((Gtk.AttachOptions)(4));
            w13.YOptions = ((Gtk.AttachOptions)(4));
            // Container child table2.Gtk.Table+TableChild
            this.title_combo = Gtk.ComboBox.NewText();
            this.title_combo.AppendText(Mono.Unix.Catalog.GetString("Default"));
            this.title_combo.AppendText(Mono.Unix.Catalog.GetString("HUD"));
            this.title_combo.AppendText(Mono.Unix.Catalog.GetString("Classic"));
            this.title_combo.Name = "title_combo";
            this.title_combo.Active = 0;
            this.table2.Add(this.title_combo);
            Gtk.Table.TableChild w14 = ((Gtk.Table.TableChild)(this.table2[this.title_combo]));
            w14.LeftAttach = ((uint)(2));
            w14.RightAttach = ((uint)(3));
            w14.XOptions = ((Gtk.AttachOptions)(4));
            w14.YOptions = ((Gtk.AttachOptions)(4));
            this.vbox3.Add(this.table2);
            Gtk.Box.BoxChild w15 = ((Gtk.Box.BoxChild)(this.vbox3[this.table2]));
            w15.Position = 0;
            w15.Expand = false;
            w15.Fill = false;
            // Container child vbox3.Gtk.Box+BoxChild
            this.hbox1 = new Gtk.HBox();
            this.hbox1.Name = "hbox1";
            this.hbox1.Spacing = 6;
            // Container child hbox1.Gtk.Box+BoxChild
            this.label8 = new Gtk.Label();
            this.label8.Name = "label8";
            this.label8.Xalign = 0F;
            this.label8.LabelProp = Mono.Unix.Catalog.GetString("Background Color:");
            this.hbox1.Add(this.label8);
            Gtk.Box.BoxChild w16 = ((Gtk.Box.BoxChild)(this.hbox1[this.label8]));
            w16.Position = 0;
            w16.Expand = false;
            w16.Fill = false;
            // Container child hbox1.Gtk.Box+BoxChild
            this.background_colorbutton = new Gtk.ColorButton();
            this.background_colorbutton.CanFocus = true;
            this.background_colorbutton.Events = ((Gdk.EventMask)(784));
            this.background_colorbutton.Name = "background_colorbutton";
            this.hbox1.Add(this.background_colorbutton);
            Gtk.Box.BoxChild w17 = ((Gtk.Box.BoxChild)(this.hbox1[this.background_colorbutton]));
            w17.Position = 1;
            w17.Expand = false;
            w17.Fill = false;
            // Container child hbox1.Gtk.Box+BoxChild
            this.clear_background = new Gtk.Button();
            this.clear_background.CanFocus = true;
            this.clear_background.Name = "clear_background";
            this.clear_background.UseUnderline = true;
            this.clear_background.Relief = ((Gtk.ReliefStyle)(2));
            // Container child clear_background.Gtk.Container+ContainerChild
            Gtk.Alignment w18 = new Gtk.Alignment(0.5F, 0.5F, 0F, 0F);
            // Container child GtkAlignment.Gtk.Container+ContainerChild
            Gtk.HBox w19 = new Gtk.HBox();
            w19.Spacing = 2;
            // Container child GtkHBox.Gtk.Container+ContainerChild
            Gtk.Image w20 = new Gtk.Image();
            w20.Pixbuf = Stetic.IconLoader.LoadIcon(this, "gtk-clear", Gtk.IconSize.Menu, 16);
            w19.Add(w20);
            // Container child GtkHBox.Gtk.Container+ContainerChild
            Gtk.Label w22 = new Gtk.Label();
            w22.LabelProp = Mono.Unix.Catalog.GetString("_Reset");
            w22.UseUnderline = true;
            w19.Add(w22);
            w18.Add(w19);
            this.clear_background.Add(w18);
            this.hbox1.Add(this.clear_background);
            Gtk.Box.BoxChild w26 = ((Gtk.Box.BoxChild)(this.hbox1[this.clear_background]));
            w26.Position = 2;
            w26.Expand = false;
            w26.Fill = false;
            this.vbox3.Add(this.hbox1);
            Gtk.Box.BoxChild w27 = ((Gtk.Box.BoxChild)(this.vbox3[this.hbox1]));
            w27.Position = 1;
            w27.Expand = false;
            w27.Fill = false;
            // Container child vbox3.Gtk.Box+BoxChild
            this.vbox4 = new Gtk.VBox();
            this.vbox4.Name = "vbox4";
            this.vbox4.Spacing = 6;
            // Container child vbox4.Gtk.Box+BoxChild
            this.preview_frame = new Gtk.Frame();
            this.preview_frame.Name = "preview_frame";
            // Container child preview_frame.Gtk.Container+ContainerChild
            this.preview_align = new Gtk.Alignment(0F, 0F, 1F, 1F);
            this.preview_align.Name = "preview_align";
            this.preview_align.LeftPadding = ((uint)(12));
            this.preview_align.RightPadding = ((uint)(12));
            this.preview_align.BottomPadding = ((uint)(6));
            this.preview_frame.Add(this.preview_align);
            this.GtkLabel4 = new Gtk.Label();
            this.GtkLabel4.Name = "GtkLabel4";
            this.GtkLabel4.LabelProp = Mono.Unix.Catalog.GetString("<b>Preview</b>");
            this.GtkLabel4.UseMarkup = true;
            this.preview_frame.LabelWidget = this.GtkLabel4;
            this.vbox4.Add(this.preview_frame);
            Gtk.Box.BoxChild w29 = ((Gtk.Box.BoxChild)(this.vbox4[this.preview_frame]));
            w29.Position = 0;
            this.vbox3.Add(this.vbox4);
            Gtk.Box.BoxChild w30 = ((Gtk.Box.BoxChild)(this.vbox3[this.vbox4]));
            w30.Position = 2;
            this.vbox1.Add(this.vbox3);
            Gtk.Box.BoxChild w31 = ((Gtk.Box.BoxChild)(this.vbox1[this.vbox3]));
            w31.Position = 2;
            this.alignment2.Add(this.vbox1);
            this.GtkAlignment.Add(this.alignment2);
            this.frame1.Add(this.GtkAlignment);
            this.GtkLabel5 = new Gtk.Label();
            this.GtkLabel5.Name = "GtkLabel5";
            this.GtkLabel5.LabelProp = Mono.Unix.Catalog.GetString("<b>Appearance</b>");
            this.GtkLabel5.UseMarkup = true;
            this.frame1.LabelWidget = this.GtkLabel5;
            this.vbox2.Add(this.frame1);
            Gtk.Box.BoxChild w35 = ((Gtk.Box.BoxChild)(this.vbox2[this.frame1]));
            w35.Position = 0;
            this.Add(this.vbox2);
            if ((this.Child != null)) {
                this.Child.ShowAll();
            }
            this.Show();
            this.theme_combo.Changed += new System.EventHandler(this.OnThemeComboChanged);
            this.pin_check.Clicked += new System.EventHandler(this.OnPinCheckClicked);
            this.shadow_check.Clicked += new System.EventHandler(this.OnShadowCheckClicked);
            this.background_colorbutton.ColorSet += new System.EventHandler(this.OnBackgroundColorbuttonColorSet);
            this.clear_background.Clicked += new System.EventHandler(this.OnClearBackgroundClicked);
        }
    }
}
