using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(Precept_GravshipLaunch), nameof(Precept_GravshipLaunch.ExposeData))]
public static class Precept_GravshipLaunch_ExposeData_Patch
{
    private static void Prefix(Precept_GravshipLaunch __instance, ref bool __state)
    {
        if (Scribe.mode == LoadSaveMode.LoadingVars && __instance.sourcePattern == null)
            __state = true;
    }

    private static void Postfix(Precept_GravshipLaunch __instance, bool __state)
    {
        if (!__state)
            return;

        if (__instance.def == VGEDefOf.VGE_GravjumperLaunch)
        {
            __instance.sourcePattern = DefDatabase<RitualPatternDef>.GetNamed("VGE_GravjumperLaunch");
            __instance.obligationTriggers = [];
            DefDatabase<RitualPatternDef>.GetNamed("VGE_GravjumperLaunch").Fill(__instance);
        }
        else if (__instance.def == VGEDefOf.VGE_GravhulkLaunch)
        {
            __instance.sourcePattern = DefDatabase<RitualPatternDef>.GetNamed("VGE_GravhulkLaunch");
            __instance.obligationTriggers = [];
            DefDatabase<RitualPatternDef>.GetNamed("VGE_GravhulkLaunch").Fill(__instance);
        }
    }
}