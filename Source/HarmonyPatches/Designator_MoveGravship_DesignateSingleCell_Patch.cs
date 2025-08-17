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
            if (GravshipMapGenUtility.BlockingThings.Any())
            {
                Messages.Message("VGE_CrashLandingWarning".Translate(), MessageTypeDefOf.CautionInput);
            }
            return true;
        }
    }
}
