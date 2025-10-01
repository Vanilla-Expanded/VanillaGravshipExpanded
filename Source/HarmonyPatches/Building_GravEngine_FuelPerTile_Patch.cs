using HarmonyLib;
using RimWorld;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(Building_GravEngine), nameof(Building_GravEngine.FuelPerTile), MethodType.Getter)]
public static class Building_GravEngine_FuelPerTile_Patch
{
    private static void Postfix(ref float __result)
    {
        // As opposed to patch in GravshipUtility_TryGetPathFuelCost_Patch (check its comments for more details),
        // this one only affects the UI - it's only used in CompPilotConsole:CompInspectStringExtra.
        __result /= 2f;
    }
}