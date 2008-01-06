/* DefineWordCommand.cs
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
using System.Text.RegularExpressions;
using Mono.Unix;

namespace Do.Universe
{
	/// <summary>
	/// Given an ITextItem, DefineWordCommand will look up the Text
	/// contents of the ITextItem using the gnome-dictionary.
	/// </summary>
	public class DefineWordCommand : ICommand
	{
		/// <summary>
		/// Should match those and only those strings that can be
		/// looked up in a dictionary.
		/// YES: "war", "peace", "hoi polloi"
		/// NO: "war9", "2 + 4", "___1337__"
		/// </summary>
		const string wordPattern = @"^([^\W0-9_]+([ -][^\W0-9_]+)?)$";

		Regex wordRegex;
		
		public DefineWordCommand ()
		{
			wordRegex = new Regex (wordPattern, RegexOptions.Compiled);
		}
		
		public string Name {
			get { return Catalog.GetString ("Define"); }
		}
		
		public string Description
		{
			get { return Catalog.GetString ("Define a given word."); }
		}
		
		public string Icon
		{
			get { return "accessories-dictionary.png"; }
		}
		
		public Type[] SupportedItemTypes
		{
			get {
				return new Type[] {
					typeof (ITextItem),
				};
			}
		}
		
		public Type[] SupportedModifierItemTypes
		{
			get { return null; }
		}

		/// <summary>
		/// Use wordRegex to determine whether item is definable.
		/// </summary>
		/// <param name="item">
		/// A <see cref="IItem"/> to define.
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/> indicating whether or not IITem
		/// can be defined.
		/// </returns>
		public bool SupportsItem (IItem item)
		{
			string word;

			word = null;
			if (item is ITextItem) {
				word = (item as ITextItem).Text;
			}

			if (word != null) {
				return (wordRegex.IsMatch (word));
			}
			return false;
		}
		
		public bool SupportsModifierItemForItems (IItem[] items, IItem modItem)
		{
			return false;
		}
		
		public void Perform (IItem[] items, IItem[] modifierItems)
		{
			string word, cmd;
			foreach (IItem item in items) {
				if (item is ITextItem) {
					word = (item as ITextItem).Text;
				} else {
					continue;
				}

				cmd = string.Format ("gnome-dictionary --look-up \"{0}\"", word);
				try {
					System.Diagnostics.Process.Start (cmd);
				} catch (Exception e) {
					Console.WriteLine ("Failed to define word: \"{0}\"", e.Message);
				}
			}
		}
	}
}
