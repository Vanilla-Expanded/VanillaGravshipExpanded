using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded
{
    [HarmonyPatch(typeof(WorkGiver_ConstructDeliverResources), "ShouldRemoveExistingFloorFirst")]
    public static class WorkGiver_ConstructDeliverResources_ShouldRemoveExistingFloorFirst_Patch
    {
        public static void Postfix(Pawn pawn, Blueprint blue, ref bool __result)
        {
            if (__result is false && blue.def.entityDefToBuild is TerrainDef def && def.IsSubstructure && pawn.Map.terrainGrid.TerrainAt(blue.Position) == VGEDefOf.VGE_DamagedSubstructure)
            {
                __result = true;
            }
        }
    }
}
