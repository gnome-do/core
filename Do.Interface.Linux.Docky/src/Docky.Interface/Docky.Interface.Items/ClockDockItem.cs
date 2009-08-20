//  
//  Copyright (C) 2009 GNOME Do
// 
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
// 
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
// 

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Gdk;
using Gtk;
using Cairo;
using Mono.Unix;

using Do.Interface;
using Do.Interface.CairoUtils;
using Do.Platform;

using Docky.Core;
using Docky.Interface.Menus;
using Docky.Interface.Painters;

namespace Docky.Interface
{
	public class ClockDockItem : AbstractDockletItem, IRightClickable
	{
		int minute;
		CalendarPainter cal_painter;
		
		static IPreferences prefs = Services.Preferences.Get<ClockDockItem> ();
		
		bool show_military = prefs.Get<bool> ("ShowMilitary", false);
		bool ShowMilitary {
			get { return show_military; }
			set {
				show_military = value;
				prefs.Set<bool> ("ShowMilitary", value);
			}
		}
		
		bool digital = prefs.Get<bool> ("ShowDigital", false);
		bool ShowDigital {
			get { return digital; }
			set {
				digital = value;
				prefs.Set<bool> ("ShowDigital", value);
			}
		}
		
		bool show_date = prefs.Get<bool> ("ShowDate", false);
		bool ShowDate {
			get { return show_date; }
			set {
				show_date = value;
				prefs.Set<bool> ("ShowDate", value);
			}
		}
		
		public override string Name {
			get {
				return "Clock";
			}
		}
		
		public override ScalingType ScalingType {
			get {
				return ShowDigital ? ScalingType.Downscaled : ScalingType.HighLow;
			}
		}
		
		string current_theme = prefs.Get<string> ("ClockTheme", "default");
		public string CurrentTheme {
			get { return current_theme; }
			protected set {
				current_theme = value;
				prefs.Set<string> ("ClockTheme", value);
			}
		}
		
		string ThemePath {
			get {
				if (Directory.Exists (System.IO.Path.Combine (Services.Paths.UserDataDirectory, "ClockTheme/" + CurrentTheme)))
					return System.IO.Path.Combine (Services.Paths.UserDataDirectory, "ClockTheme/" + CurrentTheme);
				if (Directory.Exists ("/usr/share/gnome-do/ClockTheme/" + CurrentTheme))
					return "/usr/share/gnome-do/ClockTheme/" + CurrentTheme;
				if (Directory.Exists ("/usr/local/share/gnome-do/ClockTheme/" + CurrentTheme))
					return "/usr/local/share/gnome-do/ClockTheme/" + CurrentTheme;
				return "";
			}
		}
		
		public ClockDockItem()
		{
			cal_painter = new CalendarPainter (this);
			Core.DockServices.PainterService.RegisterPainter (cal_painter);
			GLib.Timeout.Add (1000, ClockUpdateTimer);
		}
		
		bool ClockUpdateTimer ()
		{
			if (minute != DateTime.UtcNow.Minute) {
				RedrawIcon ();
				minute = DateTime.UtcNow.Minute;
			}
			return true;
		}
		
		void RenderFileOntoContext (Context cr, string file, int size)
		{
			if (!File.Exists (file))
				return;
			
			Gdk.Pixbuf pbuf = Rsvg.Tool.PixbufFromFileAtSize (file, size, size);
			CairoHelper.SetSourcePixbuf (cr, pbuf, 0, 0);
			cr.Paint ();
			pbuf.Dispose ();
		}

		protected override Surface MakeIconSurface (Cairo.Surface similar, int size)
		{
			if (ShowMilitary)
				SetText (DateTime.Now.ToString ("ddd, MMM dd HH:mm"));
			else
				SetText (DateTime.Now.ToString ("ddd, MMM dd h:mm tt"));
			
			Surface tmp_surface = similar.CreateSimilar (Cairo.Content.ColorAlpha, size, size);
			
			using (Context cr = new Context (tmp_surface)) {
				if (ShowDigital)
					MakeDigitalIcon (cr, size);
				else
					MakeAnalogIcon (cr, size);
			}
			
			return tmp_surface;
		}
		
