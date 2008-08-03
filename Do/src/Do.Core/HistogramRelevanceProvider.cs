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
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using Do.Universe;

namespace Do.Core {

	/// <summary>
	/// HistogramRelevanceProvider maintains item and action relevance using
	/// a histogram of "hit values."
	/// </summary>
	sealed class HistogramRelevanceProvider : RelevanceProvider {
		
		uint max_item_hits, max_action_hits;
		DateTime oldest_hit;
		Dictionary<string, RelevanceRecord> hits;
		const int SerializeInterval = 15*60;

		public HistogramRelevanceProvider ()
		{
			max_item_hits = max_action_hits = 1;
			oldest_hit = DateTime.Now;
			hits = new Dictionary<string, RelevanceRecord> ();

			Deserialize ();
			
			foreach (RelevanceRecord rec in hits.Values) {
				UpdateMaxHits (rec);
				oldest_hit = oldest_hit.CompareTo (rec.LastHit) < 0 ?
					oldest_hit : rec.LastHit;
			}
			GLib.Timeout.Add (SerializeInterval*1000, OnSerializeTimer);
		}
		
		void UpdateMaxHits (RelevanceRecord rec)
		{
			if (rec.IsAction)
				max_action_hits = Math.Max (max_action_hits, rec.Hits);
			else
				max_item_hits = Math.Max (max_item_hits, rec.Hits);
		}

		/// <value>
		/// Path of file where relevance data is serialized.
		/// </value>
		private string RelevanceFile {
			get {
				return Paths.Combine (Paths.ApplicationData, "relevance5");
			}
		}

		/// <summary>
		/// Serialize timer target.
		/// </summary>
		private bool OnSerializeTimer ()
		{
			Gtk.Application.Invoke (
			    delegate {
				    Serialize ();
			    }
			);
			return true;
		}

		/// <summary>
		/// Deserializes relevance data.
		/// </summary>
		private void Deserialize ()
		{
			try {
				using (Stream s = File.OpenRead (RelevanceFile)) {
					BinaryFormatter f = new BinaryFormatter ();
					hits = f.Deserialize (s) as Dictionary<string, RelevanceRecord>;
				}
				Log.Debug ("Successfully loaded learned usage data.");
			} catch (FileNotFoundException) {
			} catch (Exception e) {
				Log.Error ("Failed to load learned usage data: {0}", e.Message);
			}
		}

		/// <summary>
		/// Serializes relevance data.
		/// </summary>
		private void Serialize ()
		{
			try {
				using (Stream s = File.OpenWrite (RelevanceFile)) {
					BinaryFormatter f = new BinaryFormatter ();
					f.Serialize (s, hits);
				}
				Log.Debug ("Successfully saved learned usage data.");
			} catch (Exception e) {
				Log.Error ("Failed to save learned usage data: {0}", e.Message);
			}
		}

		public override void IncreaseRelevance (DoObject o,
												string match,
												DoObject other)
		{
			RelevanceRecord rec;
			if (!hits.TryGetValue (o.UID, out rec)) {
				rec = new RelevanceRecord (o);
				hits [o.UID] = rec;
			}
			rec.Hits++;
			rec.LastHit = DateTime.Now;
			if (match.Length > 0)
				rec.AddFirstChar (match [0]);
			UpdateMaxHits (rec);
		}

		public override void DecreaseRelevance (DoObject o,
												string match,
												DoObject other)
		{
			RelevanceRecord rec;
			
			if (hits.TryGetValue (o.UID, out rec))
				rec.Hits--;
		}

		public override float GetRelevance (DoObject o,
											string match,
											DoObject other)
		{
			// These should all be between 0 and 1.
			float relevance, score;
			
			relevance = 0f;			
			// Get string similarity score.
			score = StringScoreForAbbreviation (o.Name, match);
			if (score == 0) return 0;
			
			if (hits.ContainsKey (o.UID)) {
				float age;
				RelevanceRecord rec = hits [o.UID];
	
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
			if (o is IAction &&
			    (o as IAction).SupportedModifierItemTypes.Length > 0)
				relevance -= 0.1f;
			// Penalize item sources so that items are preferred.
			if (o.Inner is IItemSource)
				relevance -= 0.1f;
			// Give the most popular actions a little leg up.
			if (o.Inner is OpenAction ||
			    o.Inner is OpenURLAction ||
			    o.Inner is RunAction ||
			    o.Inner is EmailAction)
				relevance += 0.1f;
			if (o.Inner is AliasAction ||
				o.Inner is DeleteAliasAction ||
				o.Inner is CopyToClipboard)
				relevance = -0.1f;
			
			return BalanceRelevanceWithScore (o, relevance, score);
		}

		float BalanceRelevanceWithScore (IObject o, float rel, float score)
		{
			float reward;
		   
			reward = o is IItem ? .1f : 0f;
			return reward +
				   rel    * .20f +
				   score  * .70f;
		}
	}
	
	/// <summary>
	/// RelevanceRecord keeps track of how often an item or action has been
	/// deemed relevant (Hits) and the last time relevance was increased
	/// (LastHit).
	/// </summary>
	[Serializable]
	class RelevanceRecord {
		public DateTime LastHit;
		public uint Hits;
		public string FirstChars;
		public bool IsAction;
		
		public RelevanceRecord (IObject o)
		{
			LastHit = DateTime.Now;
			FirstChars = string.Empty;
			IsAction = o is IAction;
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
			if (!FirstChars.Contains (c.ToString ().ToLower ()))
			    return;
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
