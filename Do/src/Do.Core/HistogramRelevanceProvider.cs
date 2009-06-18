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

using Do.Platform;
using Do.Universe;
using Do.Universe.Safe;
using Do.Universe.Common;

namespace Do.Core {

	/// <summary>
	/// HistogramRelevanceProvider maintains item and action relevance using
	/// a histogram of "hit values."
	/// </summary>
	[Serializable]
	class HistogramRelevanceProvider : RelevanceProvider {

		const float DefaultRelevance = 0.001f;
		const float DefaultAge = 1f;

		DateTime newest_hit, oldest_hit;
		uint max_item_hits, max_action_hits;
		Dictionary<string, RelevanceRecord> hits;

		public HistogramRelevanceProvider ()
		{
			oldest_hit = newest_hit = DateTime.Now;
			max_item_hits = max_action_hits = 1;
			hits = new Dictionary<string, RelevanceRecord> ();
		}
		
		void UpdateMaxHits (RelevanceRecord rec, Item e)
		{
			if (e.IsAction ())
				max_action_hits = Math.Max (max_action_hits, rec.Hits);
			else
				max_item_hits = Math.Max (max_item_hits, rec.Hits);
		}

		public override void IncreaseRelevance (Item element, string match, Item other)
		{
			RelevanceRecord rec;

			if (element == null) throw new ArgumentNullException ("element");

			match = match ?? "";
			newest_hit = DateTime.Now;
			if (!hits.TryGetValue (element.UniqueId, out rec)) {
				rec = new RelevanceRecord (element);
				hits [element.UniqueId] = rec;
			}
			
			rec.Hits++;
			rec.LastHit = DateTime.Now;
			if (other == null) rec.FirstPaneHits++;
			if (0 < match.Length)
				rec.AddFirstChar (match [0]);
			UpdateMaxHits (rec, element);
		}

		public override void DecreaseRelevance (Item element, string match, Item other)
		{
			RelevanceRecord rec;

			if (element == null) throw new ArgumentNullException ("element");

			match = match ?? "";
			if (hits.TryGetValue (element.UniqueId, out rec)) {
				rec.Hits--;
				if (other == null) rec.FirstPaneHits--;
				if (rec.Hits == 0) hits.Remove (element.UniqueId);
			}
		}

		public override float GetRelevance (Item e, string match, Item other)
		{
			RelevanceRecord rec;
			bool isAction;
			float relevance = 0f, age = 0f, score = 0f;
			string name = e.Safe.Name;

			if (!hits.TryGetValue (e.UniqueId, out rec))
				rec = new RelevanceRecord (e);

			isAction = e.IsAction ();
			
			// Get string similarity score.
			score = StringScoreForAbbreviation (name, match);
			if (score == 0f) return 0f;

			// Pin some actions to top.
			// TODO Remove this when relevance is refactored and improved.
			if (other != null &&
				(e is OpenAction || e is RunAction || e is OpenUrlAction))
				return 1f;
			
			// We must give a base, non-zero relevance to make scoring rules take
			// effect. We scale by length so that if two objects have default
			// relevance, the object with the shorter name comes first. Objects
			// with shorter names tend to be simpler, and more often what the
			// user wants (e.g. "Jay-Z" vs "Jay-Z feat. The Roots").
			relevance = DefaultRelevance / Math.Max (1, name.Length);

			if (0 < rec.Hits) {
				// On a scale of 0 (new) to 1 (old), how old is the item?
				age = (float) (newest_hit - rec.LastHit).TotalSeconds /
					  (float) (newest_hit - oldest_hit).TotalSeconds;
				
				if (rec.IsRelevantForMatch (match))
					relevance = (float) rec.Hits /
						(float) (isAction ? max_action_hits : max_item_hits);
			} else {
				// Objects we don't know about are treated as old.
				age = DefaultAge;

				// Give the most popular items a leg up
				if (typeof (IApplicationItem).IsInstanceOfType (e))
					relevance *= 2;
			}

			// Newer objects (age -> 0) get scaled by factor -> 1.
			// Older objects (age -> 1) get scaled by factor -> .5.
			relevance *= 1f - age / 2f;

			if (isAction) {
				SafeAct action = e.AsAction ().Safe;
				// We penalize actions, but only if they're not used in the first pane
				// often.
				if (rec.FirstPaneHits < 3)
					relevance *= 0.8f;

				// Penalize actions that require modifier items.
				if (!action.ModifierItemsOptional)
					relevance *= 0.8f;
			}

			if (typeof (ItemSourceItemSource.ItemSourceItem).IsInstanceOfType (e))
				relevance *= 0.4f;

			return relevance * 0.30f + score * 0.70f;
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

		public DateTime LastHit;
		public string FirstChars;
		
		public RelevanceRecord (Item o)
		{
			LastHit = DateTime.Now;
			FirstChars = "";
		}

		public bool IsRelevantForMatch (string match)
		{
			return string.IsNullOrEmpty (match) || HasFirstChar (match [0]);
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
			FirstChars = FirstChars.Replace (c.ToString ().ToLower (), "");
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
