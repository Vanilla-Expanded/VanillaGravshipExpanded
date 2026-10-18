using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace VanillaGravshipExpanded
{
    [HarmonyPatch(typeof(Designator_MoveGravship), "ValidGravshipLocation")]
    public static class Designator_MoveGravship_ValidGravshipLocation_Patch
    {
        public static void Postfix(List<IntVec3> validCells, List<IntVec3> invalidCells, ref AcceptanceReport __result)
        {
            // Consider turning into a transpiler, this shit's slow
            if (!__result.Accepted && __result.Reason == "GravshipMustBeConnectedToLand".Translate())
            {
                if (validCells != null && invalidCells != null)
                {
                    validCells.AddRange(invalidCells);
                    invalidCells.Clear();
                }
                __result = true;
            }
        }
    }
}
