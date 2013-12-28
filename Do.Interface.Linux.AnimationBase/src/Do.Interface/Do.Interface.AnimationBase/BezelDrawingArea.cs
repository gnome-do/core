// BeezelDrawingArea.cs
// 
// Copyright (C) 2008 GNOME-Do
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using System;
using System.Collections.Generic;

using Cairo;
using Gdk;
using Gtk;

using Mono.Unix;

using Do.Interface;
using Do.Interface.CairoUtils;
using Do.Universe;
using Do.Platform;

namespace Do.Interface.AnimationBase
{
	public enum PointLocation {
		Window,
		Close,
		Preferences,
		Outside,
	}
	
	public enum HUDStyle {
		HUD,
		Classic,
	}
	
	public class BezelDrawingArea : Gtk.DrawingArea
	{
		enum DrawState {
			Normal,
			Text,
			NoResult,
			None,
		}
		
#region Static Area
		static IPreferences prefs = Services.Preferences.Get<BezelDrawingArea> ();
		public static event EventHandler ThemeChanged;

		static BezelDrawingArea()
		{
			prefs.PreferencesChanged += HandlePreferencesChanged;
		}
		
		static void HandlePreferencesChanged (object o, PreferencesChangedEventArgs e)
		{
			if (e.Key == "Animated" || e.Key == "WindowRadius")
				return;
			OnThemeChanged ();
		}
		
		public static bool Animated {
			get {
				return prefs.Get<bool> ("Animated", true);
			}
			set {
				prefs.Set<bool> ("Animated", value);
			}
		}
		
		public static string TitleRenderer {
			get {
				return prefs.Get<string> ("TitleRenderer", "default");
			}
			set {
				prefs.Set<string> ("TitleRenderer", value);
			}
		}
		
		public static string PaneRenderer {
			get {
				return prefs.Get<string> ("PaneRenderer", "default");
			}
			set {
				prefs.Set<string> ("PaneRenderer", value);
			}
		}
		
		public static string WindowRenderer {
			get {
				return prefs.Get<string> ("WindowRenderer", "default");
			}
			set {
				prefs.Set<string> ("WindowRenderer", value);
			}
		}
		
		public static string BgColor {
			get {
				return prefs.Get<string> ("BackgroundColor", "default");
			}
			set {
				prefs.Set<string> ("BackgroundColor", value);
			}
		}
		
		public static int RoundingRadius { 
			get { 
				return prefs.Get<int> ("WindowRadius", -1);
			} 
			set {
				prefs.Set<int> ("WindowRadius", Math.Max (-1, value));
			}
		}
		
		public static bool DrawShadow {
			get {
				return prefs.Get<bool> ("Shadow", true);
			}
			set {
				prefs.Set<bool> ("Shadow", value);
			}
		}
		
		public static void SetDefaultStyle ()
		{
			DrawShadow = true;
			prefs.Set<int> ("WindowRadius", -1);
		}
		
		public static void ResetBackgroundStyle ()
		{
			prefs.Set<string> ("BackgroundColor", "default");
		}
		
		private static void OnThemeChanged ()
		{
			if (ThemeChanged != null)
				ThemeChanged (new System.Object (), new EventArgs ());
		}
#endregion
		
		public const int TextHeight = 11;
		
		HUDStyle style;
		
		const int BoxLineWidth    = 1;
		const int BorderWidth     = 15;
		const int fade_ms         = 150;
		const int ShadowRadius    = 10;
		
		IBezelTitleBarRenderElement  titleBarRenderer;
		IBezelWindowRenderElement    backgroundRenderer;
		IBezelPaneRenderElement      paneOutlineRenderer;
		IBezelOverlayRenderElement   textModeOverlayRenderer;
		IBezelDefaults               bezelDefaults;
		
		IRenderTheme theme;
		
		IDoController controller;
		
		BezelColors colors;
		BezelGlassResults bezel_results;
		
