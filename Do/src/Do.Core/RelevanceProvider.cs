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
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using Do.Platform;
using Do.Universe;

namespace Do.Core {

	public interface IRelevanceProvider
	{
		void IncreaseRelevance (Item target, string match, Item other);
		void DecreaseRelevance (Item target, string match, Item other);
		float GetRelevance (Item target, string match, Item other);
	}

	[Serializable]
	public abstract class RelevanceProvider : IRelevanceProvider {

		const int SerializeInterval = 10 * 60 * 1000;

		static RelevanceProvider ()
		{
			DefaultProvider = Deserialize () ?? new HistogramRelevanceProvider ();
			GLib.Timeout.Add (SerializeInterval, OnSerializeTimer);
		}
		
		public static IRelevanceProvider DefaultProvider { get; private set; }

		public static string RelevanceFile {
			get {
				return Path.Combine (Services.Paths.UserDataDirectory, "relevance8");
			}
		}

		static bool OnSerializeTimer () {
			Services.Application.RunOnMainThread (() => Serialize (DefaultProvider));
			return true;
		}

		/// <summary>
		/// Deserializes relevance data.
		/// </summary>
		private static IRelevanceProvider Deserialize ()
		{
			IRelevanceProvider provider = null;
			
			try {
				using (Stream s = File.OpenRead (RelevanceFile)) {
					BinaryFormatter f = new BinaryFormatter ();
					provider = f.Deserialize (s) as IRelevanceProvider;
				}
				Log<RelevanceProvider>.Debug ("Successfully loaded learned usage data.");
			} catch (FileNotFoundException) {
			} catch (Exception e) {
				Log<RelevanceProvider>.Error ("Failed to load learned usage data: {0}", e.Message);
			}
			return provider;
		}

		/// <summary>
		/// Serializes relevance data.
		/// </summary>
		internal static void Serialize (IRelevanceProvider provider)
		{
			try {
				using (Stream s = File.OpenWrite (RelevanceFile)) {
					BinaryFormatter f = new BinaryFormatter ();
					f.Serialize (s, provider);
				}
				Log<RelevanceProvider>.Debug ("Successfully saved learned usage data.");
			} catch (Exception e) {
				Log<RelevanceProvider>.Error ("Failed to save learned usage data: {0}", e.Message);
			}
		}

		/// <summary>
		/// Scores a string based on similarity to a query.  Bonuses are given for things like
		/// perfect matches and letters from the query starting words.
		/// </summary>
		/// <param name="s">
		/// A <see cref="System.String"/>
		/// String to be scored
		/// </param>
		/// <param name="query">
		/// A <see cref="System.String"/>
		/// Query to score against
		/// </param>
		/// <returns>
		/// A <see cref="System.Single"/>
		/// A relevancy score for the string ranging from 0 to 1
		/// </returns>
		public static float StringScoreForAbbreviation (string s, string query)
		{
			if(query.Length == 0)
				return 1;
			
			float score;
			string ls = s.ToLower ();

			//Find the shortest possible substring that matches the query
			//and get the ration of their lengths for a base score
			int[] match = findBestSubstringMatchIndices (ls, query);
			if ((match[1] - match[0]) == 0) return 0;
			score = query.Length / (float)(match[1] - match[0]);
			if (score == 0) return 0;
			
			//Now we weight by string length so shorter strings are better
			score = score * .7F + query.Length / s.Length * .3F;
			//Bonus points if the characters start words
			float good = 0, bad = 1;
			int firstCount = 0;
			for(int i=Math.Max (match[0]-1,0); i<match[1]-1; i++)
			{
				if (char.IsWhiteSpace (s[i]))
				{
					if (query.Contains (ls[i + 1].ToString ()))
						firstCount++;
					else
						bad++;
				}
			}
						
			//A first character match counts extra
			if(query[0] == ls[0])
				firstCount ++;
			
			//The longer the acronym, the better it scores
			good += firstCount * firstCount * 4;
			
			//Super-duper bonus if it is a perfect match
			if(query == ls)
				good += match[1] * 2 + 4;			
			
			if(good+bad > 0)
				score = (score + 3*good/(good+bad)) / 4;
			
			//This fix makes sure that perfect matches always rank higher
			//than split matches.  Perfect matches get the .9 - 1.0 range
			//everything else goes lower
			
			if(match[1] - match[0] == query.Length)
				score = .9f + .1f * score;
			else
				score = .9f * score;
			
			return score;
		}

		/// <summary>
		/// Finds the shortest substring of s that contains all the characters of query in order
		/// </summary>
		/// <param name="s">
		/// A <see cref="System.String"/>
		/// String to search
		/// </param>
		/// <param name="query">
		/// A <see cref="System.String"/>
		/// Query to search for
		/// </param>
		/// <returns>
		/// A <see cref="System.Int32"/>
		/// A two item array containing the start and end indices of the match.
		/// No match returns {-1.-1}
		/// </returns>
		protected static int[] findBestSubstringMatchIndices (string s, string query)
		{
			if (query.Length == 0)
				return new int[] {0, 0};
			
			int index = -1;
			int[] bestMatch = {-1, -1};
			
			//Find the last instance of the last character of the query
			//since we never need to search beyond that
			int lastChar = s.Length - 1;
			while (0 <= lastChar && s[lastChar] != query[query.Length - 1])
				lastChar--;
			
			//No instance of the character?
			if (lastChar == -1)
				return bestMatch;
			
			//Loop through each instance of the first character in query
			while ( 0 <= (index = s.IndexOf (query[0], index + 1, lastChar - index))) {
				//Is there even room for a match?
				if (index > lastChar + 1 - query.Length) break;
				
				//Look for the best match in the tail
				//We know the first char matches, so we dont check it.
				int cur  = index + 1;
				int qcur = 1;
				while (qcur < query.Length && cur < s.Length)
					if (query[qcur] == s[cur++])
						qcur++;
				
				if (qcur == query.Length && (cur - index < bestMatch[1] - bestMatch[0] || bestMatch[0] == -1)) {
					bestMatch[0] = index;
					bestMatch[1] = cur;
				}
				
				if (index == s.Length - 1)
					break;
			}
			
			return bestMatch;
		}

		public virtual void IncreaseRelevance (Item r, string match, Item other)
		{
		}

		public virtual void DecreaseRelevance (Item r, string match, Item other)
		{
		}

		public virtual float GetRelevance (Item r, string match, Item other)
		{
			return StringScoreForAbbreviation (r.Safe.Name, match);
		}
	}
}
