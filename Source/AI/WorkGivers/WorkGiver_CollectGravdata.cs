using System.Linq;
using Verse;
using Verse.AI;
using RimWorld;
using UnityEngine;

namespace VanillaGravshipExpanded
{
    public class WorkGiver_CollectGravdata : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest
        {
            get
            {
                if (World_ExposeData_Patch.currentGravtechProject == null)
                {
                    return ThingRequest.ForGroup(ThingRequestGroup.Nothing);
                }
                return ThingRequest.ForDef(VGEDefOf.VGE_GravtechConsole);
            }
        }

        public override bool Prioritized => true;

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            if (World_ExposeData_Patch.currentGravtechProject == null || pawn.Tile.LayerDef != PlanetLayerDefOf.Orbit)
            {
                return true;
            }
            return false;
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!pawn.CanReserve(t, 1, -1, null, forced) || (t.def.hasInteractionCell && !pawn.CanReserveSittableOrSpot(t.InteractionCell, forced)))
            {
                return false;
            }
            return CanResearchAt(pawn, t);
        }

        public static bool CanResearchAt(Pawn pawn, Thing t)
        {
            ResearchProjectDef currentProject = World_ExposeData_Patch.currentGravtechProject;
            if (currentProject == null)
            {
                return false;
            }
            if (pawn.WorkTypeIsDisabled(WorkTypeDefOf.Research) || pawn.WorkTagIsDisabled(WorkTags.Intellectual))
            {
                return false;
            }
            if (t.def != VGEDefOf.VGE_GravtechConsole)
            {
                return false;
            }
            if (t.Tile.LayerDef != PlanetLayerDefOf.Orbit)
            {
                return false;
            }

            if (!t.TryGetComp<CompPowerTrader>()?.PowerOn ?? true)
            {
                return false;
            }

            if (t.IsBrokenDown())
            {
                return false;
            }

            if (!new HistoryEvent(HistoryEventDefOf.Researching, pawn.Named(HistoryEventArgsNames.Doer)).Notify_PawnAboutToDo_Job())
            {
                return false;
            }

            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return JobMaker.MakeJob(VGEDefOf.VGE_CollectGravdata, t);
        }

        public override float GetPriority(Pawn pawn, TargetInfo t)
        {
            return pawn.GetStatValue(VGEDefOf.VGE_GravshipResearch);
        }
    }
}
