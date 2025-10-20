using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System.Reflection;
using Verse;

namespace VanillaGravshipExpanded
{
    [HarmonyPatch]
    public static class SpaceMapDropPatches
    {
        private static bool isSpaceTerrainModified = false;

        public static MethodBase[] TargetMethods()
        {
            return new[]
            {
                AccessTools.Method(typeof(MapGenerator), nameof(MapGenerator.GenerateContentsIntoMap)),
                AccessTools.Method(typeof(Scenario), nameof(Scenario.PostMapGenerate)),
            };
        }

        [HarmonyPriority(int.MinValue)]
        public static void Prefix()
        {
            if (!isSpaceTerrainModified)
            {
                TerrainDefOf.Space.passability = Traversability.Impassable;
                TerrainDefOf.Space.affordances.Remove(TerrainAffordanceDefOf.Walkable);
                
                isSpaceTerrainModified = true;
            }
        }

        [HarmonyPriority(int.MaxValue)]
        public static void Postfix()
        {
            if (isSpaceTerrainModified)
            {
                TerrainDefOf.Space.passability = Traversability.Standable;
                if (!TerrainDefOf.Space.affordances.Contains(TerrainAffordanceDefOf.Walkable))
                {
                    TerrainDefOf.Space.affordances.Add(TerrainAffordanceDefOf.Walkable);
                }
                isSpaceTerrainModified = false;
            }
        }
    }
}
