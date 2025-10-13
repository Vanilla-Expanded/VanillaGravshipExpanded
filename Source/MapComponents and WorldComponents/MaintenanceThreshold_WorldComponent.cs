using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;
using RimWorld.Planet;
using VEF.Buildings;

namespace VanillaGravshipExpanded
{
    public class MaintenanceThreshold_WorldComponent : WorldComponent
    {

        public static MaintenanceThreshold_WorldComponent Instance;

        public float maintenanceThreshold = 0.7f;

        public MaintenanceThreshold_WorldComponent(World world) : base(world) => Instance = this;
        

        public override void ExposeData()
        {
            Scribe_Values.Look(ref this.maintenanceThreshold, "maintenanceThreshold", 0.7f, false);
        }



     
    }
}
