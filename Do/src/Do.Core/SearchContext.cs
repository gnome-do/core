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
		SentencePositionLocator searchPosition;
		SearchContext lastContext;
		SearchContext lastItemContext;
		SearchContext lastCommandContext;
		SearchContext lastModifierItemContext;
		
		GCObject [] results;
				
		public SearchContext ()
		{
			searchPosition = SentencePositionLocator.Item;
		}
		
		public SearchContext Clone () {
			SearchContext clonedContext = new SearchContext ();
			clonedContext.Command = command;
			clonedContext.CommandSearchString = commandSearchString;
			clonedContext.IndirectItem = iitem;
			clonedContext.IndirectItemSearchString = indirectItemSearchString;
			clonedContext.Item = item;
			clonedContext.ItemSearchString = itemSearchString;
			clonedContext.LastContext = lastContext;
			if (results != null) {
				clonedContext.Results = (GCObject[]) (results.Clone ());
			}
			clonedContext.LastCommandContext = lastCommandContext;
			clonedContext.LastModifierItemContext = lastModifierItemContext;
			clonedContext.LastItemContext = lastItemContext;
			clonedContext.SearchPosition = searchPosition;
			return clonedContext;
		}
		
		public SearchContext LastCommandContext {
			get {
				return lastCommandContext;
			}
			set {
				lastCommandContext = value;
			}
		}
		
		public SearchContext LastItemContext {
			get {
				return lastItemContext;
			}
			set {
				lastItemContext = value;
			}
		}
		
		public SearchContext LastModifierItemContext {
			get {
				return lastModifierItemContext;
			}
			set {
				lastModifierItemContext = value;
			}
		}
		
		public SearchContext LastContext {
			get {
				return lastContext;
			}
			set {
				lastContext = value;
			}
		}
		
		public SentencePositionLocator SearchPosition {
			get {
				return searchPosition;
			}
			set {
				searchPosition = value;
			}
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
		
		public int ObjectIndex {
			get {
				int index = -1;
				if (results == null) {
					return index;
				}
				if (searchPosition == SentencePositionLocator.Command) {
					for (int i = 0; i < results.Length; i++) {
						if (results[i].Equals (command)) {
							index = i;
							return index;
						}
				}
				}
				else if (searchPosition == SentencePositionLocator.Item) {
					for (int i = 0; i < results.Length; i++) {
						if (results[i].Equals (item)) {
							index = i;
							return index;
						}
					}
				}
				else {
					for (int i = 0; i < results.Length; i++) {
						if (results[i].Equals (iitem)) {
							index = i;
							return index;
						}
					}
				}
				return -1;
			}
			set {
				if (searchPosition == SentencePositionLocator.Command) {
					command = (Command) results[value];
				}
				else if (searchPosition == SentencePositionLocator.Item) {
					item = (Item) results[value];
				}
				else {
					iitem = (Item) results[value];
				}
			}
		}
						
	}
}
