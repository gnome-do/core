/* HistogramRelevanceProvider.cs
 *
 * GNOME Do is the legal property of its developers. Please refer to the
 * COPYRIGHT file distributed with this source distribution.
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
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using Do.Universe;

namespace Do.Core {

	/// <summary>
	/// HistogramRelevanceProvider maintains item and action relevance using
	/// a histogram of "hit values."
	/// </summary>
	class HistogramRelevanceProvider : RelevanceProvider {
		
		uint max_hits;
		Dictionary<string, RelevanceRecord> hits;

		Timer serializeTimer;
		const int SerializeInterval = 15*60;

		public HistogramRelevanceProvider ()
		{
			max_hits = 1;
			hits = new Dictionary<string, RelevanceRecord> ();

			Deserialize ();
			
			foreach (RelevanceRecord rec in hits.Values)
				max_hits = Math.Max (max_hits, rec.Hits);

			// Serialize every few minutes.
			serializeTimer = new Timer (OnSerializeTimer);
			serializeTimer.Change (SerializeInterval*1000, SerializeInterval*1000);
		}

		/// <value>
		/// Path of file where relevance data is serialized.
		/// </value>
		protected string RelevanceFile {
			get {
				return Paths.Combine (Paths.ApplicationData, "relevance4");
			}
		}

		/// <summary>
		/// Serialize timer target.
		/// </summary>
		private void OnSerializeTimer (object state)
		{
			Gtk.Application.Invoke (
			    delegate {
				    Serialize ();
			    }
			);
		}

		/// <summary>
		/// Deserializes relevance data.
		/// </summary>
		protected void Deserialize ()
		{
			try {
				using (Stream s = File.OpenRead (RelevanceFile)) {
					BinaryFormatter f = new BinaryFormatter ();
					hits = f.Deserialize (s) as Dictionary<string, RelevanceRecord>;
				}
				Log.Debug ("Successfully deserialized Histogram Relevance Provider.");
			} catch (Exception e) {
				Log.Error ("Deserializing Histogram Relevance Provider failed: {0}", e.Message);
			}
		}

		/// <summary>
		/// Serializes relevance data.
		/// </summary>
		protected void Serialize ()
		{
			try {
				using (Stream s = File.OpenWrite (RelevanceFile)) {
					BinaryFormatter f = new BinaryFormatter ();
					f.Serialize (s, hits);
				}
				Log.Debug ("Successfully serialized Histogram Relevance Provider.");
			} catch (Exception e) {
				Log.Error ("Serializing Histogram Relevance Provider failed: {0}", e.Message);
			}
		}

		public override void IncreaseRelevance (DoObject r, string match, DoObject other)
		{
			RelevanceRecord rec;
			
			if (!hits.TryGetValue (r.UID, out rec)) {
				rec = new RelevanceRecord ();
				hits [r.UID] = rec;
			}
			rec.Hits++;
			rec.LastHit = DateTime.Now;
			if (match.Length > 0)
				rec.AddFirstChar (match [0]);	
			max_hits = Math.Max (max_hits, rec.Hits);
		}

		public override void DecreaseRelevance (DoObject r, string match, DoObject other)
		{
			RelevanceRecord rec;
			
			if (!hits.TryGetValue (r.UID, out rec))
				return;
			rec.Hits--;
		}

		public override float GetRelevance (DoObject r, string match, DoObject other)
		{
			// These should all be between 0 and 1.
			float relevance, score, itemReward;
			
			relevance = itemReward = 0f;			
			// Get string similarity score. Return immediately if 0.
			score = RelevanceProvider.StringScoreForAbbreviation (r.Name, match);
			if (score == 0) return 0;
			
			if (hits.ContainsKey (r.UID)) {
				RelevanceRecord rec = hits [r.UID];
				// If the item is old, decrease its hits. 
				if (DateTime.Now - rec.LastHit > TimeSpan.FromDays (30)) {
					rec.Hits /= 2;
					rec.LastHit = DateTime.Now;
					if (rec.Hits == 0)
						hits.Remove (r.UID);
				}
				
				// Relevance is non-zero only if the record contains first char
				// relevance for the item.
				if (match.Length > 0 && rec.HasFirstChar (match [0]))
					relevance = (float) rec.Hits / (float) max_hits;
				else
					relevance = 0f;
		    } else {
				relevance = 0f;
			}
			
			// Penalize actions that require modifier items.
			// other != null ==> we're getting relevance for second pane.
			if (other != null && r is IAction &&
			    (r as IAction).SupportedModifierItemTypes.Length > 0)
				relevance -= 0.1f;
			// Penalize item sources so that items are preferred.
			if (r.Inner is IItemSource)
				relevance -= 0.1f;
			// Give the most popular actions a little leg up.
			if (r.Inner is OpenAction ||
			    r.Inner is OpenURLAction ||
			    r.Inner is RunAction)
				relevance += 0.1f;
			
			itemReward = r is IItem ? 1.0f : 0f;
				
			return itemReward * .10f +
				relevance  * .20f +
				score      * .70f;
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
		
		public RelevanceRecord ()
		{
			LastHit = DateTime.Now;
			Hits = 0;
			FirstChars = "";
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
