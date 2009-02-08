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

using Gdk;
using Cairo;

using Do.Interface;
using Do.Platform;

using Docky.Core;
using Docky.Utilities;

namespace Docky.Interface
{
	
	
	internal class HotSeatProxyItem : AbstractDockItem
	{
		AbstractDockItem inner;
		
		public override ClickAnimationType AnimationType {
			get {
				return inner.AnimationType;
			}
		}

		public override DateTime DockAddItem {
			get {
				return inner.DockAddItem;
			}
			set {
				inner.DockAddItem = value;
			}
		}

		public override int Height {
			get {
				return inner.Height;
			}
		}

		public override int Width {
			get {
				return inner.Width;
			}
		}

		public override bool IsAcceptingDrops {
			get {
				return inner.IsAcceptingDrops;
			}
		}

		public override DateTime LastClick {
			get {
				return inner.LastClick;
			}
		}

		public override int WindowCount {
			get {
				return inner.WindowCount;
			}
		}

		public override DateTime AttentionRequestStartTime {
			get {
				return inner.AttentionRequestStartTime;
			}
		}

		public override ScalingType ScalingType {
			get {
				return inner.ScalingType;
			}
		}

		public override bool NeedsAttention {
			get {
				return inner.NeedsAttention;
			}
		}

		public override bool ReceiveItem (string item)
		{
			return inner.ReceiveItem (item);
		}

		public override void SetIconRegion (Gdk.Rectangle region)
		{
			inner.SetIconRegion (region);
		}

		
		public HotSeatProxyItem(AbstractDockItem inner) : base ()
		{
			this.inner = inner;
		}
		
		protected override Pixbuf GetSurfacePixbuf (int size)
		{
			return null;
		}
		
		public override Surface GetIconSurface (Cairo.Surface similar, int targetSize, out int actualSize)
		{
			return inner.GetIconSurface (similar, targetSize, out actualSize);
		}
		
		public override Surface GetTextSurface (Cairo.Surface similar)
		{
			return inner.GetTextSurface (similar);
		}
		
		public override void Clicked (uint button, Gdk.ModifierType state, Gdk.Point position)
		{
			DockServices.ItemsService.ResetHotSeat (inner);
			base.Clicked (button, state, position);
		}
	}
}
