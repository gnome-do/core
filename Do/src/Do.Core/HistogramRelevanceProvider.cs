// HistogramRelevanceProvider.cs
//
// GNOME Do is the legal property of its developers. Please refer to the
// COPYRIGHT file distributed with this source distribution.
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using Do.Universe;

namespace Do.Core {

	/// <summary>
	/// HistogramRelevanceProvider maintains item and action relevance using
	/// a histogram of "hit values."
	/// </summary>
	[Serializable]
	class HistogramRelevanceProvider : RelevanceProvider {

		DateTime oldest_hit;
		uint max_item_hits, max_action_hits;
		Dictionary<string, RelevanceRecord> hits;

		public HistogramRelevanceProvider ()
		{
			oldest_hit = DateTime.Now;
			max_item_hits = max_action_hits = 1;
			hits = new Dictionary<string, RelevanceRecord> ();
		}
		
		void UpdateMaxHits (RelevanceRecord rec)
		{
			if (rec.IsAction)
				max_action_hits = Math.Max (max_action_hits, rec.Hits);
			else
				max_item_hits = Math.Max (max_item_hits, rec.Hits);
		}

		public override void IncreaseRelevance (DoObject o, string match, DoObject other)
		{
			RelevanceRecord rec;
			
			if (!hits.TryGetValue (o.UID, out rec)) {
				rec = new RelevanceRecord (o);
				hits [o.UID] = rec;
			}

			if (other == null) rec.FirstPaneHits++;
			rec.Hits++;
			rec.LastHit = DateTime.Now;
			if (match.Length > 0)
				rec.AddFirstChar (match [0]);
			UpdateMaxHits (rec);
		}

		public override void DecreaseRelevance (DoObject o, string match, DoObject other)
		{
			RelevanceRecord rec;
			
			if (hits.TryGetValue (o.UID, out rec)) {
				if (other == null) rec.FirstPaneHits--;
				rec.Hits--;
				if (rec.Hits == 0)
					hits.Remove (o.UID);
			}
		}

		public override float GetRelevance (DoObject o, string match, DoObject other)
		{
			RelevanceRecord rec;
			float relevance, score;
			bool usedInFirstPaneOften = false;

			if (!hits.TryGetValue (o.UID, out rec))
				rec = new RelevanceRecord (o);
			
			// Get string similarity score.
			score = StringScoreForAbbreviation (o.Name, match);
			if (score == 0) return 0;
			
			relevance = 0f;	
			if (0 < rec.Hits) {
				float age;

				// On a scale of 0 to 1, how old is the item?
				age = 1 -
					(float) (DateTime.Now - rec.LastHit).TotalSeconds /
					(float) (DateTime.Now - oldest_hit).TotalSeconds;
					
				// Relevance is non-zero only if the record contains first char
				// relevance for the item.
				if (match.Length == 0 || rec.HasFirstChar (match [0]))
					relevance = (float) rec.Hits / 
						(float) (rec.IsAction ? max_action_hits : max_item_hits);
				else
					relevance = 0f;
				
				relevance *= 0.5f * (1f + age);
			}
			
			// Penalize actions that require modifier items.
			// other != null ==> we're getting relevance for second pane.
			if ((o is IAction) && 
			    (o as IAction).SupportedModifierItemTypes.Any () &&
			    !(o as IAction).ModifierItemsOptional)
				relevance -= 1.0f;

			// We penalize actions, but only if they're not used in the first pane often.
			if (o is IAction && rec.FirstPaneHits < 3)
				relevance -= 0.5f;

			// Penalize item sources so that items are preferred.
			if (o.Inner is IItemSource)
				relevance -= 1.0f;

			// Give the most popular actions a little leg up in the second pane.
			if (other != null && (
			    o.Inner is OpenAction ||
			    o.Inner is OpenURLAction ||
			    o.Inner is RunAction ||
			    o.Inner is EmailAction))
				relevance += 1.0f;

			if (o.Inner is AliasAction ||
				o.Inner is DeleteAliasAction ||
				o.Inner is CopyToClipboard)
				relevance -= 1.0f;
			
			return BalanceRelevanceWithScore (relevance, score);
		}

		float BalanceRelevanceWithScore (float rel, float score)
		{
			return (rel * .30f) + (score * .70f);
		}
	}
	
	/// <summary>
	/// RelevanceRecord keeps track of how often an item or action has been
	/// deemed relevant (Hits) and the last time relevance was increased
	/// (LastHit).
	/// </summary>
	[Serializable]
	class RelevanceRecord {

		public uint Hits;
		public uint FirstPaneHits;

		public bool IsAction;
		public DateTime LastHit;
		public string FirstChars;
		
		public RelevanceRecord (IObject o)
		{
			LastHit = DateTime.Now;
			IsAction = o is IAction;
			FirstChars = string.Empty;
		}
		
		/// <summary>
		/// Add a character as a valid first keypress in a search for the item.
		/// Searching for "Pidgin Internet Messenger" with the query "pid" will
		/// result in 'p' being added to FirstChars for the RelevanceRecord for
		/// "Pidgin Internet Messenger".
		/// </summary>
		/// <param name="c">
		/// A <see cref="System.Char"/> to add as a first keypress.
		/// </param>
		public void AddFirstChar (char c)
		{
			if (!FirstChars.Contains (c.ToString ().ToLower ()))
			    FirstChars += c.ToString ().ToLower ();
		}
		
		/// <summary>
		/// The opposite of AddFirstChar.
		/// </summary>
		/// <param name="c">
		/// A <see cref="System.Char"/> to remove from FirstChars.
		/// </param>
		public void RemoveFirstChar (char c)
		{
			FirstChars = FirstChars.Replace (c.ToString ().ToLower (), string.Empty);
		}
		
		/// <summary>
		/// Whether record has a given first character.
		/// </summary>
		/// <param name="c">
		/// A <see cref="System.Char"/> to look for in FirstChars.
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/> indicating whether record has a
		/// given first character.
		/// </returns>
		public bool HasFirstChar (char c)
		{
			return FirstChars.Contains (c.ToString ().ToLower ());
		}
	}
}
