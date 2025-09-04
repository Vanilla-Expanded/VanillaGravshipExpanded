using System.Collections.Generic;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded
{
    [HotSwappable]
    public static class ArtilleryUtility
    {
        public static IntVec3 FindSpawnCell(Map map, PlanetTile targetTile, PlanetTile startTile)
        {
            float angle = GetAngle(targetTile, startTile);
            var edgeCells = new CellRect(0, 0, map.Size.x, map.Size.z).EdgeCells;
            var centerPos = map.Center.ToVector3();
            IntVec3 result = edgeCells.MinBy(c => Mathf.Abs(angle - (c.ToVector3() - centerPos).AngleFlat()));
            return result;
        }

        public static float GetAngle(PlanetTile targetTile, PlanetTile startTile)
        {
            Vector3 tileCenter = targetTile.Layer.GetTileCenter(targetTile);
            Vector3 tileCenter2 = startTile.Layer.GetTileCenter(startTile);
            float angle = Find.WorldGrid.GetHeadingFromTo(tileCenter, tileCenter2);
            return angle;
        }

        public static void SpawnArtilleryProjectile(PlanetTile targetTile, PlanetTile startTile, ThingDef projectileDef, Thing launcher, IntVec3 targetCell, float missRadius, float hitChance = 1.0f)
        {
            var map = Find.Maps.Find(m => m.Tile == targetTile);
            var spawnCell = FindSpawnCell(map, targetTile, startTile);
            IntVec3 finalTargetCell;
            if (missRadius > 0f)
            {
                finalTargetCell = targetCell + (Rand.InsideUnitCircle * missRadius).ToVector3().ToIntVec3();
            }
            else
            {
                if (Rand.Chance(hitChance) is false)
                {
                    ShootLine shootLine = new ShootLine(spawnCell, targetCell);
                    shootLine.ChangeDestToMissWild(hitChance, projectileDef.projectile.flyOverhead, map);
                    finalTargetCell = shootLine.Dest;
                }
                else
                {
                    finalTargetCell = targetCell;
                }
            }

            var projectile = (Projectile)GenSpawn.Spawn(projectileDef, spawnCell, map);
            projectile.Launch(launcher, spawnCell.ToVector3(), finalTargetCell, targetCell, ProjectileHitFlags.IntendedTarget | ProjectileHitFlags.NonTargetPawns | ProjectileHitFlags.NonTargetWorld);
        }

        private static PlanetTile cachedOrigin;
        private static PlanetTile cachedDest;
        private static int cachedDistance;
        private static PlanetLayer cachedOriginLayer;
        private static PlanetLayer cachedDestLayer;
        private static readonly List<PlanetLayerConnection> connections = new List<PlanetLayerConnection>();

        public static int GetDistanceDistance(PlanetTile from, PlanetTile to)
        {
            if (cachedOrigin == from && cachedDest == to)
            {
                return cachedDistance;
            }
            cachedOrigin = from;
            cachedDest = to;
            cachedDistance = 0;
            if (from.Layer != to.Layer)
            {
                if (cachedOriginLayer == from.Layer && cachedDestLayer == to.Layer)
                {
                }
                else
                {
                    if (!from.Layer.TryGetPath(to.Layer, connections, out var cost))
                    {
                        connections.Clear();
                        return 0;
                    }
                    cachedOriginLayer = to.Layer;
                    cachedDestLayer = from.Layer;
                    connections.Clear();
                }
                from = to.Layer.GetClosestTile_NewTemp(from);
            }
            cachedDistance = Find.WorldGrid.TraversalDistanceBetween(from, to);
            return cachedDistance;
        }
    }
}
