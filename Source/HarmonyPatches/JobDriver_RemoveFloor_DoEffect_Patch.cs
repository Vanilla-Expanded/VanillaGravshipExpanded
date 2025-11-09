using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded
{
    [HarmonyPatch(typeof(JobDriver_RemoveFloor), "DoEffect")]
    public static class JobDriver_RemoveFloor_DoEffect_Patch
    {
        public static void Postfix(JobDriver_RemoveFloor __instance, IntVec3 c)
        {
            if (__instance.Map.terrainGrid.CanRemoveTopLayerAt(c) is false && __instance.Map.terrainGrid.FoundationAt(c) == VGEDefOf.VGE_DamagedSubstructure)
            {
                __instance.Map.terrainGrid.RemoveFoundation(c, true);
                FilthMaker.RemoveAllFilth(c, __instance.Map);
            }
        }

    }
}
