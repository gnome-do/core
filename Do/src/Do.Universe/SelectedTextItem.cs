/* SelectedSelectedTextItem.cs
 *
 * GNOME Do is the legal property of its developers. Please refer to the
 * COPYRIGHT file distributed with this
 * source distribution.
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
using System.Threading;
using System.Collections.Generic;

namespace Do.Universe
{

	public class SelectedTextItem : ITextItem
	{		
		public SelectedTextItem ()
		{
		}
		
		public string Name
		{
			get { return "Selected text"; }
		}
		
		public string Description
		{
			get { return "Currently selected text."; }
		}
		
		public string Icon
		{
			get { return "gtk-select-all"; }
		}
		
		public string Text
		{
			get {
				string text;
				
				// This causes a lot of trouble...
				/*
				Gtk.Clipboard primary;
			
				Console.WriteLine ("\nTrying to get clipboard text on thread {0}.", Thread.CurrentThread.Name);
				primary = Gtk.Clipboard.Get (Gdk.Selection.Primary);
				if (primary.WaitIsTextAvailable ()) {
					Console.WriteLine ("Text available. Waiting for text...");
					text = primary.WaitForText ();
					Console.WriteLine ("Got clipboard text \"{0}\"", text);
				} else {
					Console.WriteLine ("No clipboard text available.");
					text = "";
				}
				return text;
				*/
				
				System.Diagnostics.Process xclip;
				xclip = new System.Diagnostics.Process ();
				xclip.StartInfo.FileName = "xclip";
				xclip.StartInfo.Arguments = "-o";
				xclip.StartInfo.RedirectStandardOutput = true;
				xclip.StartInfo.UseShellExecute = false;
				try {
					xclip.Start ();
					xclip.WaitForExit ();
					text = xclip.StandardOutput.ReadToEnd ();
				} catch {
					Console.Error.WriteLine ("SelectedTextItem error: The program 'xclip' could not be found.");
					text = "";
				}
				return text;
			}
		}
	}
}
