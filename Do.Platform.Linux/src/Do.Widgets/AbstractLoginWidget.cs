/* AbstractLoginWidget.cs 
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

using Gtk;

using Mono.Unix;

namespace Do.Platform.Linux
{

	/// <summary>
	/// A class providing a generic login widget for plugins that will need
	/// to log into an external service. Provides a clean UI and enforces
	/// asynchronous validation so the plugin developer doesn't need to know
	/// about delegates or any complex concepts. To see an example of this
	/// class in use, see the Microblogging plugin.
	/// Things to do when you implement this class:
	/// 	* Setup default values of username_entry and password_entry
	/// 	* 
	/// </summary>
	[System.ComponentModel.ToolboxItem(true)]
	public abstract partial class AbstractLoginWidget : Gtk.Bin
	{
		protected readonly string NewAccountButtonFormat = Catalog.GetString ("Sign up for {0}");
		protected readonly string BusyValidatingLabel = Catalog.GetString ("<i>Validating...</i>");
		protected readonly string NewAccountLabelFormat = Catalog.GetString ("<i>Don't have {0}?</i>");
		protected readonly string AccountValidationFailedLabel = Catalog.GetString ("<i>Account validation failed!</i>");
		protected readonly string AccountValidationSucceededLabel = Catalog.GetString ("<i>Account validation succeeded!</i>");

		// our Gtk widgets that we are exposing to subclasses
		protected Entry UsernameEntry { get; set; }
		protected Entry PasswordEntry { get; set; }
		protected Label UsernameLabel { get; set; }
		protected Label PasswordLabel { get; set; }
		protected Label ValidateLabel { get; set; }
		protected Button ValidateButton { get; set; }
		protected Label NewAccountLabel { get; set; }
		protected LinkButton NewAccountButton { get; set; }
		
		public AbstractLoginWidget (string serviceName) : this (serviceName, "")
		{
		}

		public AbstractLoginWidget (string serviceName, string serviceUri)
		{
			this.Build();

			// put gtk widgets inside protected wrapper for subclasses
			UsernameLabel = username_lbl;
			PasswordLabel = password_lbl;
			UsernameEntry = username_entry;
			PasswordEntry = password_entry;
			ValidateLabel = validate_lbl;
			ValidateButton = validate_btn;
			NewAccountLabel = new_account_lbl;

			// setup the link button
			NewAccountButton = new LinkButton (serviceUri, string.Format (NewAccountButtonFormat, serviceName));
			new_account_button_box.Add (NewAccountButton);
			new_account_vbox.Add (NewAccountButton);

			password_entry.Activated += OnPasswordEntryActivated;
			NewAccountButton.Clicked += OnNewAccountBtnClicked;
			
			if (string.IsNullOrEmpty (serviceUri)) {
				NewAccountLabel.Visible = false;
				NewAccountButton.Visible = false;
			} else {
				NewAccountLabel.Markup = string.Format (NewAccountLabelFormat, serviceName);
			}
		}

		abstract protected void SaveAccountData (string username, string password);

		abstract protected bool Validate (string username, string password);

		protected virtual void OnNewAccountBtnClicked (object sender, EventArgs e)
		{
			if (!string.IsNullOrEmpty (NewAccountButton.Uri))
				Services.Environment.OpenUrl (NewAccountButton.Uri);
		}
		
		protected virtual void OnApplyBtnClicked (object sender, EventArgs args)
		{
			validate_lbl.Markup = BusyValidatingLabel;
			validate_btn.Sensitive = false;
			
			string username = username_entry.Text;
			string password = password_entry.Text;
			
			Thread thread = new Thread (new ThreadStart(() => ValidateCredentials (username, password)));
			
			thread.IsBackground = true;
			thread.Start ();
		}

		private void ValidateCredentials (string username, string password)
		{
			bool valid = Validate (username, password);
			Gtk.Application.Invoke ((o, e) => UpdateInterface (username, password, valid));
		}

		private void UpdateInterface (string username, string password, bool valid)
		{
			if (valid) {
				validate_lbl.Markup = AccountValidationSucceededLabel;
				SaveAccountData (username, password);
			} else {
				validate_lbl.Markup = AccountValidationFailedLabel;
			}
			validate_btn.Sensitive = true;
		}

		protected virtual void OnUsernameEntryActivated (object sender, System.EventArgs e)
		{
			password_entry.GrabFocus ();
		}

		protected virtual void OnPasswordEntryActivated (object sender, System.EventArgs e)
		{
			validate_btn.Activate ();
		}
	}
}
