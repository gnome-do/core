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

using Do.Universe;

namespace Do.Core {

	class RelevanceProvider {

		public static RelevanceProvider GetProvider ()
		{
            return new HistogramRelevanceProvider ();
		}

		//Scores a string based on closeness to a query
		public static float StringScoreForAbbreviation (string s, string query)
		{
			if(query.Length == 0)
				return 1;
			
			float score;
			string ls = s.ToLower();
			string lquery = query.ToLower();

			//Find the shortest possible substring that matches the query
			//and get the ration of their lengths for a base score
			int[] match = findBestSubstringMatchIndices(ls, lquery);
			if ((match[1] - match[0]) == 0) return 0;
			score = query.Length / (float)(match[1] - match[0]);
			if (score == 0) return 0;
			
			//Bonus points if the characters start words
			float good = 0, bad = 1;
			int firstCount = 0;
			for(int i=match[0]; i<match[1]-1; i++)
			{
				if(s[i] == ' ')
				{
					if(lquery.Contains(ls[i+1].ToString()))
						firstCount++;
					else
						bad++;
				}
			}
						
			//A first character match counts extra
			if(lquery[0] == ls[0])
				firstCount += 2;
			
			//The longer the acronym, the better it scores
			good += firstCount*firstCount*4;
			
			//Better yet if the match itself started there
			if(match[0] == 0)
				good += 2;
			
			//Super bonus if the whole match is at the beginning
			if(match[1] == (query.Length - 1))
				good += match[1] + 4;
			
			//Super-duper bonus if it is a perfect match
			if(lquery == ls)
				good += match[1] * 2 + 4;			
			
			if(good+bad > 0)
				score = (score + 3*good/(good+bad)) / 4;
			
			
			return score;
		}

		//Finds the shortest substring of s that contains all the characters of query in order
		//If none is found, returns {-1, -1}
		protected static int[] findBestSubstringMatchIndices(string s, string query)
		{
			int index=-1;
			int[] bestMatch = {-1,-1};
			
			if(query.Length == 0) {
				int[] noQueryRet = {0,0};
				return noQueryRet;
			}
			
			//Loop through each instance of the first character in query
			while ((index = s.IndexOf(query[0], index+1)) >= 0) {
				//Is there even room for a match?
				if(index > s.Length - query.Length) break;
				
				//Look for the best match in the tail
				int cur = index;
				int qcur = 0;
				while(qcur < query.Length && cur < s.Length)
					if(query[qcur] == s[cur++])
						qcur++;
				
				if((qcur == query.Length) && (((cur - index) < (bestMatch[1] - bestMatch[0])) || (bestMatch[0] == -1))) {
					bestMatch[0] = index;
					bestMatch[1] = cur;
				}
				
				if(index == s.Length - 1)
					break;
			}
			
			return bestMatch;
		}

		public virtual void IncreaseRelevance (DoObject r, string match, DoObject other)
		{
		}

		public virtual void DecreaseRelevance (DoObject r, string match, DoObject other)
		{
		}

		public virtual float GetRelevance (DoObject r, string match, DoObject other)
		{
			return StringScoreForAbbreviation (r.Name, match);
		}

		public virtual bool CanBeFirstResultForKeypress (DoObject r, char a)
		{
			return true;
		}
	}
}
