/* RelevanceProvider.cs
 *
 * GNOME Do is the legal property of its developers. Please refer to the
 * COPYRIGHT file distributed with this
 * inner distribution.
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
using System.Collections.Generic;

using System.IO;
using System.Threading;

namespace Do.Core
{
	
	interface IRelevanceProvider {
		void Increase (DoObject r);
		void Decrease (DoObject r);
		int GetRelevance (DoObject r);
	}
	
	static class RelevanceProvider {

		public static IRelevanceProvider GetProvider ()
		{
			return new HistogramRelevanceProvider ();
		}

	}
	
	class HistogramRelevanceProvider : IRelevanceProvider {
		
		int maxHits;
		Dictionary<int, int> tokenHits;
		
		Timer serializeTimer;
		const int SerializeInterval = 3*60;
		
		public HistogramRelevanceProvider ()
		{
			maxHits = 0;
			tokenHits = new Dictionary<int,int> ();

			Deserialize ();
			
			// Serialize every few minutes.
			serializeTimer = new Timer (OnSerializeTimer);
			serializeTimer.Change (SerializeInterval*1000, SerializeInterval*1000);
		}
		
		protected string DB {
			get {
				return "~/.do/relevance1".Replace ("~",
					Environment.GetFolderPath (Environment.SpecialFolder.Personal));
			}
		}
		
		private void OnSerializeTimer (object state)
		{
			Serialize ();
		}
			
		protected void Deserialize ()
		{
			lock (tokenHits) {
				try {
					Log.Info ("Deserializing HistogramRelevanceProvider...");
					string[] parts;
					int	key, value;
					foreach (string line in File.ReadAllLines (DB)) {
						try {
							parts = line.Split ('\t');
							key = int.Parse (parts[0]);
							value = int.Parse (parts[1]);
							tokenHits[key] = value;
							maxHits = Math.Max (maxHits, value);
						} catch {
							continue;
						}
					}
					Log.Info ("Successfully deserialized HistogramRelevanceProvider.");
				} catch (Exception e) {
					Log.Error ("Deserializing HistogramRelevanceProvider failed: {0}", e.Message);
				}
			}
		}
		
		protected void Serialize ()
		{
			lock (tokenHits) {		
				try {
					Log.Info ("Serializing HistogramRelevanceProvider...");
					using (StreamWriter writer = new StreamWriter (DB)) {
						foreach (KeyValuePair<int, int> kvp in tokenHits) {
							writer.WriteLine (string.Format ("{0}\t{1}", kvp.Key, kvp.Value));
						}
					}
					Log.Info ("Successfully serialized HistogramRelevanceProvider.");
				} catch (Exception e) {
					Log.Error ("Serializing HistogramRelevanceProvider failed: {0}", e.Message);
				}
			}
		}
		
		public virtual void Increase (DoObject r)
		{
			int rel;
			int key = r.GetHashCode ();
			
			if (r is DoTextItem) return;

			lock (tokenHits) {
				tokenHits.TryGetValue (key, out rel);
				rel = rel + 1;
				tokenHits[key] = rel;
				maxHits = Math.Max (maxHits, rel);
			}
		}
		
		public virtual void Decrease (DoObject r)
		{
			int rel;
			int key = r.GetHashCode ();
			
			if (r is DoTextItem) return;

			lock (tokenHits) {
				tokenHits.TryGetValue (key, out rel);
				rel = rel - 1;
				if (rel == 0) {
					tokenHits.Remove (key);
				} else {
					tokenHits[key] = rel;
				}
			}
		}
		
		public virtual int GetRelevance (DoObject r)
		{
			int rel;
			int key = r.GetHashCode ();
			
			lock (tokenHits) {
				tokenHits.TryGetValue (key, out rel);
			}
			if (rel != 0) {
				rel = (int) (100 * ((double) rel / (double) maxHits));
			}
			return rel;
		}
	}
}
