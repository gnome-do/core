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
using System.Linq;

using Cairo;
using Gdk;
using Gtk;

using Docky.Utilities;
using Docky.Core;

namespace Docky.Interface
{
	
	
	[System.ComponentModel.ToolboxItem(true)]
	public partial class DockyConfigurationWidget : Gtk.Bin
	{
		NodeView docklets_nodeview;
		
		[TreeNode (ListOnly=true)]
		class DockletTreeNode : TreeNode {
			AbstractDockletItem docklet;
			
			public DockletTreeNode (AbstractDockletItem docklet) 
			{
				this.docklet = docklet;
			}
			
			[TreeNodeValue (Column=0)]
			public bool Enabled { 
				get { return DockServices.DockletService.ActiveDocklets.Contains (docklet); }
			}
			
			[TreeNodeValue (Column=1)]
			public string Name {
				get { return docklet.Name; }
			}
			
			public void Toggle ()
			{
				DockServices.DockletService.ToggleDocklet (docklet);
			}
		}
		
		bool setup = false;
		
		NodeStore store;
		NodeStore Store {
			get {
				if (store == null) {
					store = new NodeStore (typeof (DockletTreeNode));
					
				}
				return store;
			}
		}
		
		public DockyConfigurationWidget()
		{
			setup = true;
			this.Build();
			
			zoom_scale.Digits = 1;
			zoom_scale.SetRange (1.1, 4);
			zoom_scale.SetIncrements (.1, .1);
			zoom_scale.Value = DockPreferences.ZoomPercent;
			
			zoom_width_scale.Digits = 0;
			zoom_width_scale.SetRange (200, 500);
			zoom_width_scale.SetIncrements (10, 10);
			zoom_width_scale.Value = DockPreferences.ZoomSize;
			
			advanced_indicators_checkbutton.Active = DockPreferences.IndicateMultipleWindows;
			autohide_checkbutton.Active = DockPreferences.AutoHide;
			window_overlap_checkbutton.Active = DockPreferences.AllowOverlap;
			zoom_checkbutton.Active = DockPreferences.ZoomEnabled;
			
			orientation_combobox.AppendText (DockOrientation.Bottom.ToString ());
			orientation_combobox.AppendText (DockOrientation.Top.ToString ());
			orientation_combobox.Active = DockPreferences.Orientation == DockOrientation.Bottom ? 0 : 1;
			
			BuildDocklets ();
			
			SetSensativity ();
			
			DockPreferences.IconSizeChanged += HandleIconSizeChanged; 
			
			Gtk.Application.Invoke (delegate { setup = false; });
		}

		void HandleIconSizeChanged()
		{
			SetSensativity ();
		}
		
		void BuildDocklets ()
		{
			docklets_nodeview = new NodeView (Store);
			docklets_nodeview.RulesHint = true;
			scrolled_window.Add (docklets_nodeview);
			
			Gtk.CellRendererToggle toggle = new Gtk.CellRendererToggle ();
			toggle.Toggled += HandleToggled;
			docklets_nodeview.AppendColumn ("Enabled", toggle, "active", 0);
			docklets_nodeview.AppendColumn ("Name", new Gtk.CellRendererText (), "text", 1);
			
			docklets_nodeview.HeadersVisible = false;
			
			foreach (AbstractDockletItem adi in DockServices.DockletService.Docklets) {
				Store.AddNode (new DockletTreeNode (adi));
			}
			
			scrolled_window.ShowAll ();
		}
		
		void SetSensativity ()
		{
			zoom_scale.Sensitive = 
				zoom_width_scale.Sensitive = 
					zoom_size_label.Sensitive = 
					zoom_width_label.Sensitive =
					DockPreferences.ZoomEnabled;
		}

		void HandleToggled (object o, ToggledArgs args)
		{
			DockletTreeNode node = Store.GetNode (new Gtk.TreePath (args.Path)) as DockletTreeNode;
			node.Toggle ();
		}
		
		protected virtual void OnZoomScaleFormatValue (object o, Gtk.FormatValueArgs args)
		{
			args.RetVal = string.Format ("{0}%", Math.Round (args.Value * 100));
		}

		protected virtual void OnZoomScaleValueChanged (object sender, System.EventArgs e)
		{
			if (setup) return;
			if (!(sender is HScale)) return;
			
			HScale scale = sender as HScale;
			DockPreferences.ZoomPercent = scale.Value;
		}

		protected virtual void OnAdvancedIndicatorsCheckbuttonToggled (object sender, System.EventArgs e)
		{
			if (setup) return;
			DockPreferences.IndicateMultipleWindows = advanced_indicators_checkbutton.Active;
		}

		protected virtual void OnZoomCheckbuttonToggled (object sender, System.EventArgs e)
		{
			if (setup) return;
			DockPreferences.ZoomEnabled = zoom_checkbutton.Active;
		}

		protected virtual void OnWindowOverlapCheckbuttonToggled (object sender, System.EventArgs e)
		{
			if (setup) return;
			DockPreferences.AllowOverlap = window_overlap_checkbutton.Active;
		}

		protected virtual void OnAutohideCheckbuttonToggled (object sender, System.EventArgs e)
		{
			if (setup) return;
			DockPreferences.AutoHide = autohide_checkbutton.Active;
		}

		protected virtual void OnOrientationComboboxChanged (object sender, System.EventArgs e)
		{
			if (setup) return;
			DockPreferences.Orientation = (DockOrientation) orientation_combobox.Active;
		}

		protected virtual void OnZoomWidthScaleValueChanged (object sender, System.EventArgs e)
		{
			if (setup) return;
			DockPreferences.ZoomSize = (int) zoom_width_scale.Value;
		}
		
		public override void Dispose ()
		{
			DockPreferences.IconSizeChanged -= HandleIconSizeChanged;
			base.Dispose ();
		}

	}
}
