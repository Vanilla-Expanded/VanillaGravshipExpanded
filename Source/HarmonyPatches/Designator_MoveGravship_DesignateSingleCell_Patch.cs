using HarmonyLib;
using RimWorld;
using System.Linq;
using Verse;

namespace VanillaGravshipExpanded
{
    [HarmonyPatch(typeof(Designator_MoveGravship), "DesignateSingleCell")]
    public static class Designator_MoveGravship_DesignateSingleCell_Patch
    {
        public static bool Prefix(Designator_MoveGravship __instance, IntVec3 c)
        {
            var things = GravshipMapGenUtility.GetBlockingThings(__instance.marker.GravshipCells.Select((IntVec3 cell) => cell + c), __instance.Map);
            if (things.Any())
            {
                Messages.Message("VGE_CrashLandingWarning".Translate(), MessageTypeDefOf.CautionInput);
            }
            return true;
        }
    }
}
