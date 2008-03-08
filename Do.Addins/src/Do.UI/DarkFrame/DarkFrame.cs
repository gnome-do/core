// DarkFrame.cs
//
//GNOME Do is the legal property of its developers. Please refer to the
//COPYRIGHT file distributed with this
//source distribution.
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
//
//

using System;
using Mono.Unix;

using Do.Universe;
using Do.Addins;
using Gdk;
using Gtk;

namespace Do.UI
{
	
	
	public class DarkFrame : MiniWindow
	{
		new const int IconBoxIconSize = 64;
		new const uint IconBoxPadding = 2;
		new const int IconBoxRadius = 3;
		
		protected int frameoffset;
		protected new const int MainRadius = 10;
		protected new GlassFrame frame;
		
		public DarkFrame (IDoController controller) : base (controller)
		{
		}
		
		protected override void Build ()
		{
			VBox      vbox;
			Alignment align;

			AppPaintable = true;
			KeepAbove = true;
			Decorated = false;
			// This typehint gets the window to raise all the way to top.
			TypeHint = WindowTypeHint.Splashscreen;

			try {
				SetIconFromFile ("/usr/share/icons/gnome/scalable/actions/system-run.svg");
			} catch { }
			SetColormap ();

			resultsWindow = new ResultsWindow (new Color(15, 15, 15));
			resultsWindow.SelectionChanged += OnResultsWindowSelectionChanged;

			currentPane = Pane.First;

			frameoffset = 10;
			frame = new GlassFrame (frameoffset);
			frame.DrawFill = frame.DrawFrame = true;
			frame.FillColor = new Color(15, 15, 15);
			frame.FillAlpha = WindowTransparency;
			frame.FrameColor = new Color(125, 125, 135);
			frame.FrameAlpha = .35;
			frame.Radius = Screen.IsComposited ? MainRadius : 0;
			Add (frame);
			frame.Show ();

			vbox = new VBox (false, 0);
			frame.Add (vbox);
			vbox.BorderWidth = (uint) (IconBoxPadding + frameoffset);
			vbox.Show ();

			resultsHBox = new HBox (false, (int) IconBoxPadding * 2);
			resultsHBox.BorderWidth = IconBoxPadding;
			vbox.PackStart (resultsHBox, false, false, 0);
			resultsHBox.Show ();

			iconbox = new MiniIconBox[3];

			iconbox[0] = new MiniIconBox (IconBoxIconSize);
			iconbox[0].IsFocused = true;
			iconbox[0].Radius = IconBoxRadius;
			resultsHBox.PackStart (iconbox[0], false, false, 0);
			iconbox[0].Show ();

			iconbox[1] = new MiniIconBox (IconBoxIconSize);
			iconbox[1].IsFocused = false;
			iconbox[1].Radius = IconBoxRadius;
			resultsHBox.PackStart (iconbox[1], false, false, 0);
			iconbox[1].Show ();

			iconbox[2] = new MiniIconBox (IconBoxIconSize);
			iconbox[2].IsFocused = false;
			iconbox[2].Radius = IconBoxRadius;
			resultsHBox.PackStart (iconbox[2], false, false, 0);
			// iconbox[2].Show ();

			align = new Alignment (0.5F, 0.5F, 1, 1);
			align.SetPadding (0, 2, 0, 0);
			label = new SymbolDisplayLabel ();
			align.Add (label);
			vbox.PackStart (align, false, false, 0);
			//label.Show ();
			//align.Show ();

			ScreenChanged += OnScreenChanged;
			ConfigureEvent += OnConfigureEvent;
			
			summonable = true;

			Reposition ();
		}
		
		protected override bool OnMotionNotifyEvent (EventMotion evnt)
		{
			//SLOW, fix by permenant storage
			int end_x, end_y, start_x, start_y;
			int point_x, point_y;

			GetPosition (out start_x, out start_y);
			GetSize (out end_x, out end_y);
			
			end_x += start_x;
			end_y += start_y;
			
			point_x = (int) evnt.XRoot;
			point_y = (int) evnt.YRoot;
			
			if ((end_x - 30 <= point_x) && (point_x < end_x - 10) && 
			    (start_y <= point_y) && (point_y < start_y + 15)) {
				if (!frame.DrawArrow)
					frame.DrawArrow = true;
			} else {
				if (frame.DrawArrow)
					frame.DrawArrow = false;
			}
			
			return base.OnMotionNotifyEvent (evnt);
		}

		
		protected override bool OnButtonPressEvent (EventButton evnt)
		{
			int start_x, start_y, end_x, end_y;
			int click_x, click_y;
			bool click_on_window, click_near_settings_icon;

			GetPosition (out start_x, out start_y);
			GetSize (out end_x, out end_y);
			end_x += start_x;
			end_y += start_y;
			click_x = (int) evnt.XRoot;
			click_y = (int) evnt.YRoot;
			click_on_window = start_x <= click_x && click_x < end_x &&
			                  start_y <= click_y && click_y < end_y;
			click_near_settings_icon = (((end_x - 30) <= click_x) && (click_x < end_x - 10) && 
			                            (start_y <= click_y) && (click_y < (start_y + 15)));
			if (click_near_settings_icon) {
				Addins.Util.Appearance.PopupMainMenuAtPosition (end_x - 21, start_y + 12);
				// Have to re-grab the pane from the menu.
				Addins.Util.Appearance.PresentWindow (this);
				frame.DrawArrow = false;
			} else if (!click_on_window) {
				controller.ButtonPressOffWindow ();
			}
			//what do true/false mean here?
			return true;
		}
		
		public override void Reposition ()
		{
			int monitor;
			Gdk.Rectangle geo, main, results;
			
			GetPosition (out main.X, out main.Y);
			GetSize (out main.Width, out main.Height);
			monitor = Screen.GetMonitorAtPoint (main.X, main.Y);
			geo = Screen.GetMonitorGeometry (monitor);
			main.X = (geo.Width - main.Width) / 2;
			main.Y = (int)((geo.Height - main.Height) / 2.5);
			Move (main.X, main.Y);

			resultsWindow.GetSize (out results.Width, out results.Height);
			results.Y = main.Y + main.Height - frameoffset;
			results.X = main.X + (((iconbox[0].Width) + ((int) IconBoxPadding * 2)) * 
			                      (int) currentPane + MainRadius) + frameoffset;
			resultsWindow.Move (results.X, results.Y);
		}
	}
}
