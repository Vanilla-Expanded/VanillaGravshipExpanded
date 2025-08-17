using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded
{
    [HarmonyPatch(typeof(Designator_MoveGravship), "Selected")]
    public static class Designator_MoveGravship_Selected_Patch
    {
        public static void Prefix()
        {
            GravshipMapGenUtility.Reset();
        }
    }
}
