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

namespace Do.Core
{
	class RelevanceProvider {

		public static RelevanceProvider GetProvider ()
		{
			return new HistogramRelevanceProvider ();
		}

		// Quicksilver algorithm.
		// http://docs.blacktree.com/quicksilver/development/string_ranking?DokuWiki=10df5a965790f5b8cc9ef63be6614516
		public static float StringScoreForAbbreviation (string s, string ab)
		{
			return StringScoreForAbbreviationInRanges (s, ab,
			                                           new int[] {0, s.Length},
			                                           new int[] {0, ab.Length});
		}

		protected static float
		StringScoreForAbbreviationInRanges (string s, string ab, int[] s_range, int[] ab_range)
		{
			float score, remainingScore;
			int i, j;
			int[] remainingSearchRange = {0, 0};

			if (ab_range[1] == 0) return 0.9F;
			if (ab_range[1] > s_range[1]) return 0.0F;
			for (i = ab_range[1]; i > 0; i--) {
				// Search for steadily smaller portions of the abbreviation.
				// TODO Turn this into a dynamic algorithm.
				string ab_substring = ab.Substring (ab_range[0], i);
				string s_substring = s.Substring (s_range[0], s_range[1]);
				int loc = s_substring.IndexOf (ab_substring, StringComparison.CurrentCultureIgnoreCase);
				if (loc < 0) continue;
				remainingSearchRange[0] = loc + i;
				remainingSearchRange[1] = s_range[1] - remainingSearchRange[0];
				remainingScore = StringScoreForAbbreviationInRanges (s, ab,
				                                                     remainingSearchRange,
				                                                     new int[] {ab_range[0]+i, ab_range[1]-i});
				if (remainingScore != 0) {
					score = remainingSearchRange[0] - s_range[0];
					if (loc > s_range[0]) {
						// If some letters were skipped.
						if (s[loc-1] == ' ') {
							for (j = loc-2; j >= s_range[0]; j--) {
								if (s[j] == ' ')
									score--;
								else
									score -= 0.15F;
							}
						}
						// Else if word is uppercase (?)
						else if (s[loc] >= 'A') {
							for (j = loc-1; j >= s_range[0]; j--) {
								if (s[j] >= 'A')
									score--;
								else
									score -= 0.15F;
							}
						}
						else {
							score -= loc - s_range[0];
						}
					}
					score += remainingScore * remainingSearchRange[1];
					score /= s_range[1];
					return score;
				}
			}
			return 0;
		}

		static bool IObjectBelongsInFirstResultsForKeypress (IObject o, char a)
		{
			return LetterOccursAfterDelimiter (o.Name.ToLower (), char.ToLower (a));
		}

		static bool LetterOccursAfterDelimiter (string s, char a)
		{
			int idx;

			idx = 0;
			while (idx < s.Length && (idx = s.IndexOf (a, idx)) > -1) {
				if (idx == 0 ||
					(idx > 0 && s[idx-1] == ' ')) {
					return true;
				}
				idx++;
			}
			return false;
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
			return IObjectBelongsInFirstResultsForKeypress (r, a);
		}
	}
}
