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
using Gtk;

namespace Do.Addins
{
	public abstract partial class AbstractLogin : Gtk.Bin
	{		
		private LinkButton newAcct_btn;
		public AbstractLogin()
		{
			this.Build();
			newAcct_btn = new LinkButton ("", "Get account");
			this.hbox1.Add (newAcct_btn);
			Gtk.Box.BoxChild wInt = ((Gtk.Box.BoxChild)(this.hbox1[this.newAcct_btn]));
			wInt.Position = 1;
			
			newAcct_btn.Clicked += OnNewAcctBtnClicked;
			this.ShowAll ();
		}
		
		protected Gtk.Entry Username {
			get { 
				return username_entry;
			}
		}
		
		protected Gtk.Entry Password {
			get {
				return password_entry;
			}
		}
		
		protected Gtk.Label StatusLabel {
			get {
				return validate_lbl;
			}
		}
		
		protected Gtk.Button ValidateButton {
			get {
				return validate_btn;
			}
		}
		
		protected Gtk.Label GetAccountLabel {
			get {
				return get_account_lbl;
			}
		}
		
		protected Gtk.LinkButton GetAccountButton {
			get {
				return newAcct_btn;
			}
		}
		
		
		
		protected abstract void Validate ();

		protected virtual void OnApplyBtnClicked (object sender, System.EventArgs e)
		{
			Validate ();
		}
		
		protected virtual void OnNewAcctBtnClicked (object sender, System.EventArgs e)
		{
			Util.Environment.Open (newAcct_btn.Uri);
		}
	}
}
