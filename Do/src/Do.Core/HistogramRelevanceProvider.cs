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

		const float DefaultRelevance = 0.01f;
		const float DefaultAge = 1f;

		static readonly IEnumerable<Type> RewardedItemTypes = new Type[] {
			typeof (ApplicationItem),
		};

		static readonly IEnumerable<Type> RewardedActionTypes = new Type[] {
			typeof (OpenAction),
			typeof (OpenURLAction),
			typeof (RunAction),
			typeof (EmailAction),
		};

		static readonly IEnumerable<Type> PenalizedActionTypes = new Type[] {
			typeof (AliasAction),
			typeof (DeleteAliasAction),
			typeof (CopyToClipboardAction),
		};

		DateTime newest_hit, oldest_hit;
		uint max_item_hits, max_action_hits;
		Dictionary<string, RelevanceRecord> hits;

		public HistogramRelevanceProvider ()
		{
			oldest_hit = newest_hit = DateTime.Now;
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

			newest_hit = DateTime.Now;
			if (!hits.TryGetValue (o.UID, out rec)) {
				rec = new RelevanceRecord (o);
				hits [o.UID] = rec;
			}
			
			rec.Hits++;
			rec.LastHit = DateTime.Now;
			if (other == null) rec.FirstPaneHits++;
			if (0 < match.Length)
				rec.AddFirstChar (match [0]);
			UpdateMaxHits (rec);
		}

		public override void DecreaseRelevance (DoObject o, string match, DoObject other)
		{
			RelevanceRecord rec;
			
			if (hits.TryGetValue (o.UID, out rec)) {
				rec.Hits--;
				if (other == null) rec.FirstPaneHits--;
				if (rec.Hits == 0) 	hits.Remove (o.UID);
			}
		}

		public override float GetRelevance (DoObject o, string match, DoObject other)
		{
			RelevanceRecord rec;
			bool isAction;
			float relevance = 0f, age = 0f, score = 0f;

			if (!hits.TryGetValue (o.UID, out rec))
				rec = new RelevanceRecord (o);

			isAction = rec.IsAction;
			
			// Get string similarity score.
			score = StringScoreForAbbreviation (o.Name, match);
			if (score == 0f) return 0f;
			
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

				// We must give a base, non-zero relevance to make scoring rules take
				// effect. We divide by length so that if two objects have default
				// relevance, the object with the shorter name comes first. Objects
				// with shorter names tend to be simpler, and more often what the
				// user wants (e.g. "Jay-Z" vs "Jay-Z feat. The Roots").
				relevance = DefaultRelevance / Math.Max (1, o.Name.Length);

				// Give the most popular actions a little leg up in the second pane.
				if (isAction && other != null && RewardedActionTypes.Contains (o.Inner.GetType ()))
					relevance = 1f;
				// Give the most popular actions a little leg up in the second pane.
				else if (RewardedItemTypes.Contains (o.Inner.GetType ()))
					relevance = 1f;
			}

			// Newer objects (age -> 0) get scaled by factor -> 1.
			// Older objects (age -> 1) get scaled by factor -> .5.
			relevance *= 1f - (age / 2f);

			if (isAction) {
				IAction oa = o as IAction;
				// We penalize actions, but only if they're not used in the first pane
				// often.
				if (rec.FirstPaneHits < 3)
					relevance *= 0.8f;

				// Penalize actions that require modifier items.
				if (!oa.ModifierItemsOptional)
					relevance *= 0.8f;
			}

			if (o.Inner is IItemSource)
				relevance *= 0.4f;

			if (PenalizedActionTypes.Contains (o.Inner.GetType ()))
				relevance *= 0.8f;

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

		public Type Type;
		public DateTime LastHit;
		public string FirstChars;
		
		public RelevanceRecord (IObject o)
		{
			LastHit = DateTime.Now;
			Type = o.GetType ();
			FirstChars = string.Empty;
		}

		public bool IsAction {
			get {
				return typeof (IAction).IsAssignableFrom (Type);
			}
		}

		public bool IsRelevantForMatch (string match)
		{
			return null == match || match.Length == 0 || HasFirstChar (match [0]);
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
