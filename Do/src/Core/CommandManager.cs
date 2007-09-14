// CommandManager.cs created with MonoDevelop
// User: dave at 9:58 AM 8/19/2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.Collections;
using System.Collections.Generic;

namespace Do.Core
{

	public class CommandManager : GCObjectManager
	{
			
		private Hashtable commandLists;
		
		public CommandManager()
		{
			commandLists = new Hashtable ();
		}
		
		public void AddCommand (Command command)
		{
			foreach (Type type in command.SupportedTypes) {
				List<Command> commands;
				if (!commandLists.ContainsKey (type)) {
					commandLists[type] = new List<Command> ();
				}
				commands = commandLists[type] as List<Command>;
				if (!commands.Contains (command)) {
					commands.Add (command);
				}
			}
		}
		
		public Command[] CommandsForItem (Item item, string match) {
			SearchContext context;
			
			context = new SearchContext ();
			context.Item = item;
			context.CommandSearchString = match;
			Search (context);
			return context.Results as Command[];
		}
		
		private Command[] _CommandsForItem (Item item) {
			List<Command> commands;
			
			commands = new List<Command> ();
			Type[] interfaces = item.IItem.GetType ().GetInterfaces ();
			foreach (Type type in interfaces) {
				if (commandLists.ContainsKey (type)) {
					foreach (Command command in commandLists[type] as IEnumerable<Command>) {
						if (command.SupportsItem (item.IItem)) {
							commands.Add (command);
						}
					}
				}
			}
			return commands.ToArray ();
		}
		
		protected override ContextRelation GetContextRelation (SearchContext a, SearchContext b)
		{
			
			if (a.Item == b.Item && a.CommandSearchString == b.CommandSearchString)
				return ContextRelation.Repeat;
			else if (a.Item == b.Item && a.CommandSearchString.StartsWith (b.CommandSearchString))
				return ContextRelation.Continuation;
			else
				return ContextRelation.Fresh;
		}
		
		protected override void PerformSearch (SearchContext context)
		{
			int numScoreNonZero;
			Command[] commands;
			
			// Use intermediate search results if available.
			if (context.Results == null) {
				commands = _CommandsForItem (context.Item);
			} else {
				commands = context.Results as Command[];
			}
			
			// Score the commands based on the search string and sort them.
			foreach (Command command in commands) {
				command.Score = command.ScoreForAbbreviation (context.CommandSearchString);
			}
			Array.Sort<GCObject> (commands, new GCObjectScoreComparer ());
			
			// Chop the array where the scores become zero
			for (numScoreNonZero = 0; numScoreNonZero < commands.Length; ++numScoreNonZero) {
				if (commands[numScoreNonZero].Score == 0) break;
			}
			Array.Resize<Command> (ref commands, numScoreNonZero);
			
			context.Results = commands;
		}


	}
}
