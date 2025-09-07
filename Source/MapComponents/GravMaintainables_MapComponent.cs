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

        public float maintenanceThreshold = 0.7f;

        public GravMaintainables_MapComponent(Map map) : base(map)
        {
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref this.maintenanceThreshold, "maintenanceThreshold", 0.7f, false);
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
    }
}