		bool third_pane_visible, preview;
		BezelDrawingContext context, old_context;
		PixbufSurfaceCache surface_cache;
		Pane focus;
		DateTime delta_time;
		uint timer;
		
		Gdk.Rectangle drawing_area;
		Surface surface;
		
		double text_box_scale, window_fade = 1, window_scale=1;
		
		double[] icon_fade = new double [] {1, 1, 1};
		bool[] entry_mode = new bool[3];
		
		public Cairo.Color BackgroundColor {
			get {
				Gdk.Color color = new Gdk.Color ();
				if (Gdk.Color.Parse ("#" + BgColor.Substring (0, 6), ref color)) {
					if (BgColor.Length == 8) {
						double alpha = Convert.ToInt32 (BgColor.Substring (6, 2), 16);
						alpha = alpha / 255;
						return color.ConvertToCairo (alpha);
					}
					return color.ConvertToCairo (backgroundRenderer.BackgroundColor.A);
				}
				return backgroundRenderer.BackgroundColor;
			}
		}
		
		public int BoxWidth { 
			get { return PaneOutlineRenderer.Width; } 
		}
		
		public int BoxHeight { 
			get { return PaneOutlineRenderer.Height; } 
		}
		
		public Pane Focus {
			get { return focus; }
			set { 
				if (focus == value)
					return;
				focus = value;
				AnimatedDraw ();
			}
		}
		
		public BezelColors Colors {
			get {
				return colors;
			}
		}
		
		public TextRenderer TextUtility { get; set; }
		
		public int IconSize {
			get {
				return paneOutlineRenderer.IconSize;
			}
		}
		
		public int InternalHeight {
			get {
				if (BezelDefaults.RenderDescriptionText)
					return BoxHeight + 2 * WindowBorder + TextHeight + TitleBarHeight;
				return BoxHeight + 2 * WindowBorder + TitleBarHeight;
			}
		}

		public BezelGlassResults Results {
			get {
				if (bezel_results == null) 
					bezel_results = new BezelGlassResults (controller, Math.Min (TwoPaneWidth - 2 * WindowRadius, 360), style, colors);
				return bezel_results;
			}
		}
		
		public int TextModeOffset {
			get { 
				return Math.Max (TitleBarHeight, WindowRadius); 
			}
		}
		
		public bool ThirdPaneVisible {
			get { return third_pane_visible; }
			set { 
				third_pane_visible = value; 
				if (third_pane_visible)
				AnimatedDraw ();
			}
		}
		
		public int ThreePaneWidth { 
			get { 
				return (2 * WindowBorder - BorderWidth) + (BoxWidth + BorderWidth) * 3; 
			} 
		}
		
		public int TitleBarHeight { 
			get { 
				return TitleBarRenderer.Height; 
			} 
		}
		
		public int TwoPaneWidth { 
			get { 
				return (2 * WindowBorder) - BorderWidth + ((BoxWidth + (BorderWidth)) * 2); 
			} 
		}
		
		public string HighlightFormat { 
			get { 
				if (colors.Background.B == colors.Background.G && colors.Background.B == colors.Background.R)
					return BezelDefaults.HighlightFormat; 
				else
					return "<span underline=\"single\"><b>{0}</b></span>";
			} 
		}
		
		public int WindowBorder { 
			get { 
				return BezelDefaults.WindowBorder; 
			} 
		}
		
		public int WindowHeight {
			get {
				return InternalHeight + 2 * ShadowRadius;
			}
		}
		
		public int WindowRadius { 
			get { 
				if (RoundingRadius <= -1)
					return BezelDefaults.WindowRadius; 
				return Math.Max (1, prefs.Get<int> ("WindowRadius", -1));
			} 
		}
		
		public int WindowWidth { 
			get { 
				return (2 * WindowBorder - BorderWidth) + ((BoxWidth + BorderWidth) * 3) + 2 * ShadowRadius; 
			} 
		}
		
