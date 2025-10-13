using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse.Noise;
using Verse.AI;

namespace VanillaGravshipExpanded
{
    public class CompGravMaintainable : ThingComp
    {
        public CompProperties_GravMaintainable Props => props as CompProperties_GravMaintainable;

        public float maintenance = 1;
        public CompBreakdownable compBreakdownable;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref this.maintenance, "maintenance", 1, false);

            if (Scribe.mode == LoadSaveMode.LoadingVars)
                InitializeComps();
        }

        public override void PostPostMake()
        {
            base.PostPostMake();
            InitializeComps();
        }

        private void InitializeComps()
        {
            // Initialize comps from ExposeData and PostMake to best handle minifiable stuff, handling it the same way as vanilla does it.
            // This ensures we only need to grab the comp once (when building is made or game loaded), rather than every time it spawns.
            compBreakdownable = this.parent.GetComp<CompBreakdownable>();
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);


            FleckSystem system = parent.Map.flecks.CreateFleckSystemFor(VGEDefOf.VGE_MaintenanceSmoke);
            system.Prewarm(VGEDefOf.VGE_MaintenanceSmoke.Lifetime, null, delegate
            {
                if (maintenance < Props.minMaintenanceForAlert)
                {
                    EmissionTick(system);
                }             
            });
            parent.Map.flecks.HandOverSystem(system);

            GravMaintainables_MapComponent mapComp = parent.Map?.GetComponent<GravMaintainables_MapComponent>();
            if (mapComp != null)
            {
                mapComp.AddMaintainableToMap(this.parent);
            }
        }
        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            GravMaintainables_MapComponent mapComp = parent.Map?.GetComponent<GravMaintainables_MapComponent>();
            if (mapComp != null)
            {
                mapComp.RemoveMaintainableFromMap(this.parent);
            }
            base.PostDestroy(mode, previousMap);
        }

        public override void CompTickInterval(int delta)
        {
            if (!parent.Spawned)
                return;

            if (this.parent.IsHashIntervalTick(GenTicks.TickLongInterval, delta))
            {
                TickInterval();
            }
        }

        public override void CompTick()
        {
            if (!parent.Spawned)
                return;

            if (maintenance < Props.minMaintenanceForAlert)
            {
                EmissionTick(parent.Map.flecks);
            }
        }

        public override void CompTickRare()
        {
            if (!parent.Spawned)
                return;

            if (this.parent.IsHashIntervalTick(GenTicks.TickLongInterval, GenTicks.TickRareInterval))
            {
                TickInterval();
            }
            if (maintenance < Props.minMaintenanceForAlert)
            {
                EmissionTick(parent.Map.flecks);
            }
        }


        public override void CompTickLong()
        {
            if (!parent.Spawned)
                return;

            TickInterval();
            if (maintenance < Props.minMaintenanceForAlert)
            {
                EmissionTick(parent.Map.flecks);
            }

        }

        private void TickInterval()
        {
            if (maintenance > 0)
            {
                maintenance -= (1f / 1800) * this.parent.GetStatValue(VGEDefOf.VGE_MaintenanceSensitivity);


            }

            if (maintenance <= 0)
            {
                maintenance = 0.05f;
                Signal_Breakdown();
            }
        }

        public void Signal_Breakdown()
        {
            if (compBreakdownable != null)
            {
                compBreakdownable.DoBreakdown();
            }
          
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (DebugSettings.ShowDevGizmos)
            {

                Command_Action command_Action_Debug = new Command_Action();
                command_Action_Debug.defaultLabel = "Set maintenance to 2%";
                command_Action_Debug.action = delegate
                {
                    maintenance = 0.02f;
                };
                yield return command_Action_Debug;

                Command_Action command_Action_Debug2 = new Command_Action();
                command_Action_Debug2.defaultLabel = "Reduce maintenance by 10%";
                command_Action_Debug2.action = delegate
                {
                    maintenance -= 0.1f;
                };
                yield return command_Action_Debug2;

            }



        }

        public override string CompInspectStringExtra()
        {
            return "VGE_Maintenance".Translate(maintenance.ToStringPercent("F2"));
        }

       

        private void EmissionTick(IFleckCreator fleckDestination)
        {
            if (!(Rand.Value < Props.fleckEmissionRate))
            {
                return;
            }

            Vector3 vector = parent.TrueCenter() + new Vector3(0.5f, 0.0f, 0.5f) + Rand.InsideUnitCircleVec3 * 0.2f;
            if (vector.ToIntVec3().ShouldSpawnMotesAt(parent.Map))
            {
                FleckCreationData dataStatic = FleckMaker.GetDataStatic(vector, parent.Map, VGEDefOf.VGE_MaintenanceSmoke, 4);
                dataStatic.rotationRate = Rand.Range(-10, 10);
                dataStatic.velocityAngle = Rand.Range(-20, 20);
                dataStatic.velocitySpeed = Rand.Range(0.5f, 0.7f);
                if (fleckDestination is FleckSystem)
                {
                    dataStatic.spawnPosition.y = dataStatic.def.altitudeLayer.AltitudeFor(dataStatic.def.altitudeLayerIncOffset);
                }
                fleckDestination.CreateFleck(dataStatic);
            }
        }


    }


}

