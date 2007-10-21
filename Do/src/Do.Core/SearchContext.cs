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
		
		GCObject [] results;
				
		public SearchContext ()
		{
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
			clonedContext.Results = (GCObject[]) (results.Clone ());
			clonedContext.SearchPosition = searchPosition;
			return clonedContext;
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
	}
}