		private bool AnimationNeeded {
			get {
				return  ExpandNeeded || 
					    ShrinkNeeded || 
						TextScaleNeeded || 
						FadeNeeded || 
						WindowFadeNeeded;
			}
		}
		
		private bool ExpandNeeded {
			get {
				bool shouldBeExpanded = ThirdPaneVisible || entry_mode [(int) Focus];
				bool isNotExpanded = drawing_area.Width != ThreePaneWidth;
				return shouldBeExpanded && isNotExpanded;
			}
		}
		
		private bool ShrinkNeeded {
			get {
				bool shouldNotBeExpanded = !ThirdPaneVisible && !entry_mode [(int) focus];
				return shouldNotBeExpanded  && (drawing_area.Width != TwoPaneWidth) && (Focus != Pane.Third);
			}
		}
		
		private bool FadeNeeded {
			get {
				return icon_fade [0] != 1 || 
					   icon_fade [1] != 1 || 
					   icon_fade [2] != 1;
			}
		}
		
		private bool TextScaleNeeded {
			get {
				bool textBoxNeedsExpand = entry_mode [(int) Focus] && text_box_scale != 1;
				bool textBoxNeedsShrink = !entry_mode [(int) Focus] && text_box_scale != 0;
				return textBoxNeedsExpand || textBoxNeedsShrink;
			}
		}
		
		private bool WindowFadeNeeded {
			get {
				return window_fade != 1;
			}
		}
		
		private BezelDrawingContext Context {
			get {
				if (context == null)
					context = new BezelDrawingContext ();
				return context;
			}
		}
		
		private BezelDrawingContext OldContext {
			get {
				if (old_context == null)
					old_context = new BezelDrawingContext ();
				return old_context;
			}
		}
		
		public IBezelTitleBarRenderElement TitleBarRenderer { 
			get { return titleBarRenderer; } 
		}

		public IBezelWindowRenderElement BackgroundRenderer { 
			get { return backgroundRenderer; } 
		}

		public IBezelPaneRenderElement PaneOutlineRenderer { 
			get { return paneOutlineRenderer; } 
		}

		public IBezelOverlayRenderElement TextModeOverlayRenderer {	
			get { return textModeOverlayRenderer; }	
		}

		public IBezelDefaults BezelDefaults { 
			get { return bezelDefaults; }	
		}
		
		public event EventHandler GtkThemeChanged;
		
		public BezelDrawingArea (IDoController controller, IRenderTheme theme, bool preview) : base ()
		{
			this.controller = controller;
			TextUtility = new TextRenderer (this);
			
			DoubleBuffered = false;
			this.preview = preview;
			this.theme = theme;
			
			if (theme.Name == "Nouveau") //this is a temporary hack
				style = HUDStyle.HUD;
			else
				style = HUDStyle.Classic; //gets us the classic results list
			
			ResetRenderStyle ();
			SetDrawingArea ();
			
			BezelDrawingArea.ThemeChanged += OnThemeChanged;
			Realized += delegate {
				GdkWindow.SetBackPixmap (null, false);
			};
			
			StyleSet += delegate {
				if (IsRealized)
					GdkWindow.SetBackPixmap (null, false);
			};
		}
		
		private void SetDrawingArea ()
		{
			if (preview && TwoPaneWidth > 400) {
				window_scale = 400.0 / TwoPaneWidth;
			}
			SetSizeRequest ((int) Math.Floor (WindowWidth * window_scale), (int) Math.Floor (WindowHeight * window_scale));
			drawing_area  = new Gdk.Rectangle ((WindowWidth - TwoPaneWidth) / 2, ShadowRadius, TwoPaneWidth, InternalHeight);
			if (preview)
				drawing_area.X = ShadowRadius;
		}
		
		private void ResetRenderStyle ()
		{
			BuildRenderers ();
			
			if (colors == null)
				colors = new BezelColors (BackgroundColor);
			else
				colors.RebuildColors (BackgroundColor);
			
			SetDrawingArea ();
		}
		
