using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded
{
    [HotSwappable]
    public static class ArtilleryUtility
    {
        public static IntVec3 FindSpawnCell(Map map, PlanetTile targetTile, PlanetTile startTile, IntVec3 targetCell)
        {
            float angle = GetAngle(targetTile, startTile);
            Vector3 centerPos = targetCell.ToVector3();
            Vector3 direction = Vector3Utility.FromAngleFlat(angle - 90);
            Vector3 current = centerPos;
            IntVec3 result = targetCell;
            while (true)
            {
                current += direction;
                IntVec3 currentCell = current.ToIntVec3();
                if (!currentCell.InBounds(map))
                {
                    return result;
                }
                result = currentCell;
            }
        }

        public static float GetAngle(PlanetTile targetTile, PlanetTile startTile)
        {
            Vector3 tileCenter = targetTile.Layer.GetTileCenter(targetTile);
            Vector3 tileCenter2 = startTile.Layer.GetTileCenter(startTile);
            float angle = Find.WorldGrid.GetHeadingFromTo(tileCenter, tileCenter2);
            return angle;
        }

        public static void SpawnArtilleryProjectile(PlanetTile targetTile, PlanetTile startTile, ThingDef projectileDef, Thing launcher, IntVec3 targetCell, float missRadius, float hitChance = 1.0f, Thing targetThing = null)
        {
            if (targetCell.IsValid is false)
            {
                Log.Error("[VGE] Attempted to spawn " + projectileDef + " with an invalid target cell. Aborting.");
                return;
            }
            var map = Find.Maps.Find(m => m.Tile == targetTile);
            if (map is null) return;
            var spawnCell = FindSpawnCell(map, targetTile, startTile, targetCell);
            IntVec3 finalTargetCell;
            LocalTargetInfo usedTarget;
            LocalTargetInfo intendedTarget = targetCell;
            if (missRadius > 0f)
            {
                finalTargetCell = targetCell + (Rand.InsideUnitCircle * missRadius).ToVector3().ToIntVec3();
                usedTarget = finalTargetCell;
            }
            else
            {
                if (Rand.Chance(hitChance) is false)
                {
                    ShootLine shootLine = new ShootLine(spawnCell, targetCell);
                    shootLine.ChangeDestToMissWild(hitChance, projectileDef.projectile.flyOverhead, map);
                    finalTargetCell = shootLine.Dest;
                    usedTarget = finalTargetCell;
                }
                else
                {
                    finalTargetCell = targetCell;
                    if (targetThing != null)
                    {
                        usedTarget = targetThing;
                        intendedTarget = targetThing;
                    }
                    else
                    {
                        usedTarget = targetCell;
                    }
                }
            }

            var projectile = (Projectile)GenSpawn.Spawn(projectileDef, spawnCell, map);
            projectile.Launch(launcher, spawnCell.ToVector3(), usedTarget, intendedTarget, ProjectileHitFlags.IntendedTarget | ProjectileHitFlags.NonTargetPawns | ProjectileHitFlags.NonTargetWorld);
        }
    }
}
