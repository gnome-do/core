// ------------------------------------------------------------------------------
//  <autogenerated>
//      This code was generated by a tool.
//      
// 
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
//  </autogenerated>
// ------------------------------------------------------------------------------

namespace Do.UI {
    
    
    public partial class ColorConfigurationWidget {
        
        private Gtk.VBox vbox2;
        
        private Gtk.Alignment alignment1;
        
        private Gtk.VBox vbox1;
        
        private Gtk.Frame frame1;
        
        private Gtk.Alignment GtkAlignment1;
        
        private Gtk.Alignment GtkAlignment2;
        
        private Gtk.Alignment alignment2;
        
        private Gtk.VBox vbox3;
        
        private Gtk.Table table3;
        
        private Gtk.HBox hbox10;
        
        private Gtk.HBox hbox4;
        
        private Gtk.ComboBox theme_combo;
        
        private Gtk.CheckButton pin_check;
        
        private Gtk.Label GtkLabel2;
        
        private Gtk.VBox composite_warning_widget;
        
        private Gtk.Label label1;
        
        private Gtk.HButtonBox hbuttonbox1;
        
        private Gtk.Button composite_warning_info_btn;
        
        private Gtk.HSeparator hseparator1;
        
        private Gtk.Alignment theme_configuration_container;
        
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
            this.alignment1 = new Gtk.Alignment(0.5F, 0.5F, 1F, 1F);
            this.alignment1.Name = "alignment1";
            this.alignment1.TopPadding = ((uint)(4));
            this.alignment1.RightPadding = ((uint)(7));
            // Container child alignment1.Gtk.Container+ContainerChild
            this.vbox1 = new Gtk.VBox();
            this.vbox1.Name = "vbox1";
            this.vbox1.Spacing = 6;
            // Container child vbox1.Gtk.Box+BoxChild
            this.frame1 = new Gtk.Frame();
            this.frame1.Name = "frame1";
            this.frame1.ShadowType = ((Gtk.ShadowType)(0));
            // Container child frame1.Gtk.Container+ContainerChild
            this.GtkAlignment1 = new Gtk.Alignment(0F, 0F, 1F, 1F);
            this.GtkAlignment1.Name = "GtkAlignment1";
            this.GtkAlignment1.LeftPadding = ((uint)(12));
            // Container child GtkAlignment1.Gtk.Container+ContainerChild
            this.GtkAlignment2 = new Gtk.Alignment(0F, 0F, 1F, 1F);
            this.GtkAlignment2.Name = "GtkAlignment2";
            this.GtkAlignment2.LeftPadding = ((uint)(5));
            this.GtkAlignment2.TopPadding = ((uint)(5));
            this.GtkAlignment2.RightPadding = ((uint)(5));
            this.GtkAlignment2.BottomPadding = ((uint)(5));
            // Container child GtkAlignment2.Gtk.Container+ContainerChild
            this.alignment2 = new Gtk.Alignment(0.5F, 0.5F, 1F, 1F);
            this.alignment2.Name = "alignment2";
            // Container child alignment2.Gtk.Container+ContainerChild
            this.vbox3 = new Gtk.VBox();
            this.vbox3.Name = "vbox3";
            this.vbox3.Spacing = 6;
            // Container child vbox3.Gtk.Box+BoxChild
            this.table3 = new Gtk.Table(((uint)(2)), ((uint)(2)), false);
            this.table3.Name = "table3";
            this.table3.RowSpacing = ((uint)(6));
            this.table3.ColumnSpacing = ((uint)(6));
            // Container child table3.Gtk.Table+TableChild
            this.hbox10 = new Gtk.HBox();
            this.hbox10.WidthRequest = 70;
            this.hbox10.Name = "hbox10";
            this.hbox10.Spacing = 6;
            this.table3.Add(this.hbox10);
            Gtk.Table.TableChild w1 = ((Gtk.Table.TableChild)(this.table3[this.hbox10]));
            w1.TopAttach = ((uint)(1));
            w1.BottomAttach = ((uint)(2));
            w1.XOptions = ((Gtk.AttachOptions)(4));
            w1.YOptions = ((Gtk.AttachOptions)(4));
            // Container child table3.Gtk.Table+TableChild
            this.hbox4 = new Gtk.HBox();
            this.hbox4.Name = "hbox4";
            this.hbox4.Spacing = 6;
            // Container child hbox4.Gtk.Box+BoxChild
            this.theme_combo = Gtk.ComboBox.NewText();
            this.theme_combo.Name = "theme_combo";
            this.hbox4.Add(this.theme_combo);
            Gtk.Box.BoxChild w2 = ((Gtk.Box.BoxChild)(this.hbox4[this.theme_combo]));
            w2.Position = 0;
            w2.Expand = false;
            w2.Fill = false;
            this.table3.Add(this.hbox4);
            Gtk.Table.TableChild w3 = ((Gtk.Table.TableChild)(this.table3[this.hbox4]));
            w3.LeftAttach = ((uint)(1));
            w3.RightAttach = ((uint)(2));
            w3.XOptions = ((Gtk.AttachOptions)(4));
            w3.YOptions = ((Gtk.AttachOptions)(4));
            // Container child table3.Gtk.Table+TableChild
            this.pin_check = new Gtk.CheckButton();
            this.pin_check.CanFocus = true;
            this.pin_check.Name = "pin_check";
            this.pin_check.Label = Mono.Unix.Catalog.GetString("Always show results window");
            this.pin_check.DrawIndicator = true;
            this.pin_check.UseUnderline = true;
            this.table3.Add(this.pin_check);
            Gtk.Table.TableChild w4 = ((Gtk.Table.TableChild)(this.table3[this.pin_check]));
            w4.TopAttach = ((uint)(1));
            w4.BottomAttach = ((uint)(2));
            w4.LeftAttach = ((uint)(1));
            w4.RightAttach = ((uint)(2));
            w4.XOptions = ((Gtk.AttachOptions)(4));
            w4.YOptions = ((Gtk.AttachOptions)(4));
            this.vbox3.Add(this.table3);
            Gtk.Box.BoxChild w5 = ((Gtk.Box.BoxChild)(this.vbox3[this.table3]));
            w5.Position = 0;
            w5.Expand = false;
            w5.Fill = false;
            this.alignment2.Add(this.vbox3);
            this.GtkAlignment2.Add(this.alignment2);
            this.GtkAlignment1.Add(this.GtkAlignment2);
            this.frame1.Add(this.GtkAlignment1);
            this.GtkLabel2 = new Gtk.Label();
            this.GtkLabel2.Name = "GtkLabel2";
            this.GtkLabel2.LabelProp = Mono.Unix.Catalog.GetString("<b>Selected Theme</b>");
            this.GtkLabel2.UseMarkup = true;
            this.frame1.LabelWidget = this.GtkLabel2;
            this.vbox1.Add(this.frame1);
            Gtk.Box.BoxChild w10 = ((Gtk.Box.BoxChild)(this.vbox1[this.frame1]));
            w10.Position = 0;
            // Container child vbox1.Gtk.Box+BoxChild
            this.composite_warning_widget = new Gtk.VBox();
            this.composite_warning_widget.Name = "composite_warning_widget";
            this.composite_warning_widget.Spacing = 6;
            // Container child composite_warning_widget.Gtk.Box+BoxChild
            this.label1 = new Gtk.Label();
            this.label1.Name = "label1";
            this.label1.LabelProp = Mono.Unix.Catalog.GetString("<b>Your display is not properly configured for theme and animation support. To use these features, you must enable compositing.</b>");
            this.label1.UseMarkup = true;
            this.label1.Wrap = true;
            this.composite_warning_widget.Add(this.label1);
            Gtk.Box.BoxChild w11 = ((Gtk.Box.BoxChild)(this.composite_warning_widget[this.label1]));
            w11.Position = 0;
            w11.Expand = false;
            w11.Fill = false;
            // Container child composite_warning_widget.Gtk.Box+BoxChild
            this.hbuttonbox1 = new Gtk.HButtonBox();
            this.hbuttonbox1.LayoutStyle = ((Gtk.ButtonBoxStyle)(4));
            // Container child hbuttonbox1.Gtk.ButtonBox+ButtonBoxChild
            this.composite_warning_info_btn = new Gtk.Button();
            this.composite_warning_info_btn.CanFocus = true;
            this.composite_warning_info_btn.Name = "composite_warning_info_btn";
            this.composite_warning_info_btn.UseStock = true;
            this.composite_warning_info_btn.UseUnderline = true;
            this.composite_warning_info_btn.Label = "gtk-dialog-info";
            this.hbuttonbox1.Add(this.composite_warning_info_btn);
            Gtk.ButtonBox.ButtonBoxChild w12 = ((Gtk.ButtonBox.ButtonBoxChild)(this.hbuttonbox1[this.composite_warning_info_btn]));
            w12.Expand = false;
            w12.Fill = false;
            this.composite_warning_widget.Add(this.hbuttonbox1);
            Gtk.Box.BoxChild w13 = ((Gtk.Box.BoxChild)(this.composite_warning_widget[this.hbuttonbox1]));
            w13.Position = 1;
            w13.Expand = false;
            w13.Fill = false;
            this.vbox1.Add(this.composite_warning_widget);
            Gtk.Box.BoxChild w14 = ((Gtk.Box.BoxChild)(this.vbox1[this.composite_warning_widget]));
            w14.Position = 1;
            w14.Expand = false;
            w14.Fill = false;
            // Container child vbox1.Gtk.Box+BoxChild
            this.hseparator1 = new Gtk.HSeparator();
            this.hseparator1.Name = "hseparator1";
            this.vbox1.Add(this.hseparator1);
            Gtk.Box.BoxChild w15 = ((Gtk.Box.BoxChild)(this.vbox1[this.hseparator1]));
            w15.Position = 2;
            w15.Expand = false;
            w15.Fill = false;
            this.alignment1.Add(this.vbox1);
            this.vbox2.Add(this.alignment1);
            Gtk.Box.BoxChild w17 = ((Gtk.Box.BoxChild)(this.vbox2[this.alignment1]));
            w17.Position = 0;
            // Container child vbox2.Gtk.Box+BoxChild
            this.theme_configuration_container = new Gtk.Alignment(0.5F, 0.5F, 1F, 1F);
            this.theme_configuration_container.Name = "theme_configuration_container";
            this.theme_configuration_container.RightPadding = ((uint)(6));
            this.theme_configuration_container.BottomPadding = ((uint)(3));
            this.vbox2.Add(this.theme_configuration_container);
            Gtk.Box.BoxChild w18 = ((Gtk.Box.BoxChild)(this.vbox2[this.theme_configuration_container]));
            w18.PackType = ((Gtk.PackType)(1));
            w18.Position = 1;
            this.Add(this.vbox2);
            if ((this.Child != null)) {
                this.Child.ShowAll();
            }
            this.composite_warning_info_btn.Hide();
            this.composite_warning_widget.Hide();
            this.Show();
            this.pin_check.Clicked += new System.EventHandler(this.OnPinCheckClicked);
            this.theme_combo.Changed += new System.EventHandler(this.OnThemeComboChanged);
            this.composite_warning_info_btn.Clicked += new System.EventHandler(this.OnCompositeWarningInfoBtnClicked);
        }
    }
}