		private void BuildRenderers ()
		{
			bezelDefaults           = theme.GetDefaults (this);
			titleBarRenderer        = theme.GetTitleBar (this);
			textModeOverlayRenderer = theme.GetOverlay (this);
			backgroundRenderer      = theme.GetWindow (this);
			paneOutlineRenderer     = theme.GetPane (this);
		}
		
		public PixbufSurfaceCache SurfaceCache {
			get {
				if (surface_cache == null) {
					using (Context cr = CairoHelper.Create (GdkWindow)) {
						using (var target = cr.GetTarget ()) {
							surface_cache = new PixbufSurfaceCache (50, IconSize, IconSize, target);
						}
					}
				}
				return surface_cache;
			}
		}

		private void AnimatedDraw ()
		{
			if (!IsDrawable || timer > 0)
				return;

			if (preview) {
				Paint ();
				return;
			}
			
			delta_time = DateTime.Now;
			timer = GLib.Timeout.Add (1000 / 65, OnDrawTimeoutElapsed);
		}
		
		private DrawState PaneDrawState (Pane pane)
		{
			if (pane == Pane.Third && !ThirdPaneVisible)
				return DrawState.None;
			
			if (Context.GetPaneTextMode (pane))
				return DrawState.Text;
			
			if (Context.GetPaneObject (pane) != null)
				return DrawState.Normal;
			
			if (!string.IsNullOrEmpty (Context.GetPaneQuery (pane))) {
				return DrawState.NoResult;
			}
			
			return DrawState.None;
		}
		
		public void BezelSetPaneObject (Pane pane, Do.Universe.Item obj)
		{
			if (Context.GetPaneObject (pane) == obj && obj != null)
				return;
			
			OldContext.SetPaneObject (pane, Context.GetPaneObject (pane));
			Context.SetPaneObject (pane, obj);
			
			icon_fade [(int) pane] = 0;
			Draw ();
		}
		
		public void BezelSetTextMode (Pane pane, bool textMode)
		{
			if (Context.GetPaneTextMode (pane) == textMode)
				return;
			
			OldContext.SetPaneTextMode (pane, Context.GetPaneTextMode (pane));
			Context.SetPaneTextMode (pane, textMode);
			
			Draw ();
		}
		
		public void BezelSetEntryMode (Pane pane, bool entryMode)
		{
			entry_mode [(int) pane] = entryMode;
			Draw ();
		}
		
		public void BezelSetQuery (Pane pane, string query)
		{
			if (Context.GetPaneQuery (pane) == query)
				return;
			
			OldContext.SetPaneQuery (pane, Context.GetPaneQuery (pane));
			Context.SetPaneQuery (pane, query);
			Draw ();
		}
		
		public void Clear ()
		{
			context = new BezelDrawingContext ();
			old_context = new BezelDrawingContext ();
			entry_mode = new bool [3];
			Draw ();
		}
		
		void Draw ()
		{
			AnimatedDraw ();
		}
		
