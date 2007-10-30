// SearchContext.cs created with MonoDevelop
// User: dave at 11:11 PM 8/30/2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

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
