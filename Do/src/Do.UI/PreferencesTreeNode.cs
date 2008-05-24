/* PreferencesTreeNode.cs
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

using Gtk;

namespace Do.UI
{
    [TreeNode (ListOnly=true)]
    public class PreferencesTreeNode : TreeNode, IEquatable<PreferencesTreeNode> {

		string label;
	
        public PreferencesTreeNode (string label)
        {
                this.label = label;
        }

        [TreeNodeValue (Column=0)]
        public string Label {
			get { return label; }
		}
		
		public bool Equals (PreferencesTreeNode x)
        {
        	return label.Equals (x.Label);
        }
		
		public override int GetHashCode ()
		{
			return label.GetHashCode ();
		}

    }
}