		void MakeDigitalIcon (Context cr, int size)
		{
			// shared by all text
			TextRenderContext textContext = new TextRenderContext (cr, string.Empty, size);
			textContext.Alignment = Pango.Alignment.Center;
			textContext.EllipsizeMode = Pango.EllipsizeMode.None;
			
			// draw the time, outlined
			textContext.FontSize = size / 4;
			int yOffset = ShowMilitary ? textContext.FontSize / 2 : 0;
			if (ShowDate)
				textContext.LeftCenteredPoint = new Gdk.Point (0, yOffset + textContext.FontSize);
			else
				textContext.LeftCenteredPoint = new Gdk.Point (0, yOffset + size / 2 - size / 8);
			
			if (ShowMilitary)
				textContext.Text = string.Format ("<b>{0}</b>", DateTime.Now.ToString ("HH:mm"));
			else
				textContext.Text = string.Format ("<b>{0}</b>", DateTime.Now.ToString ("h:mm"));
			
			DockServices.DrawingService.TextPathAtPoint (textContext);
			cr.LineWidth = 3;
			cr.Color = new Cairo.Color (0, 0, 0, 0.5);
			cr.StrokePreserve ();
			cr.Color = new Cairo.Color (1, 1, 1, 0.8);
			cr.Fill ();
			
			// draw the date, outlined
			if (ShowDate) {
				textContext.FontSize = size / 5;
				textContext.LeftCenteredPoint = new Gdk.Point (0, size - textContext.FontSize);
				
				textContext.Text = string.Format ("<b>{0}</b>", DateTime.Now.ToString ("MMM dd"));
				
				DockServices.DrawingService.TextPathAtPoint (textContext);
				cr.LineWidth = 2.5;
				cr.Color = new Cairo.Color (0, 0, 0, 0.5);
				cr.StrokePreserve ();
				cr.Color = new Cairo.Color (1, 1, 1, 0.8);
				cr.Fill ();
			}
			
			if (!ShowMilitary) {
				// shared for AM/PM
				textContext = new TextRenderContext (cr, string.Empty, size / 2);
				textContext.FontSize = size / 5;
				
				// draw AM indicator
				if (DateTime.Now.Hour < 12)
					cr.Color = new Cairo.Color (1, 1, 1, 0.9);
				else
					cr.Color = new Cairo.Color (1, 1, 1, 0.5);
				
				textContext.Text = "<b>am</b>";
				if (ShowDate)
					textContext.LeftCenteredPoint = new Gdk.Point (size / 10, size / 2);
				else
					textContext.LeftCenteredPoint = new Gdk.Point (size / 10, size / 8 + size / 2 + textContext.FontSize);
				DockServices.DrawingService.TextPathAtPoint (textContext);
				cr.Fill ();
				
				// draw PM indicator
				if (DateTime.Now.Hour > 11)
					cr.Color = new Cairo.Color (1, 1, 1, 0.9);
				else
					cr.Color = new Cairo.Color (1, 1, 1, 0.5);
				
				textContext.Text = "<b>pm</b>";
				if (ShowDate)
					textContext.LeftCenteredPoint = new Gdk.Point (size / 10 + size / 2, size / 2);
				else
					textContext.LeftCenteredPoint = new Gdk.Point (size / 10 + size / 2, size / 8 + size / 2 + textContext.FontSize);
				DockServices.DrawingService.TextPathAtPoint (textContext);
				cr.Fill ();
			}
		}
		
		void MakeAnalogIcon (Context cr, int size)
		{
			cr.AlphaFill ();
			
			int center = size / 2;
			int radius = center;
			
			RenderFileOntoContext (cr, System.IO.Path.Combine (ThemePath, "clock-drop-shadow.svg"), radius * 2);
			RenderFileOntoContext (cr, System.IO.Path.Combine (ThemePath, "clock-face-shadow.svg"), radius * 2);
			RenderFileOntoContext (cr, System.IO.Path.Combine (ThemePath, "clock-face.svg"), radius * 2);
			RenderFileOntoContext (cr, System.IO.Path.Combine (ThemePath, "clock-marks.svg"), radius * 2);
			
			cr.Translate (center, center);
			cr.Color = new Cairo.Color (.15, .15, .15);
			
			cr.LineWidth = Math.Max (1, size / 48);
			cr.LineCap = LineCap.Round;
			double minuteRotation = 2 * Math.PI * (DateTime.Now.Minute / 60.0) + Math.PI;
			cr.Rotate (minuteRotation);
			cr.MoveTo (0, radius - radius * .35);
			cr.LineTo (0, 0 - radius * .15);
			cr.Stroke ();
			cr.Rotate (0 - minuteRotation);
			
			cr.Color = new Cairo.Color (0, 0, 0);
			double hourRotation = 2 * Math.PI * (DateTime.Now.Hour / (ShowMilitary ? 24.0 : 12.0)) + 
					Math.PI + (Math.PI / (ShowMilitary ? 12.0 : 6.0)) * DateTime.Now.Minute / 60.0;
			cr.Rotate (hourRotation);
			cr.MoveTo (0, radius - radius * .5);
			cr.LineTo (0, 0 - radius * .15);
			cr.Stroke ();
			cr.Rotate (0 - hourRotation);
			
			cr.Translate (0 - center, 0 - center);
			
			RenderFileOntoContext (cr, System.IO.Path.Combine (ThemePath, "clock-glass.svg"), radius * 2);
			RenderFileOntoContext (cr, System.IO.Path.Combine (ThemePath, "clock-frame.svg"), radius * 2);
		}
		
