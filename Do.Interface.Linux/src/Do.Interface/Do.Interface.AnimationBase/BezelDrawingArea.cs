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
				OnThemeChanged ();
			}
		}
		
		public static string PaneRenderer {
			get {
				return prefs.Get<string> ("PaneRenderer", "default");
			}
			set {
				prefs.Set<string> ("PaneRenderer", value);
				OnThemeChanged ();
			}
		}
		
		public static string WindowRenderer {
			get {
				return prefs.Get<string> ("WindowRenderer", "default");
			}
			set {
				prefs.Set<string> ("WindowRenderer", value);
				OnThemeChanged ();
			}
		}
		
		public static string BgColor {
			get {
				return prefs.Get<string> ("BackgroundColor", "default");
			}
			set {
				prefs.Set<string> ("BackgroundColor", value);
				OnThemeChanged ();
			}
		}
		
		public static int RoundingRadius { 
			get { 
				return prefs.Get<int> ("WindowRadius", -1);
			} 
			set {
				prefs.Set<int> ("WindowRadius", Math.Max (-1, value));
				OnThemeChanged ();
			}
		}
		
		public static bool DrawShadow {
			get {
				return prefs.Get<bool> ("Shadow", true);
			}
			set {
				prefs.Set<bool> ("Shadow", value);
				OnThemeChanged ();
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
			OnThemeChanged ();
		}
		
		private static void OnThemeChanged ()
		{
			if (ThemeChanged != null)
				ThemeChanged (new System.Object (), new EventArgs ());
		}
#endregion
		
#region LocalVariables
		HUDStyle style;
		
		const int BoxLineWidth    = 1;
		const int TextHeight      = 11;
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
		
		GConf.Client gconfClient;
#endregion
		
#region Properties
#region Box Format
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
		
		public int BoxWidth { get { return PaneOutlineRenderer.Width; } }
		
		public int BoxHeight { get { return PaneOutlineRenderer.Height; } }
		
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
				return ((2 * WindowBorder) - BorderWidth) + ((BoxWidth + (BorderWidth)) * 3); 
			} 
		}
		
		public int TitleBarHeight { get { return TitleBarRenderer.Height; } }
		
		public int TwoPaneWidth { get { return (2 * WindowBorder) - BorderWidth + ((BoxWidth + (BorderWidth)) * 2); } }
		
		public string HighlightFormat { 
			get { 
				if (colors.Background.B == colors.Background.G && 
				    colors.Background.B == colors.Background.R)
					return BezelDefaults.HighlightFormat; 
				else
					return "<span underline=\"single\">{0}</span>";
			} 
		}
		
		public int WindowBorder { get { return BezelDefaults.WindowBorder; } }
		
		public int WindowHeight {
			get {
				return InternalHeight + 2*ShadowRadius;
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
				return ((2 * WindowBorder) - BorderWidth) + ((BoxWidth + (BorderWidth)) * 3) + (2*ShadowRadius); 
			} 
		}
#endregion
#region Animation Properties
		private bool AnimationNeeded {
			get {
				return ExpandNeeded || ShrinkNeeded || TextScaleNeeded || FadeNeeded || WindowFadeNeeded;
			}
		}
		
		private bool ExpandNeeded {
			get {
				return (ThirdPaneVisible || entry_mode[(int) Focus]) && 
					drawing_area.Width != ThreePaneWidth; 
			}
		}
		
		private bool ShrinkNeeded {
			get {
				return (!ThirdPaneVisible && !entry_mode[(int) focus])  && 
					drawing_area.Width != TwoPaneWidth && Focus != Pane.Third;
			}
		}
		
		private bool FadeNeeded {
			get {
				return (icon_fade[0] != 1 || icon_fade[1] != 1 || icon_fade[2] != 1);
			}
		}
		
		private bool TextScaleNeeded {
			get {
				return (entry_mode[(int) Focus] && text_box_scale != 1) ||
					(!entry_mode[(int) Focus] && text_box_scale != 0);
			}
		}
		
		private bool WindowFadeNeeded {
			get {
				return window_fade != 1;
			}
		}
