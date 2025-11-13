using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(LearningGiver_Radiotalking), nameof(LearningGiver_Radiotalking.TryFindCommsConsole))]
public static class LearningGiver_Radiotalking_TryFindCommsConsole_Patch
{
    private static void Postfix(Pawn pawn, ref Thing commsConsole, ref bool __result)
    {
        // If we found a comms console, move on
        if (__result)
            return;

        // Try searching for a comms terminal as an alternative
        commsConsole = GenClosest.ClosestThingReachable(
            pawn.Position, pawn.Map, ThingRequest.ForDef(VGEDefOf.VGE_CommsTerminal), PathEndMode.InteractionCell, TraverseParms.For(pawn),
            validator: t => t is Building_CommsConsole { CanUseCommsNow: true } console && pawn.CanReserve(console) && !console.IsForbidden(pawn));

        __result = commsConsole != null;
    }
}