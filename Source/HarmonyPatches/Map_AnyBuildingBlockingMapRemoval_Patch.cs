using HarmonyLib;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(Map), nameof(Map.AnyBuildingBlockingMapRemoval), MethodType.Getter)]
public class Map_AnyBuildingBlockingMapRemoval_Patch
{
    private static void Postfix(Map __instance, ref bool __result)
    {
        // If false, check if there's any of our own engines on the map to prevent the map from being removed.
        if (!__result)
        {
            __result = __instance.listerThings.AnyThingWithDef(VGEDefOf.VGE_GravjumperEngine) ||
                       __instance.listerThings.AnyThingWithDef(VGEDefOf.VGE_GravhulkEngine);
        }
    }
}