using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded
{
    [HotSwappable]
    [HarmonyPatch(typeof(Designator_MoveGravship), "IsValidCell")]
    public static class Designator_MoveGravship_IsValidCell_Patch
    {
        public static bool ignore;
        public static bool Prefix(IntVec3 cell, Map map, ref AcceptanceReport __result)
        {
            if (ignore)
            {
                return true;
            }
            __result = IsValidCell(cell, map);
            return false;
        }

        public static AcceptanceReport IsValidCell(IntVec3 cell, Map map)
        {
            if (!cell.InBounds(map))
            {
                return "GravshipOutOfBounds".Translate();
            }
            if (!cell.InBounds(map, 1) || cell.InNoBuildEdgeArea(map))
            {
                return "GravshipInNoBuildArea".Translate();
            }
            if (map.landingBlockers != null)
            {
                foreach (CellRect landingBlocker in map.landingBlockers)
                {
                    if (landingBlocker.Contains(cell))
                    {
                        return "GravshipInBlockedArea".Translate();
                    }
                }
            }
            GravshipMapGenUtility.GetBlockingThingsInCell(cell, map);
            if (!GenConstruct.CanBuildOnTerrain(TerrainDefOf.Substructure, cell, map, Rot4.North))
            {
                return "GravshipBlockedByTerrain".Translate(cell.GetTerrain(map));
            }
            return true;
        }

    }
}
