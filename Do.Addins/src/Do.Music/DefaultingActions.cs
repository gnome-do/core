// DefaultingActions.cs created with MonoDevelop
// User: zgold at 21:54 06/15/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using Do.Addins;
using Do.Universe;

namespace Do.Addins.DoMusic
{
	
	
	public class SetDefaultAction : AbstractAction
	{

		public override string Name{ get { return "Set As Music Source"; }	}
		public override string Description { get { return "Set the music source to be used by Gnome Do"; } }
		public override string Icon	{ get { return "gcdmaster-doc"; } }
		
		public override Type[] SupportedItemTypes { 
			get { 
				return new Type[] {	typeof (IItem) }; 
			} 
		}

		public override bool SupportsItem (IItem item) {
			/*
			 * A fairly ugly hack.  IItemSources themselves do not get put into the universe
			 * but a DoItemSource in its place does (DoItemSource : IItem, IItemSource).  
			 * Thus we can't test "item is IMusicSource".  Only thing we can do is 
			 * go through all known IMusicSource (DoMusic has this) and compare
			 * IItem.Name vs IMusicSource.Name
			 */
			foreach (IMusicSource ims in DoMusic.GetSources()) {		
				if (item.Name.Equals(ims.Name))
				   return true;
			}
			return false;
		}
	
		public override IItem[] Perform (IItem[] items, IItem[] modifierItems)
		{		
			if (items.Length < 1)
				return null;
			
			IItem item = items[0];
			if (item == null)
				return null;
			
			IMusicSource source = null;
			foreach (IMusicSource ims in DoMusic.GetSources ()) {				
				if (item.Name.Equals (ims.Name))
				   source = ims;
			}
			
			Configuration.setUseAllSources (false);
			Configuration.CurrentSource = source;
			DoMusic.UpdateItemSources ();
			return null;
		}
	}
	
	public class EnableAllSources : IRunnableItem 
	{
		public string Name {get {return "All Sources";} }
		public string Description {get {return "Enable use of all music sources in Gnome Do";} }
		public string Icon {get {return "gcdmaster";} }
		
		public void Run (){
			Configuration.setUseAllSources (true);
			DoMusic.UpdateItemSources ();
		}
	}
}
