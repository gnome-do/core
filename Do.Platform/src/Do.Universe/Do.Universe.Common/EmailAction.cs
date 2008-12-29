// EmailAction.cs
// 
// GNOME Do is the legal property of its developers, whose names are too
// numerous to list here.  Please refer to the COPYRIGHT file distributed with
// this source distribution.
// 
// This program is free software: you can redistribute it and/or modify it under
// the terms of the GNU General Public License as published by the Free Software
// Foundation, either version 3 of the License, or (at your option) any later
// version.
// 
// This program is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
// FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more
// details.
// 
// You should have received a copy of the GNU General Public License along with
// this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Mono.Unix;

using Do.Platform;

namespace Do.Universe.Common
{
	public class EmailAction : Act
	{
		public override string Name {
			get {
				return Catalog.GetString ("Email");
			}
		}

		public override string Description {
			get {
				return Catalog.GetString ("Compose a new email to a friend.");
			}
		}

		public override string Icon {
			get { return "stock_mail-compose"; }
		}

		public override IEnumerable<Type> SupportedItemTypes {
			get {
				yield return typeof (ContactItem);
				yield return typeof (IContactDetailItem);
				yield return typeof (ITextItem);
			}
		}

		public override IEnumerable<Type> SupportedModifierItemTypes {
			get {
				yield return typeof (IFileItem);
				yield return typeof (ITextItem);
			}
		}

		public override bool ModifierItemsOptional {
			get { return true; }
		}

		public override bool SupportsItem (Item item)
		{
			if (item is ContactItem) {
				return (item as ContactItem).Details.Any (d => d.StartsWith ("email"));
			} else if (item is IContactDetailItem) {
				return (item as IContactDetailItem).Key.StartsWith ("email");
			} else if (item is ITextItem) {
				return new Regex (
					// Regex should conform to RFC2822.
					@"[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?\.)+[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?"
				).IsMatch ((item as ITextItem).Text);
			}
			return false;
		}

		public override IEnumerable<Item> Perform (IEnumerable<Item> items, IEnumerable<Item> modItems)
		{
			string subject, body;
			IEnumerable<string> recipients, texts, files;

			recipients = items
				.Select (item => {
					if (item is ContactItem) {
						ContactItem contact = item as ContactItem;
						string emailKey = contact.Details.FirstOrDefault (d => d.StartsWith ("email"));
						return contact [emailKey];
					} else if (item is IContactDetailItem)
						return (item as IContactDetailItem).Value;
					else if (item is ITextItem)
						return (item as ITextItem).Text;
					else return "";
				})
				.Where (email => !string.IsNullOrEmpty (email));

			texts = modItems.OfType<ITextItem> ().Select (item => item.Text);
			files = modItems.OfType<IFileItem> ().Select (item => item.Path);
			subject = texts.FirstOrDefault () ?? "";
			body = texts.Aggregate ("", (a, b) => a + "\n\n" + b);

			Services.Environment.OpenEmail (recipients, subject, body, files); 
			yield break;
		}
	}
}
