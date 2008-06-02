/* AbstractLogin.cs
 *
 * GNOME Do is the legal property of its developers. Please refer to the
 * COPYRIGHT file distributed with this
 * source distribution.
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
using System.Threading;
using System.Collections;

using Gnome.Keyring;
using Gtk;

namespace Do.Addins
{
	/// <summary>
	/// A class providing a generic login widget for plugins that will need
	/// to log into an external service. Provides a clean UI and enforces
	/// asynchronous validation so the plugin developer doesn't need to know
	/// about delegates or any complex concepts. To see an example of this
	/// class in use, see GMailContacts.
	/// </summary>
	public abstract partial class AbstractLoginWidget : Gtk.Bin
	{
		
		private LinkButton new_acct_btn;
		
		/// <summary>
		/// Builds the generic UI with the name passed in by service. 
		/// </summary>
		/// <param name="service">
		/// A <see cref="System.String"/>
		/// </param>
		public AbstractLoginWidget (string service)
		{
			this.Build ();
			
			get_account_lbl.Markup = String.Format ("<i>Don't have {0}?</i>",service);
			new_acct_btn = new LinkButton ("", String.Format ("Sign up for {0}",
				service));
			this.hbox1.Add (new_acct_btn);
			Box.BoxChild wInt = hbox1 [new_acct_btn] as Box.BoxChild;
			wInt.Position = 1;
			
			new_acct_btn.Clicked += OnNewAcctBtnClicked;
			
			string username, password;
			username = password = "";
			GetValidatedAccountData (out username, out password,
				this.GetType ().FullName);
			
			username_entry.Text = username;
			password_entry.Text = password;
			
			this.ShowAll ();
		}
		
		/// <summary>
		/// Default contructor that initializes any service names with the
		/// generic string "an account".
		/// </summary>
		public AbstractLoginWidget () : 
			this ("an account")
		{
			 // nothing
		}
		
		/// <value>
		/// Provides access to the Gtk.Entry field for a username
		/// </value>
		protected Gtk.Entry UsernameEntry {
			get { 
				return username_entry;
			}
		}
		
		/// <value>
		/// Provides access to the Gtk.Label for the username
		/// Access to this field is provided as a courtesy to developers
		/// who would like to customize their UI. It is not generally needed.
		/// </value>
		protected Gtk.Label UsernameLabel {
			get {
				return username_lbl;
			}
		}
		
		/// <value>
		/// Provides access to the Gtk.Entry field for the password
		/// </value>
		protected Gtk.Entry PasswordEntry {
			get {
				return password_entry;
			}
		}
		
		/// <value>
		/// Provides access to the Gtk.Label for the password
		/// Access to this field is provided as a courtesy to developers
		/// who would like to customize their UI. It is not generally needed.
		/// </value>
		protected Gtk.Label PasswordLabel {
			get {
				return password_lbl;
			}
		}
		
		/// <value>
		/// Provides access to the Gtk.Label for current validation status
		/// </value>
		protected Gtk.Label StatusLabel {
			get {
				return validate_lbl;
			}
		}
		
		/// <value>
		/// Provides access to the Gtk.Button to validate account settings
		/// </value>
		protected Gtk.Button ValidateButton {
			get {
				return validate_btn;
			}
		}
		
		/// <value>
		/// Provides access to the Gtk.Label for signing up for the service
		/// Access to this field is provided as a courtesy to developers
		/// who would like to customize their UI. It is not generally needed.
		/// </value>
		protected Gtk.Label GetAccountLabel {
			get {
				return get_account_lbl;
			}
		}
		
		/// <value>
		/// Provides access to the Gtk.Label field for the username.
		/// Generally only the .Uri property needs set.
		/// </value>
		protected Gtk.LinkButton GetAccountButton {
			get {
				return new_acct_btn;
			}
		}
		
		/// <summary>
		/// Fires when button to validate account is clicked
		/// </summary>
		/// <param name="sender">
		/// A <see cref="System.Object"/>
		/// </param>
		/// <param name="e">
		/// A <see cref="EventArgs"/>
		/// </param>
		protected virtual void OnApplyBtnClicked (object sender, EventArgs e)
		{
			new Thread ((ThreadStart) delegate {					
				Gtk.Application.Invoke (delegate {
					if (Validate ()) {
						StatusLabel.Markup = "<i>Account validation succeeded</i>!";
						SaveAccountData (username_entry.Text, password_entry.Text);
					} else {
						StatusLabel.Markup = "<i>Account validation failed!</i>";
					}
					ValidateButton.Sensitive = true;
				});
			}).Start ();
		}
		
		/// <summary>
		/// Opens new browser window with the uri from new_acct_btn.
		/// if uri is unset, button does nothing.
		/// </summary>
		/// <param name="sender">
		/// A <see cref="System.Object"/>
		/// </param>
		/// <param name="e">
		/// A <see cref="EventArgs"/>
		/// </param>
		protected virtual void OnNewAcctBtnClicked (object sender, EventArgs e)
		{
			if (!String.IsNullOrEmpty (new_acct_btn.Uri))
				Util.Environment.Open (new_acct_btn.Uri);
		}
		
		/// <summary>
		/// Method to save account data to permanant storage whether it be
		/// GConf, gnome-keyring, or a flat file.
		/// </summary>
		protected void SaveAccountData (string username, string password)
		{
			string keyringItemName = this.GetType ().FullName;
			string keyring;
			Hashtable ht;
			
			try {
				keyring = Ring.GetDefaultKeyring ();
				ht = new Hashtable ();
				ht["name"] = keyringItemName;
				ht["username"] = username;
				
				Ring.CreateItem (keyring, ItemType.GenericSecret, keyringItemName,
					ht, password, true);
				                 
			} catch (Exception e) {
				Console.Error.WriteLine (e.Message);
			}
		}
		
		/// <summary>
		/// Method to load validated account data from gnome-keyring
		/// </summary>
		protected static void GetValidatedAccountData (out string username, 
		                                               out string password,
		                                               string keyringItemName)
		{
			username = password = "";
			Hashtable ht = new Hashtable ();
			ht ["name"] = keyringItemName;
			
			try {
				foreach (ItemData s in Ring.Find (ItemType.GenericSecret, ht)) {
					if (s.Attributes.ContainsKey ("name") && s.Attributes.ContainsKey ("username")
					    && (s.Attributes ["name"] as string).Equals (keyringItemName)) {
						username = s.Attributes ["username"] as string;
						password = s.Secret;
						return;
					}
				}
			} catch (Exception) {
				Console.Error.WriteLine ("No account info stored for {0}",
					keyringItemName);
			}
		}
		
		/// <summary>
		/// Makes validation call with account credentials passed in
		/// through the username and password entry fields.
		/// </summary>
		protected abstract bool Validate ();
	}
}
