using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;
using RimWorld.Planet;

namespace VanillaGravshipExpanded
{
    public class GravMaintainables_MapComponent : MapComponent
    {

        public HashSet<Thing> maintainables_InMap = new HashSet<Thing>();

      

        public GravMaintainables_MapComponent(Map map) : base(map)
        {
        }

       



        public void AddMaintainableToMap(Thing thing)
        {
            if (!maintainables_InMap.Contains(thing))
            {
                maintainables_InMap.Add(thing);
            }
        }

        public void RemoveMaintainableFromMap(Thing thing)
        {
            if (maintainables_InMap.Contains(thing))
            {
                maintainables_InMap.Remove(thing);
            }
        }

        public float AverageMaintenanceInMap()
        {
            float totalMaintenance = 0;
            if (maintainables_InMap.Count > 0) {
                foreach (Thing thing in maintainables_InMap)
                {
                    totalMaintenance+=thing.TryGetComp<CompGravMaintainable>().maintenance;
                }
                return totalMaintenance/ maintainables_InMap.Count;
            } else return 1;         
        }

        public void ChangeGlobalMaintenance(float amount, float chance)
        {
            if (maintainables_InMap.Count > 0)
            {
                foreach (Thing thing in maintainables_InMap)
                {
                    if (Rand.Chance(chance))
                    {
                        CompGravMaintainable comp = thing.TryGetComp<CompGravMaintainable>();
                        comp.maintenance += amount * thing.GetStatValue(VGEDefOf.VGE_MaintenanceSensitivity);
                    }
                    
                }
                
            }

        }
    }
}
