using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded
{
    public class GameCondition_AsteroidShower : GameCondition_SpawningProjectileEvent
    {
        protected override int GetNextSpawnInterval()
        {
            return CurrentPhase switch
            {
                EventPhase.Buildup or EventPhase.FadeOut => Rand.RangeInclusive(100, 200),
                EventPhase.Peak => Rand.RangeInclusive(20, 60),
                _ => 999,
            };
        }

        protected override void SpawnProjectile()
        {
            Map map = SingleMap;
            if (!CellFinder.TryFindRandomEdgeCellWith((c) => c.Standable(map) && !c.Roofed(map), map, 0f, out IntVec3 spawnCell))
            {
                return;
            }

            Vector3 directionToCenter = (map.Center.ToVector3() - spawnCell.ToVector3()).normalized;
            float baseAngle = Quaternion.LookRotation(directionToCenter).eulerAngles.y;
            float finalAngle = baseAngle + Rand.Range(-25f, 25f);

            Quaternion rotation = Quaternion.AngleAxis(finalAngle, Vector3.up);
            Vector3 direction = rotation * Vector3.forward;

            IntVec3 targetCell = spawnCell + (direction * 1000f).ToIntVec3();

            ThingDef asteroidType = GetRandomAsteroidType();
            Projectile projectile = (Projectile)GenSpawn.Spawn(asteroidType, spawnCell, map);
            projectile.Launch(null, spawnCell.ToVector3Shifted(), targetCell, targetCell, ProjectileHitFlags.All);
        }

        private ThingDef GetRandomAsteroidType()
        {
            var options = new List<Pair<ThingDef, float>>();

            switch (CurrentPhase)
            {
                case EventPhase.Buildup:
                case EventPhase.FadeOut:
                    options.Add(new Pair<ThingDef, float>(VGEDefOf.VGE_SmallAsteroid, 0.7f));
                    options.Add(new Pair<ThingDef, float>(VGEDefOf.VGE_MediumAsteroid, 0.3f));
                    break;

                case EventPhase.Peak:
                    options.Add(new Pair<ThingDef, float>(VGEDefOf.VGE_SmallAsteroid, 0.25f));
                    options.Add(new Pair<ThingDef, float>(VGEDefOf.VGE_MediumAsteroid, 0.40f));
                    options.Add(new Pair<ThingDef, float>(VGEDefOf.VGE_LargeAsteroid, 0.35f));
                    break;

                default:
                    options.Add(new Pair<ThingDef, float>(VGEDefOf.VGE_SmallAsteroid, 1.0f));
                    break;
            }

            return options.RandomElementByWeight(pair => pair.Second).First;
        }
    }
}
