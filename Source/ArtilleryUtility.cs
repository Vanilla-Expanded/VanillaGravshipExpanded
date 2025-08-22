using RimWorld;
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
            float angle = Find.WorldGrid.GetHeadingFromTo(targetTile, startTile);
            var edgeCells = new CellRect(0, 0, map.Size.x, map.Size.z).EdgeCells;
            var centerPos = map.Center.ToVector3();
            IntVec3 result = edgeCells.MinBy(c => Mathf.Abs(angle - (c.ToVector3() - centerPos).AngleFlat()));
            return result;
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

    }
}