		void Paint () 
		{
			if (!IsDrawable)
				return;
			using (var cr2 = Gdk.CairoHelper.Create (GdkWindow)) {
				//Much kudos to Ian McIntosh
				if (surface == null)
					surface = cr2.CreateSimilarToTarget (WindowWidth, WindowHeight);
			
				using (var cr = new Context (surface)) {
			
					if (preview) {
						Gdk.Color bgColor;
						using (Gtk.Style rcstyle = Gtk.Rc.GetStyle (this)) {
							bgColor = rcstyle.Backgrounds [(int)StateType.Normal];
						}
						cr.SetSourceRGBA (bgColor.ConvertToCairo (1));
					} else {
						cr.SetSourceRGBA (0, 0, 0, 0);
					}			
					cr.Operator = Cairo.Operator.Source;
					cr.Paint ();
					cr.Operator = Cairo.Operator.Over;
			
					BackgroundRenderer.RenderItem (cr, drawing_area);
			
					RenderTitleBar (cr);
					do {
						if (text_box_scale > 0) {
					
							RenderTextModeOverlay (cr);
							if (text_box_scale == 1) {
								RenderTextModeText (cr);
								continue;
							}
						}
				
						if (BezelDefaults.RenderDescriptionText)
							RenderDescriptionText (cr);
						//--------------First Pane---------------
						RenderPane (Pane.First, cr);
						//------------Second Pane----------------
						RenderPane (Pane.Second, cr);
						//------------Third Pane-----------------
						if (ThirdPaneVisible) {
							RenderPane (Pane.Third, cr);
						}

						if (text_box_scale > 0) {
							RenderTextModeOverlay (cr);
						}
				
					} while (false);
			
					if (DrawShadow)
						Util.Appearance.DrawShadow (cr, drawing_area.X, drawing_area.Y, drawing_area.Width, 
							drawing_area.Height, WindowRadius, new Util.ShadowParameters (.5, ShadowRadius));

					if (window_scale != 1) {//we are likely in preview mode, though this can be set on the fly
						cr2.Scale (window_scale, window_scale);
					}
					cr2.SetSourceSurface (surface, 0, 0);
					cr2.Operator = Operator.Source;
					cr2.Paint ();
				}
			}
		}
		
		protected override void OnDestroyed ()
		{
			base.OnDestroyed ();
			BezelDrawingArea.ThemeChanged -= OnThemeChanged;
			if (surface != null)
				surface.Dispose ();
			context = old_context = null;
		}
		
		protected bool OnDrawTimeoutElapsed ()
		{
			double change = (Animated) ? DateTime.Now.Subtract (delta_time).TotalMilliseconds / fade_ms : 10;
			
			delta_time = DateTime.Now;
			if (ExpandNeeded) {
				drawing_area.Width += (int) ((ThreePaneWidth - TwoPaneWidth) * change);
				drawing_area.Width = (drawing_area.Width > ThreePaneWidth) ? ThreePaneWidth : drawing_area.Width;
				drawing_area.X = (WindowWidth - drawing_area.Width) / 2;
			} else if (ShrinkNeeded) {
				drawing_area.Width -= (int) ((ThreePaneWidth - TwoPaneWidth) * change);
				drawing_area.Width = (drawing_area.Width < TwoPaneWidth) ? TwoPaneWidth : drawing_area.Width;
				drawing_area.X = (WindowWidth - drawing_area.Width) / 2;
			}
			
			if (TextScaleNeeded) {
				if (entry_mode [(int) Focus]) {
					text_box_scale += change;
					text_box_scale = (text_box_scale > 1) ? 1 : text_box_scale;
				} else {
					text_box_scale -= change;
					text_box_scale = (text_box_scale < 0) ? 0 : text_box_scale;
				}
			}
			
			if (FadeNeeded) {
				if (text_box_scale == 1) {
					icon_fade [0] = icon_fade [1] = icon_fade [2] = 1;
				}
				icon_fade [0] += change;
				icon_fade [1] += change;
				icon_fade [2] += change;
				
				icon_fade [0] = (icon_fade [0] > 1) ? 1 : icon_fade [0];
				icon_fade [1] = (icon_fade [1] > 1) ? 1 : icon_fade [1];
				icon_fade [2] = (icon_fade [2] > 1) ? 1 : icon_fade [2];
			}
			
			if (WindowFadeNeeded) {
				window_fade += (change * .1) + (window_fade / 2);
				window_fade = Math.Min (window_fade, 1);
			}
			
			QueueDraw ();
			
			if (AnimationNeeded) {
				return true;
			} else {
				timer = 0;
				return false;
			}
		}
		
		protected override bool OnExposeEvent (EventExpose evnt)
		{
			bool ret = base.OnExposeEvent (evnt);
			Paint ();
			return ret;
		}
		
