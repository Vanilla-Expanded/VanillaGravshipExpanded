
using Verse;
using System;
using RimWorld;
using System.Collections.Generic;
using System.Linq;


namespace VanillaGravshipExpanded
{
    [StaticConstructorOnStartup]
    public static class StaticCollections
    {
        public static List<ThingDef> gravMaintainables = new List<ThingDef>();
     

        static StaticCollections()
        {

            gravMaintainables = DefDatabase<ThingDef>.AllDefsListForReading.Where(x => x.GetCompProperties<CompProperties_GravMaintainable>() != null).ToList();
               
            
        }




    }
}
