using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace VanillaGravshipExpanded
{
    public class WorkGiver_MaintainGrav : WorkGiver_Scanner
    {
        public static string NotStudied;

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            return pawn.Map?.GetComponent<GravMaintainables_MapComponent>().maintainables_InMap;
        }

        public override PathEndMode PathEndMode
        {
            get
            {
                return PathEndMode.Touch;
            }
        }

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            return pawn.Map?.GetComponent<GravMaintainables_MapComponent>().maintainables_InMap.Count == 0;
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            CompGravMaintainable comp = t.TryGetComp<CompGravMaintainable>();

            if (comp is null)
            {
                return false;
            }

            if (pawn.Map is null)
            {
                return false;
            }

            if (t.Faction != pawn.Faction)
            {
                return false;
            }

            if (t.IsForbidden(pawn))
            {
                return false;
            }
           
            if (!pawn.CanReserve(t, 1, -1, null, forced))
            {
                return false;
            }
            if (pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Deconstruct) != null)
            {
                return false;
            }
            if (t.IsBurning())
            {
                return false;
            }

            if (comp.maintenance > MaintenanceThreshold_WorldComponent.Instance?.maintenanceThreshold)
            {

                return false;
            }

            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return new Job(VGEDefOf.VGE_MaintainGrav, t);
        }
    }
}
