using HarmonyLib;
using RimWorld;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(CompGravshipThruster), nameof(CompGravshipThruster.CanBeActive), MethodType.Getter)]
public static class CompGravshipThruster_CanBeActive_Patch
{
    private static void Postfix(CompGravshipThruster __instance, ref bool __result)
    {
        // If not active, don't change anything at all
        if (!__result)
            return;

        // If comp exists and doesn't have enough fuel, disable thruster
        var pipeComp = __instance.parent.GetComp<CompResourceThruster>();
        if (pipeComp is { HasFuel: false })
            __result = false;
    }
}