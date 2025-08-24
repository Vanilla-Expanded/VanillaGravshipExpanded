using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded
{
    [HarmonyPatch(typeof(GravshipUtility), nameof(GravshipUtility.GetConnectedSubstructure))]
    public static class GravshipUtility_GetConnectedSubstructure_Patch
    {
        public static void Postfix(Building_GravEngine engine, HashSet<IntVec3> cells)
        {
            Map map = engine.Map;
            HashSet<IntVec3> subscaffoldCells = new HashSet<IntVec3>();
            map.floodFiller.FloodFill(engine.Position, delegate (IntVec3 x)
            {
                if (x.InBounds(map))
                {
                    TerrainDef terrainDef = map.terrainGrid.FoundationAt(x);
                    return terrainDef != null && (terrainDef == VGEDefOf.VGE_GravshipSubscaffold || terrainDef.IsSubstructure);
                }
                return false;
            }, delegate (IntVec3 x)
            {
                if (!cells.Contains(x))
                {
                    subscaffoldCells.Add(x);
                }
                return false;
            });
            foreach (IntVec3 subscaffoldCell in subscaffoldCells)
            {
                cells.Add(subscaffoldCell);
            }
        }
    }
}