		public override void Clicked (uint button, Gdk.ModifierType state, PointD position)
		{
			cal_painter.Summon ();
			base.Clicked (button, state, position);
		}
		
		public void SetTheme (string theme)
		{
			if (string.IsNullOrEmpty (theme))
				return;
			
			Services.Application.RunOnMainThread (() => {
				CurrentTheme = theme;
				RedrawIcon ();
			});
		}
		
		#region IRightClickable implementation 
		
		public event EventHandler RemoveClicked;
		
		public IEnumerable<AbstractMenuArgs> GetMenuItems ()
		{
			yield return new SeparatorMenuButtonArgs ();
			
			yield return new SimpleMenuButtonArgs (() => { ShowDigital = !ShowDigital; RedrawIcon (); },
					Catalog.GetString ("Digital Clock"), ShowDigital ? "gtk-apply" : "gtk-remove");
			
			yield return new SimpleMenuButtonArgs (() => { ShowMilitary = !ShowMilitary; RedrawIcon (); },
					Catalog.GetString ("24-Hour Clock"), ShowMilitary ? "gtk-apply" : "gtk-remove");
			
			yield return new SimpleMenuButtonArgs (() => { ShowDate = !ShowDate; RedrawIcon (); },
					Catalog.GetString ("Show Date"), ShowDate ? "gtk-apply" : "gtk-remove", !ShowDigital);
			
			yield return new SimpleMenuButtonArgs (() => { new ClockThemeSelector (this).Show (); },
					Catalog.GetString ("Select Theme"), "preferences-desktop-theme", ShowDigital);
		}
		
		#endregion 
	}
	
	public class ClockThemeSelector : Gtk.Dialog
	{
		TreeStore labelTreeStore = new TreeStore (typeof (string));
		TreeView labelTreeView = new TreeView ();
		
		ClockDockItem DockItem { get; set; }
		
		public ClockThemeSelector (ClockDockItem dockItem)
		{
			DockItem = dockItem;
			Title = Catalog.GetString ("Themes");
			
			labelTreeView.Model = labelTreeStore;
			labelTreeView.HeadersVisible = false;
			labelTreeView.Selection.Changed += OnLabelSelectionChanged;
			labelTreeView.AppendColumn (Catalog.GetString ("Theme"), new CellRendererText (), "text", 0);
			
			ScrolledWindow win = new ScrolledWindow ();
			win.Add (labelTreeView);
			win.SetSizeRequest (200, 300);
			win.Show ();
			VBox.PackEnd (win);
			VBox.ShowAll ();
			AddButton ("Close", ResponseType.Close);

			UpdateThemeList ();
		}
		
		public void UpdateThemeList ()
		{
			List<string> themes = new List<string> ();
			
			if (Directory.Exists (System.IO.Path.Combine (Services.Paths.UserDataDirectory, "ClockTheme"))) {
				DirectoryInfo root = new DirectoryInfo (System.IO.Path.Combine (Services.Paths.UserDataDirectory, "ClockTheme"));
				root.GetDirectories ().ForEach (p => themes.Add (p.Name));
			}
			if (Directory.Exists ("/usr/share/gnome-do/ClockTheme")) {
				DirectoryInfo root = new DirectoryInfo ("/usr/share/gnome-do/ClockTheme");
				root.GetDirectories ().ForEach (p => themes.Add (p.Name));
			}
			if (Directory.Exists ("/usr/local/share/gnome-do/ClockTheme")) {
				DirectoryInfo root = new DirectoryInfo ("/usr/local/share/gnome-do/ClockTheme");
				root.GetDirectories ().ForEach (p => themes.Add (p.Name));
			}
			
			labelTreeStore.Clear ();
			
			themes.Sort ();
			
			int i = 0, selected = -1;
			themes.Distinct ().ForEach (p => {
				if (p == DockItem.CurrentTheme)
					selected = i;
				labelTreeStore.AppendValues (p);
				i++;
			});
			
			labelTreeView.Selection.SelectPath(new TreePath("" + selected));
		}
		
		protected virtual void OnLabelSelectionChanged (object o, System.EventArgs args)
		{
			TreeIter iter;
			TreeModel model;
			
			if (((TreeSelection)o).GetSelected (out model, out iter))
				DockItem.SetTheme ((string) model.GetValue (iter, 0));
		}
		
		protected override void OnResponse (ResponseType response_id)
		{
			Destroy ();
		}
	}
}
