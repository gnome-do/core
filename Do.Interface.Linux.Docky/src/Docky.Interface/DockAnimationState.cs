// DockAnimationState.cs
// 
// Copyright (C) 2008 GNOME Do
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
using System.Linq;

using Do;
using Do.Interface;
using Do.Platform;
using Do.Universe;

namespace Docky.Interface
{
	public delegate bool AnimationConditionHandler ();
	
	public enum Animations {
		Open,
		Zoom,
		Bounce,
		Painter,
		Summon,
		IconInsert,
		UrgencyChanged,
		InputModeChanged,
		ActiveWindowChanged,
	}
	
	public class DockAnimationState : IDisposable
	{
		Dictionary<Animations, AnimationConditionHandler> animation_conditions;
		bool previous_animation_needed;
		
		public bool AnimationNeeded {
			get {
				// we will pass one additional animation requirement after none of our handlers say its needed.
				// this is to allow these handlers to "0" on the screen, and not just be in a half finished state
				bool animationNeeded = animation_conditions.Values.Any (handler => handler.Invoke ());
				bool retVal = previous_animation_needed || animationNeeded;
				previous_animation_needed = animationNeeded;
				return retVal;
			}
		}
		
		public DockAnimationState()
		{
			animation_conditions = new Dictionary<Animations, AnimationConditionHandler> ();
		}
		
		public void AddCondition (Animations id, AnimationConditionHandler handler)
		{
			if (animation_conditions.ContainsKey (id))
				throw new Exception (string.Format ("Animation Condition Handler already contains callback for {0}", id));
			
			animation_conditions [id] = handler;
		}

		public bool Contains (Animations id)
		{
			return animation_conditions.ContainsKey (id);
		}
		
		public bool this [Animations condition]
		{
			get { 
				if (!animation_conditions.ContainsKey (condition))
					throw new Exception (string.Format ("Animation Condition Handler does not contain a condition named {0}", condition));
				return animation_conditions [condition].Invoke (); 
			}
		}
		
		public void RemoveCondition (Animations id)
		{
			animation_conditions.Remove (id);
		}

		#region IDisposable implementation 
		
		public void Dispose ()
		{
			animation_conditions.Clear ();
		}
		
		#endregion 
		
	}
}
