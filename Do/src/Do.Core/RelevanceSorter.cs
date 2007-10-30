// /home/dave/trunk-md/Do/src/Do.Core/RelevanceSorter.cs created with MonoDevelop
// User: dave at 8:16 PM 10/19/2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.Collections;
using System.Collections.Generic;
using Do.Universe;

namespace Do.Core
{
	
	
	public class RelevanceSorter : IComparer<IObject>
	{
		SentencePositionLocator sentencePosition;
		string searchString;
		
		public RelevanceSorter(string searchString, SentencePositionLocator sentencePosition)
		{
			this.sentencePosition = sentencePosition;
			this.searchString = searchString;
		}
		
		public int Compare (IObject x, IObject y) {
			return 0;
		}
		
		public List<IObject> NarrowResults (List<IObject> broadResults) {
			List<IObject> narrowResults;
			if (sentencePosition == SentencePositionLocator.Command) {
				int numScoreNonZero;
				Command[] commands;
				IObject[] uncastedCommands = broadResults.ToArray ();
				commands = new Command[uncastedCommands.Length];
				for (int i = 0; i < uncastedCommands.Length; i++) {
					commands[i] = (Command) (uncastedCommands[i]);
				}
				
				foreach (Command command in commands) {
					command.Score = command.ScoreForAbbreviation (searchString);
				}
				Array.Sort<GCObject> (commands, new GCObjectScoreComparer ());
				
				// Chop the array where the scores become zero
				for (numScoreNonZero = 0; numScoreNonZero < commands.Length && numScoreNonZero < 1000; ++numScoreNonZero) {
					if (commands[numScoreNonZero].Score == 0) break;
				}
				Array.Resize<Command> (ref commands, numScoreNonZero);
				
				narrowResults = new List<IObject> (commands);
			}
			else {
				int numScoreAboveCutoff, cutoff;
				Item [] items;
				IObject[] uncastedItems = broadResults.ToArray ();
				items = new Item[uncastedItems.Length];
				for (int i = 0; i < uncastedItems.Length; i++) {
					items[i] = (Item) (uncastedItems[i]);
				}
			
				cutoff = 30;
				// Score the commands based on the search string and sort them.
				foreach (GCObject item in items) {
					item.Score = item.ScoreForAbbreviation (searchString);
				}
				Array.Sort<GCObject> (items, new GCObjectScoreComparer ());
				
				// Chop the array where the scores become less than cutoff
				for (numScoreAboveCutoff = 0; numScoreAboveCutoff < items.Length && numScoreAboveCutoff < 1000; ++numScoreAboveCutoff) {
					if (items [numScoreAboveCutoff].Score < cutoff) break;
				}
				Array.Resize<Item> (ref items, numScoreAboveCutoff);
				
				narrowResults = new List<IObject> (items);
			}
			return narrowResults;
		}
	}
}