		protected override void OnStyleSet (Gtk.Style previous_style)
		{
			if (GtkThemeChanged != null)
				GtkThemeChanged (this, System.EventArgs.Empty);
			Colors.RebuildColors (BackgroundColor);
			base.OnStyleSet (previous_style);
		}

		private void OnThemeChanged (object o, System.EventArgs args)
		{
			ResetRenderStyle ();
			QueueDraw ();
		}
		
		public int PaneOffset (Pane pane) 
		{
			return WindowBorder + ((int) pane * (BoxWidth + BorderWidth));
		}
		
		private void RenderPane (Pane pane, Context cr)
		{
			if (pane == Pane.Third) {
				cr.Rectangle (drawing_area.X, drawing_area.Y, drawing_area.Width, drawing_area.Height);
				cr.Clip ();
			}
			RenderPaneOutline (pane, cr);
			
			switch (PaneDrawState (pane)) {
			case DrawState.Normal:
				RenderPixbuf (pane, cr);
				RenderPaneText (pane, cr);
				break;
			case DrawState.NoResult:
				RenderPixbuf (pane, cr, "gtk-question-dialog", 1);
				RenderPaneText (pane, cr, string.Format (Catalog.GetString ("No results for: {0}"), Context.GetPaneQuery (pane)));
				break;
			case DrawState.Text:
				if (text_box_scale < 1) {
					RenderPixbuf (pane, cr, "gnome-mime-text", .1);
					RenderPaneText (pane, cr);
				}
				break;
			case DrawState.None:
				if (pane == Pane.First) {
					RenderPixbuf (pane, cr, "search", 1);
					RenderPaneText (pane, cr, Catalog.GetString ("Type To Search"));
				}
				break;
			}
			if (pane == Pane.Third)
				cr.ResetClip ();
		}
		
		private void RenderPaneOutline (Pane pane, Context cr)
		{
			Gdk.Rectangle render_region = new Gdk.Rectangle () {
				Width = BoxWidth,
				Height = BoxHeight,
				X = drawing_area.X + PaneOffset (pane),
				Y = drawing_area.Y + WindowBorder + TitleBarHeight,
			};
			PaneOutlineRenderer.RenderItem (cr, render_region, (Focus == pane));
		}
		
		private void RenderPixbuf (Pane pane, Context cr)
		{
			Do.Universe.Item obj = Context.GetPaneObject (pane);
			RenderPixbuf (pane, cr, obj.Icon, 1);
		}
		
		private void RenderPixbuf (Pane pane, Context cr, string icon, double alpha)
		{
			int offset = PaneOffset (pane);
			if (!SurfaceCache.ContainsKey (icon)) {
				CreateCachedSurfaceForIcon (icon);
			}
			
			string sec_icon = "";
			if (OldContext.GetPaneObject (pane) != null)
				sec_icon = OldContext.GetPaneObject (pane).Icon;
			
			double calc_alpha = (sec_icon != icon) ? icon_fade[(int) pane] : 1;
			int x;
			int y;
			if (PaneOutlineRenderer.StackIconText) {
				x = drawing_area.X + offset + (BoxWidth / 2 - IconSize / 2);
				y = drawing_area.Y + WindowBorder + TitleBarHeight + 3;
			} else {
				x = drawing_area.X + offset + 5;
				y = drawing_area.Y + WindowBorder + TitleBarHeight + ((BoxHeight - IconSize) / 2);
			}
			cr.SetSource (SurfaceCache.GetSurface (icon), x, y);
			cr.PaintWithAlpha (calc_alpha * alpha);
			if (string.IsNullOrEmpty (sec_icon) || calc_alpha < 1) 
				return;
			
			if (!SurfaceCache.ContainsKey (OldContext.GetPaneObject (pane).Icon)) {
				CreateCachedSurfaceForIcon (OldContext.GetPaneObject (pane).Icon);
			}
			cr.SetSource (SurfaceCache.GetSurface (OldContext.GetPaneObject (pane).Icon), x, y);
			cr.PaintWithAlpha (alpha * (1 - calc_alpha));
		}
		
