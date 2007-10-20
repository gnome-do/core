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
			List<Type> types;
			Type baseType;
			
			commands = new List<Command> ();
			types = new List<Type> ();
			
			// Climb up the inheritance tree adding types.
			baseType = item.IItem.GetType ();
			while (baseType != typeof (object)) {
				types.Add (baseType);
				baseType = baseType.BaseType;    
			}
			// Add all implemented interfaces
			types.AddRange (item.IItem.GetType ().GetInterfaces ());
			foreach (Type type in types) {
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
			else if (a.Item == b.Item
			         && a.CommandSearchString.StartsWith (b.CommandSearchString))
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