#endregion
#region Contexts
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
#endregion
#region Renderers
		public IBezelTitleBarRenderElement TitleBarRenderer { get { return titleBarRenderer; } }

		public IBezelWindowRenderElement BackgroundRenderer { get { return backgroundRenderer; } }

		public IBezelPaneRenderElement PaneOutlineRenderer { get { return paneOutlineRenderer; } }

		public IBezelOverlayRenderElement TextModeOverlayRenderer {	get { return textModeOverlayRenderer; }	}

		public IBezelDefaults BezelDefaults { get { return bezelDefaults; }	}
#endregion
#endregion
		
		public event EventHandler GtkThemeChanged;
		
		public BezelDrawingArea (IDoController controller, IRenderTheme theme, bool preview) : base ()
		{
			this.controller = controller;
			
			DoubleBuffered = false;
			prefs = Services.Preferences.Get<BezelDrawingArea> ();
			this.preview = preview;
			this.theme = theme;
			
			if (theme.Name == "Nouveau") //this is a temporary hack
				style = HUDStyle.HUD;
			else
				style = HUDStyle.Classic; //gets us the classic results list
			
			ResetRenderStyle ();
			SetDrawingArea ();
			
			gconfClient = new GConf.Client ();
			gconfClient.AddNotify ("/desktop/gnome/interface", OnGtkThemeChanged);
			
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
				window_scale = 400.0/TwoPaneWidth;
			}
			SetSizeRequest ((int)Math.Floor (WindowWidth*window_scale), (int)Math.Floor (WindowHeight*window_scale));
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
			this.bezelDefaults           = theme.GetDefaults (this);
			this.titleBarRenderer        = theme.GetTitleBar (this);
			this.textModeOverlayRenderer = theme.GetOverlay (this);
			this.backgroundRenderer      = theme.GetWindow (this);
			this.paneOutlineRenderer     = theme.GetPane (this);
		}
		
		public PixbufSurfaceCache SurfaceCache {
			get {
				if (surface_cache == null) {
					using (Context cr = CairoHelper.Create (GdkWindow))
						surface_cache = new PixbufSurfaceCache (50, IconSize, IconSize, cr.Target);
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
			timer = GLib.Timeout.Add (1000/65, OnDrawTimeoutElapsed);
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
		
		public void BezelSetPaneObject (Pane pane, Element obj)
		{
			if (Context.GetPaneObject (pane) == obj && obj != null)
				return;
			
			OldContext.SetPaneObject (pane, Context.GetPaneObject (pane));
			Context.SetPaneObject (pane, obj);
			
			icon_fade[(int) pane] = 0;
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
			entry_mode[(int) pane] = entryMode;
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
			Cairo.Context cr2 = Gdk.CairoHelper.Create (GdkWindow);
			
			//Much kudos to Ian McIntosh
			if (surface == null)
				surface = cr2.Target.CreateSimilar (cr2.Target.Content, WindowWidth, WindowHeight);
			
			Context cr = new Context (surface);
			
			if (preview) {
				Gdk.Color bgColor;
				using (Gtk.Style rcstyle = Gtk.Rc.GetStyle (this)) {
					bgColor = rcstyle.Backgrounds[(int) StateType.Normal];
				}
				cr.Color = bgColor.ConvertToCairo (1);
			} else {
				cr.Color = new Cairo.Color (0, 0, 0, 0);
			}			
			cr.Operator = Cairo.Operator.Source;
			cr.Paint ();
			cr.Operator = Cairo.Operator.Over;
			
			BackgroundRenderer.RenderElement (cr, drawing_area);
			
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
				if (ThirdPaneVisible /*&& drawing_area.Width == ThreePaneWidth*/) {
					RenderPane (Pane.Third, cr);
				}

				if (text_box_scale > 0) {
					RenderTextModeOverlay (cr);
				}
				
			} while (false);
			
			if (DrawShadow)
				Util.Appearance.DrawShadow (cr, drawing_area.X, drawing_area.Y, drawing_area.Width, 
				                            drawing_area.Height, WindowRadius, new Util.ShadowParameters (.5, ShadowRadius));

			if (window_scale != 1) //we are likely in preview mode, though this can be set on the fly
				cr2.Scale (window_scale, window_scale);
			cr2.SetSourceSurface (surface, 0, 0);
			cr2.Operator = Operator.Source;
//			cr2.PaintWithAlpha (window_fade);
			cr2.Paint ();
			
			(cr2 as IDisposable).Dispose ();
			(cr as IDisposable).Dispose ();
		}
		
		protected override void OnDestroyed ()
		{
			base.OnDestroyed ();
			BezelDrawingArea.ThemeChanged -= OnThemeChanged;
			if (surface != null)
				surface.Destroy ();
			context = old_context = null;
		}
		
		protected bool OnDrawTimeoutElapsed ()
		{
			double change = (Animated) ? DateTime.Now.Subtract (delta_time).TotalMilliseconds / fade_ms : 10;
			
			delta_time = DateTime.Now;
			if (ExpandNeeded) {
				drawing_area.Width += (int) ((ThreePaneWidth-TwoPaneWidth)*change);
				drawing_area.Width = (drawing_area.Width > ThreePaneWidth) ? ThreePaneWidth : drawing_area.Width;
				drawing_area.X = (WindowWidth - drawing_area.Width) / 2;
			} else if (ShrinkNeeded) {
				drawing_area.Width -= (int) ((ThreePaneWidth-TwoPaneWidth)*change);
				drawing_area.Width = (drawing_area.Width < TwoPaneWidth) ? TwoPaneWidth : drawing_area.Width;
				drawing_area.X = (WindowWidth - drawing_area.Width) / 2;
			}
			
			if (TextScaleNeeded) {
				if (entry_mode[(int) Focus]) {
					text_box_scale += change;
					text_box_scale = (text_box_scale > 1) ? 1 : text_box_scale;
				} else {
					text_box_scale -= change;
					text_box_scale = (text_box_scale < 0) ? 0 : text_box_scale;
				}
			}
			
			if (FadeNeeded) {
				if (text_box_scale == 1) {
					icon_fade[0] = icon_fade[1] = icon_fade[2] = 1;
				}
				icon_fade[0] += change;
				icon_fade[1] += change;
				icon_fade[2] += change;
				
				icon_fade[0] = (icon_fade[0] > 1) ? 1 : icon_fade[0];
				icon_fade[1] = (icon_fade[1] > 1) ? 1 : icon_fade[1];
				icon_fade[2] = (icon_fade[2] > 1) ? 1 : icon_fade[2];
			}
			
			if (WindowFadeNeeded) {
				window_fade += (change*.1)+(window_fade/2);
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
//			Draw ();
			Paint ();
			return ret;
		}
		
		private void OnGtkThemeChanged (object o, GConf.NotifyEventArgs args)
		{
			GLib.Timeout.Add (3000, () => {
				if (GtkThemeChanged != null)
					GtkThemeChanged (o, args);
				Colors.RebuildColors (BackgroundColor);
				return false;
			});
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
				RenderPaneText (pane, cr, "No results for: " + Context.GetPaneQuery (pane));
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
					RenderPaneText (pane, cr, "Type To Search");
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
			PaneOutlineRenderer.RenderElement (cr, render_region, (Focus == pane));
		}
		
		private void RenderPixbuf (Pane pane, Context cr)
		{
			Element obj = Context.GetPaneObject (pane);
			RenderPixbuf (pane, cr, obj.Icon, 1);
		}
		
		private void RenderPixbuf (Pane pane, Context cr, string icon, double alpha)
		{
			int offset = PaneOffset (pane);
			if (!SurfaceCache.ContainsKey (icon)) {
				BufferIcon (cr, icon);
			}
			
			string sec_icon = "";
			if (OldContext.GetPaneObject (pane) != null)
				sec_icon = OldContext.GetPaneObject (pane).Icon;
			
			double calc_alpha = (sec_icon != icon) ? icon_fade[(int) pane] : 1;
			int x;
			int y;
			if (PaneOutlineRenderer.StackIconText) {
				x = drawing_area.X + offset + ((BoxWidth/2)-(IconSize/2));
				y = drawing_area.Y + WindowBorder + TitleBarHeight + 3;
			} else {
				x = drawing_area.X + offset + 5;
				y = drawing_area.Y + WindowBorder + TitleBarHeight + ((BoxHeight-IconSize)/2);
			}
			cr.SetSource (SurfaceCache.GetSurface (icon), x, y);
			cr.PaintWithAlpha (calc_alpha * alpha);
			if (string.IsNullOrEmpty (sec_icon) || calc_alpha < 1) 
				return;
			
			if (!SurfaceCache.ContainsKey (OldContext.GetPaneObject (pane).Icon)) {
				BufferIcon (cr, OldContext.GetPaneObject (pane).Icon);
			}
			cr.SetSource (SurfaceCache.GetSurface (OldContext.GetPaneObject (pane).Icon), x, y);
			cr.PaintWithAlpha (alpha * (1 - calc_alpha));
		}
		
		private void BufferIcon (Context cr, string icon)
		{
			SurfaceCache.AddPixbufSurface (icon, icon);
		}
		
		void RenderDescriptionText (Context cr)
		{
			if (Context.GetPaneObject (Focus) == null)
				return;
			BezelTextUtils.RenderLayoutText (cr, GLib.Markup.EscapeText (Context.GetPaneObject (Focus).Description), drawing_area.X + 10,
			                                 drawing_area.Y + InternalHeight - WindowBorder - 4, drawing_area.Width - 20, this);
		}
		
		void RenderPaneText (Pane pane, Context cr)
		{
			if (Context.GetPaneObject (pane) != null)
				RenderPaneText (pane, cr, GLib.Markup.EscapeText (Context.GetPaneObject (pane).Name));
		}
		
		void RenderPaneText (Pane pane, Context cr, string text)
		{
			if (text.Length == 0) return;
			text = string.Format ("<b>{0}</b>", text);
			
			if (Context.GetPaneTextMode (pane)) {
				Pango.Color color = new Pango.Color ();
				color.Blue = color.Green = color.Red = ushort.MaxValue;
				int y = drawing_area.Y + WindowBorder + TitleBarHeight + 6;
				BezelTextUtils.RenderLayoutText (cr, text, drawing_area.X + PaneOffset (pane) + 5, y, BoxWidth - 10, 
				                                 color, Pango.Alignment.Left, Pango.EllipsizeMode.None, this);
			} else {
				text = (!string.IsNullOrEmpty (Context.GetPaneQuery (pane))) ? 
					Util.FormatCommonSubstrings 
						(text, Context.GetPaneQuery (pane), HighlightFormat) : text;
				if (PaneOutlineRenderer.StackIconText) {
					int y = drawing_area.Y + WindowBorder + TitleBarHeight + BoxHeight - TextHeight - 9;
					BezelTextUtils.RenderLayoutText (cr, text, drawing_area.X + PaneOffset (pane) + 5, y, BoxWidth - 10, this);
				} else {
					int y = drawing_area.Y + WindowBorder + TitleBarHeight + (int)(BoxHeight/2);
					BezelTextUtils.RenderLayoutText (cr, text, drawing_area.X + PaneOffset (pane) + IconSize + 10, y, BoxWidth - IconSize - 20, this);
				}
			}
		}
		
		void RenderTextModeText (Context cr)
		{
			Pango.Color color = new Pango.Color ();
			color.Blue = color.Red = color.Green = (ushort) (ushort.MaxValue * text_box_scale);
			int tmp = BezelTextUtils.TextHeight;
			BezelTextUtils.TextHeight = 18;
			Gdk.Rectangle cursor = BezelTextUtils.RenderLayoutText (cr, GLib.Markup.EscapeText (Context.GetPaneQuery (Focus)), 
			                                                        drawing_area.X + 10, drawing_area.Y + TextModeOffset + 5, 
			                                                        drawing_area.Width - 20, color, 
			                                                        Pango.Alignment.Left, Pango.EllipsizeMode.None, this);
			BezelTextUtils.TextHeight = tmp;
			if (cursor.X == cursor.Y && cursor.X == 0) return;
			
			cr.Rectangle (cursor.X, cursor.Y, 2, cursor.Height);
			cr.Color = new Cairo.Color (.4, .5, 1, .85);
			cr.Fill ();
		}
		
		void RenderTextModeOverlay (Context cr) 
		{
			TextModeOverlayRenderer.RenderElement (cr, drawing_area, text_box_scale);
		}
		
		void RenderTitleBar (Context cr)
		{
			TitleBarRenderer.RenderElement (cr, drawing_area);
		}
		
		public PointLocation GetPointLocation (Gdk.Point point)
		{
			if (BackgroundRenderer.GetPointLocation (drawing_area, point) == PointLocation.Outside)
				return PointLocation.Outside;
			return TitleBarRenderer.GetPointLocation (drawing_area, point);
		}
	}
}

				

			
			
			