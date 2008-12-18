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
    
    
    public partial class AbstractLoginWidget {
        
        private Gtk.VBox vbox1;
        
        private Gtk.Table table3;
        
        private Gtk.Entry password_entry;
        
        private Gtk.Label password_lbl;
        
        private Gtk.Entry username_entry;
        
        private Gtk.Label username_lbl;
        
        private Gtk.VBox vbox3;
        
        private Gtk.Label validate_lbl;
        
        private Gtk.Table table4;
        
        private Gtk.Fixed fixed5;
        
        private Gtk.Button validate_btn;
        
        private Gtk.VBox vbox4;
        
        private Gtk.Table table5;
        
        private Gtk.Fixed fixed1;
        
        private Gtk.Fixed fixed2;
        
        private Gtk.HSeparator hseparator1;
        
        private Gtk.Label get_account_lbl;
        
        private Gtk.HBox new_acct_hbox;
        
        private Gtk.Fixed fixed3;
        
        private Gtk.Fixed fixed4;
        
        protected virtual void Build() {
            Stetic.Gui.Initialize(this);
            // Widget Do.UI.AbstractLoginWidget
            Stetic.BinContainer.Attach(this);
            this.Name = "Do.UI.AbstractLoginWidget";
            // Container child Do.UI.AbstractLoginWidget.Gtk.Container+ContainerChild
            this.vbox1 = new Gtk.VBox();
            this.vbox1.Name = "vbox1";
            this.vbox1.Spacing = 6;
            // Container child vbox1.Gtk.Box+BoxChild
            this.table3 = new Gtk.Table(((uint)(2)), ((uint)(2)), false);
            this.table3.Name = "table3";
            this.table3.RowSpacing = ((uint)(6));
            this.table3.ColumnSpacing = ((uint)(6));
            this.table3.BorderWidth = ((uint)(7));
            // Container child table3.Gtk.Table+TableChild
            this.password_entry = new Gtk.Entry();
            this.password_entry.CanFocus = true;
            this.password_entry.Name = "password_entry";
            this.password_entry.IsEditable = true;
            this.password_entry.Visibility = false;
            this.password_entry.InvisibleChar = '●';
            this.table3.Add(this.password_entry);
            Gtk.Table.TableChild w1 = ((Gtk.Table.TableChild)(this.table3[this.password_entry]));
            w1.TopAttach = ((uint)(1));
            w1.BottomAttach = ((uint)(2));
            w1.LeftAttach = ((uint)(1));
            w1.RightAttach = ((uint)(2));
            w1.YOptions = ((Gtk.AttachOptions)(4));
            // Container child table3.Gtk.Table+TableChild
            this.password_lbl = new Gtk.Label();
            this.password_lbl.Name = "password_lbl";
            this.password_lbl.Xpad = 5;
            this.password_lbl.Xalign = 1F;
            this.password_lbl.LabelProp = Mono.Unix.Catalog.GetString("Password");
            this.table3.Add(this.password_lbl);
            Gtk.Table.TableChild w2 = ((Gtk.Table.TableChild)(this.table3[this.password_lbl]));
            w2.TopAttach = ((uint)(1));
            w2.BottomAttach = ((uint)(2));
            w2.YOptions = ((Gtk.AttachOptions)(4));
            // Container child table3.Gtk.Table+TableChild
            this.username_entry = new Gtk.Entry();
            this.username_entry.CanFocus = true;
            this.username_entry.Name = "username_entry";
            this.username_entry.IsEditable = true;
            this.username_entry.InvisibleChar = '●';
            this.table3.Add(this.username_entry);
            Gtk.Table.TableChild w3 = ((Gtk.Table.TableChild)(this.table3[this.username_entry]));
            w3.LeftAttach = ((uint)(1));
            w3.RightAttach = ((uint)(2));
            w3.XOptions = ((Gtk.AttachOptions)(4));
            w3.YOptions = ((Gtk.AttachOptions)(4));
            // Container child table3.Gtk.Table+TableChild
            this.username_lbl = new Gtk.Label();
            this.username_lbl.Name = "username_lbl";
            this.username_lbl.Xpad = 5;
            this.username_lbl.Xalign = 1F;
            this.username_lbl.LabelProp = Mono.Unix.Catalog.GetString("Username");
            this.table3.Add(this.username_lbl);
            Gtk.Table.TableChild w4 = ((Gtk.Table.TableChild)(this.table3[this.username_lbl]));
            w4.XOptions = ((Gtk.AttachOptions)(4));
            w4.YOptions = ((Gtk.AttachOptions)(4));
            this.vbox1.Add(this.table3);
            Gtk.Box.BoxChild w5 = ((Gtk.Box.BoxChild)(this.vbox1[this.table3]));
            w5.Position = 0;
            w5.Expand = false;
            w5.Fill = false;
            // Container child vbox1.Gtk.Box+BoxChild
            this.vbox3 = new Gtk.VBox();
            this.vbox3.Name = "vbox3";
            this.vbox3.Spacing = 6;
            // Container child vbox3.Gtk.Box+BoxChild
            this.validate_lbl = new Gtk.Label();
            this.validate_lbl.Name = "validate_lbl";
            this.validate_lbl.LabelProp = Mono.Unix.Catalog.GetString("<i>Verify and save account information</i>");
            this.validate_lbl.UseMarkup = true;
            this.vbox3.Add(this.validate_lbl);
            Gtk.Box.BoxChild w6 = ((Gtk.Box.BoxChild)(this.vbox3[this.validate_lbl]));
            w6.Position = 0;
            w6.Expand = false;
            w6.Fill = false;
            // Container child vbox3.Gtk.Box+BoxChild
            this.table4 = new Gtk.Table(((uint)(3)), ((uint)(2)), false);
            this.table4.Name = "table4";
            this.table4.RowSpacing = ((uint)(6));
            this.table4.ColumnSpacing = ((uint)(6));
            // Container child table4.Gtk.Table+TableChild
            this.fixed5 = new Gtk.Fixed();
            this.fixed5.WidthRequest = 119;
            this.fixed5.Name = "fixed5";
            this.fixed5.HasWindow = false;
            this.table4.Add(this.fixed5);
            Gtk.Table.TableChild w7 = ((Gtk.Table.TableChild)(this.table4[this.fixed5]));
            w7.YOptions = ((Gtk.AttachOptions)(4));
            // Container child table4.Gtk.Table+TableChild
            this.validate_btn = new Gtk.Button();
            this.validate_btn.WidthRequest = 40;
            this.validate_btn.CanFocus = true;
            this.validate_btn.Name = "validate_btn";
            this.validate_btn.UseStock = true;
            this.validate_btn.UseUnderline = true;
            this.validate_btn.BorderWidth = ((uint)(7));
            this.validate_btn.Label = "gtk-apply";
            this.table4.Add(this.validate_btn);
            Gtk.Table.TableChild w8 = ((Gtk.Table.TableChild)(this.table4[this.validate_btn]));
            w8.LeftAttach = ((uint)(1));
            w8.RightAttach = ((uint)(2));
            w8.YOptions = ((Gtk.AttachOptions)(4));
            this.vbox3.Add(this.table4);
            Gtk.Box.BoxChild w9 = ((Gtk.Box.BoxChild)(this.vbox3[this.table4]));
            w9.Position = 1;
            w9.Expand = false;
            w9.Fill = false;
            this.vbox1.Add(this.vbox3);
            Gtk.Box.BoxChild w10 = ((Gtk.Box.BoxChild)(this.vbox1[this.vbox3]));
            w10.Position = 1;
            // Container child vbox1.Gtk.Box+BoxChild
            this.vbox4 = new Gtk.VBox();
            this.vbox4.Name = "vbox4";
            this.vbox4.Spacing = 6;
            // Container child vbox4.Gtk.Box+BoxChild
            this.table5 = new Gtk.Table(((uint)(3)), ((uint)(3)), false);
            this.table5.Name = "table5";
            this.table5.RowSpacing = ((uint)(6));
            this.table5.ColumnSpacing = ((uint)(6));
            // Container child table5.Gtk.Table+TableChild
            this.fixed1 = new Gtk.Fixed();
            this.fixed1.Name = "fixed1";
            this.fixed1.HasWindow = false;
            this.table5.Add(this.fixed1);
            Gtk.Table.TableChild w11 = ((Gtk.Table.TableChild)(this.table5[this.fixed1]));
            w11.LeftAttach = ((uint)(2));
            w11.RightAttach = ((uint)(3));
            w11.YOptions = ((Gtk.AttachOptions)(4));
            // Container child table5.Gtk.Table+TableChild
            this.fixed2 = new Gtk.Fixed();
            this.fixed2.Name = "fixed2";
            this.fixed2.HasWindow = false;
            this.table5.Add(this.fixed2);
            Gtk.Table.TableChild w12 = ((Gtk.Table.TableChild)(this.table5[this.fixed2]));
            w12.YOptions = ((Gtk.AttachOptions)(4));
            // Container child table5.Gtk.Table+TableChild
            this.hseparator1 = new Gtk.HSeparator();
            this.hseparator1.WidthRequest = 190;
            this.hseparator1.Name = "hseparator1";
            this.table5.Add(this.hseparator1);
            Gtk.Table.TableChild w13 = ((Gtk.Table.TableChild)(this.table5[this.hseparator1]));
            w13.LeftAttach = ((uint)(1));
            w13.RightAttach = ((uint)(2));
            w13.YOptions = ((Gtk.AttachOptions)(4));
            this.vbox4.Add(this.table5);
            Gtk.Box.BoxChild w14 = ((Gtk.Box.BoxChild)(this.vbox4[this.table5]));
            w14.Position = 0;
            w14.Expand = false;
            w14.Fill = false;
            // Container child vbox4.Gtk.Box+BoxChild
            this.get_account_lbl = new Gtk.Label();
            this.get_account_lbl.Name = "get_account_lbl";
            this.get_account_lbl.UseMarkup = true;
            this.vbox4.Add(this.get_account_lbl);
            Gtk.Box.BoxChild w15 = ((Gtk.Box.BoxChild)(this.vbox4[this.get_account_lbl]));
            w15.Position = 1;
            w15.Expand = false;
            w15.Fill = false;
            // Container child vbox4.Gtk.Box+BoxChild
            this.new_acct_hbox = new Gtk.HBox();
            this.new_acct_hbox.Name = "new_acct_hbox";
            this.new_acct_hbox.Spacing = 6;
            // Container child new_acct_hbox.Gtk.Box+BoxChild
            this.fixed3 = new Gtk.Fixed();
            this.fixed3.Name = "fixed3";
            this.fixed3.HasWindow = false;
            this.new_acct_hbox.Add(this.fixed3);
            Gtk.Box.BoxChild w16 = ((Gtk.Box.BoxChild)(this.new_acct_hbox[this.fixed3]));
            w16.Position = 0;
            // Container child new_acct_hbox.Gtk.Box+BoxChild
            this.fixed4 = new Gtk.Fixed();
            this.fixed4.Name = "fixed4";
            this.fixed4.HasWindow = false;
            this.new_acct_hbox.Add(this.fixed4);
            Gtk.Box.BoxChild w17 = ((Gtk.Box.BoxChild)(this.new_acct_hbox[this.fixed4]));
            w17.Position = 2;
            this.vbox4.Add(this.new_acct_hbox);
            Gtk.Box.BoxChild w18 = ((Gtk.Box.BoxChild)(this.vbox4[this.new_acct_hbox]));
            w18.Position = 2;
            w18.Expand = false;
            w18.Fill = false;
            this.vbox1.Add(this.vbox4);
            Gtk.Box.BoxChild w19 = ((Gtk.Box.BoxChild)(this.vbox1[this.vbox4]));
            w19.Position = 2;
            w19.Expand = false;
            w19.Fill = false;
            this.Add(this.vbox1);
            if ((this.Child != null)) {
                this.Child.ShowAll();
            }
            this.password_entry.Hide();
            this.Show();
            this.password_entry.Activated += new System.EventHandler(this.OnPasswordEntryActivated);
            this.validate_btn.Clicked += new System.EventHandler(this.OnApplyBtnClicked);
        }
    }
}
