/* ${FileName}
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

namespace Do.Core
{
	public enum ContextRelation {
		Fresh,
		Repeat,
		Continuation
	}
	
	public class SearchContext
	{
		Item item, iitem;
		Command command;
		string itemSearchString, indirectItemSearchString, commandSearchString;
		
		GCObject [] results;
				
		public SearchContext ()
		{
		}
		
		public Item Item {
			get {
				return item;
			}
			set {
				item = value;
			}
		}

		public Item IndirectItem {
			get {
				return iitem;
			}
			set {
				iitem = value;
			}
		}

		public Command Command {
			get {
				return command;
			}
			set {
				command = value;
			}
		}

		public string ItemSearchString {
			get {
				return itemSearchString;
			}
			set {
				itemSearchString = value;
			}
		}

		public string IndirectItemSearchString {
			get {
				return indirectItemSearchString;
			}
			set {
				indirectItemSearchString = value;
			}
		}

		public string CommandSearchString {
			get {
				return commandSearchString;
			}
			set {
				commandSearchString = value;
			}
		}

		public GCObject[] Results {
			get {
				return results;
			}
			set {
				results = value;
			}
		}
	}
}
