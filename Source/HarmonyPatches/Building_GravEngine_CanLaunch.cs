using HarmonyLib;
using PipeSystem;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(Building_GravEngine), nameof(Building_GravEngine.CanLaunch))]
public static class VanillaGravshipExpanded_Building_GravEngine_CanLaunch_Patch
{
    private static void Postfix(CompPilotConsole console, ref AcceptanceReport __result)
    {
        if (console.parent.Map?.gameConditionManager.ConditionIsActive(VGEDefOf.VGE_GravitationalAnomaly) ==true)
        {
            __result= new AcceptanceReport("VGE_CannotLaunchGravAnomaly".Translate().CapitalizeFirst());
        }
    }
}