		private void CreateCachedSurfaceForIcon (string icon)
		{
			SurfaceCache.AddPixbufSurface (icon, icon);
		}
		
		void RenderDescriptionText (Context cr)
		{
			if (Context.GetPaneObject (Focus) == null)
				return;
			string text = GLib.Markup.EscapeText (Context.GetPaneObject (Focus).Description);
			int x = drawing_area.X + 10;
			int y = drawing_area.Y + InternalHeight - WindowBorder - 4;
			TextUtility.RenderLayoutText (cr, text, x, y, drawing_area.Width - 20, TextHeight);
		}
		
		void RenderPaneText (Pane pane, Context cr)
		{
			if (Context.GetPaneObject (pane) != null)
				RenderPaneText (pane, cr, GLib.Markup.EscapeText (Context.GetPaneObject (pane).Name));
		}
		
		void RenderPaneText (Pane pane, Context cr, string text)
		{
			if (text.Length == 0) return;
			
			if (Context.GetPaneTextMode (pane)) {
				Pango.Color color = new Pango.Color ();
				color.Blue = color.Green = color.Red = ushort.MaxValue;
				int y = drawing_area.Y + WindowBorder + TitleBarHeight + 6;
				int x = drawing_area.X + PaneOffset (pane) + 5;
				
				TextUtility.RenderLayoutText (cr, text, x, y, BoxWidth - 10, TextHeight, color, 
				                              Pango.Alignment.Left, Pango.EllipsizeMode.None);
			} else {
				string query = Context.GetPaneQuery (pane); 
				if (!string.IsNullOrEmpty (query)) {
					text = Util.FormatCommonSubstrings (text, query, HighlightFormat);
				}
				
				if (PaneOutlineRenderer.StackIconText) {
					int y = drawing_area.Y + WindowBorder + TitleBarHeight + BoxHeight - TextHeight - 9;
					int x = drawing_area.X + PaneOffset (pane) + 5;
					TextUtility.RenderLayoutText (cr, text, x, y, BoxWidth - 10, TextHeight);
				} else {
					int y = drawing_area.Y + WindowBorder + TitleBarHeight + (int)(BoxHeight/2);
					int x = drawing_area.X + PaneOffset (pane) + IconSize + 10;
					TextUtility.RenderLayoutText (cr, text, x, y, BoxWidth - IconSize - 20, TextHeight);
				}
			}
		}
		
		void RenderTextModeText (Context cr)
		{
			Pango.Color color = new Pango.Color ();
			color.Blue = color.Red = color.Green = (ushort) (ushort.MaxValue * text_box_scale);
			Gdk.Rectangle cursor = TextUtility.RenderLayoutText (cr, GLib.Markup.EscapeText (Context.GetPaneQuery (Focus)), 
			                                                        drawing_area.X + 10, drawing_area.Y + TextModeOffset + 5, 
			                                                        drawing_area.Width - 20, 18, color, 
			                                                        Pango.Alignment.Left, Pango.EllipsizeMode.None);
			if (cursor.X == cursor.Y && cursor.X == 0) return;
			
			cr.Rectangle (cursor.X, cursor.Y, 2, cursor.Height);
			cr.SetSourceRGBA (.4, .5, 1, .85);
			cr.Fill ();
		}
		
		void RenderTextModeOverlay (Context cr) 
		{
			TextModeOverlayRenderer.RenderItem (cr, drawing_area, text_box_scale);
		}
		
		void RenderTitleBar (Context cr)
		{
			TitleBarRenderer.RenderItem (cr, drawing_area);
		}
		
		public PointLocation GetPointLocation (Gdk.Point point)
		{
			if (BackgroundRenderer.GetPointLocation (drawing_area, point) == PointLocation.Outside)
				return PointLocation.Outside;
			return TitleBarRenderer.GetPointLocation (drawing_area, point);
		}
	}
}
