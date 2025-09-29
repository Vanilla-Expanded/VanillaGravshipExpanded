using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded
{
    [HotSwappable]
    public class GameCondition_SpaceDebris : GameCondition_SpawningProjectileEvent
    {
        private Rot4 spawnDirection = Rot4.Invalid;
        private float baseAngle;

        public override void Init()
        {
            base.Init();
            spawnDirection = Rot4.Random;
            Map map = SingleMap;
            CellFinder.TryFindRandomEdgeCellWith(c => c.Standable(map), map, spawnDirection, 0f, out IntVec3 startPoint);
            CellFinder.TryFindRandomEdgeCellWith(c => c.Standable(map), map, spawnDirection.Opposite, 0f, out IntVec3 endPoint);
            Vector3 directionToTarget = (endPoint.ToVector3() - startPoint.ToVector3()).normalized;
            this.baseAngle = Quaternion.LookRotation(directionToTarget).eulerAngles.y;
        }

        protected override int GetNextSpawnInterval()
        {
            return CurrentPhase switch
            {
                EventPhase.Buildup or EventPhase.FadeOut => Rand.RangeInclusive(200, 500),
                EventPhase.Peak => Rand.RangeInclusive(60, 150),
                _ => 999,
            };
        }

        protected override void SpawnProjectile()
        {
            Map map = SingleMap;
            if (!CellFinder.TryFindRandomEdgeCellWith((c) => c.Standable(map) && !c.Roofed(map), map, spawnDirection, 0f, out IntVec3 spawnCell))
            {
                return;
            }
            float finalAngle = this.baseAngle + Rand.Range(-5f, 5f);

            Quaternion rotation = Quaternion.AngleAxis(finalAngle, Vector3.up);
            Vector3 direction = rotation * Vector3.forward;

            IntVec3 targetCell = spawnCell + (direction * 1000f).ToIntVec3();

            ThingDef debrisType = GetRandomDebrisType();
            Projectile projectile = (Projectile)GenSpawn.Spawn(debrisType, spawnCell, map);
            projectile.Launch(null, spawnCell.ToVector3Shifted(), targetCell, targetCell, ProjectileHitFlags.All);
        }

        private ThingDef GetRandomDebrisType()
        {
            var options = new List<Pair<ThingDef, float>>();

            switch (CurrentPhase)
            {
                case EventPhase.Buildup:
                case EventPhase.FadeOut:
                    options.Add(new Pair<ThingDef, float>(VGEDefOf.VGE_SmallDebris, 0.8f));
                    options.Add(new Pair<ThingDef, float>(VGEDefOf.VGE_MediumDebris, 0.2f));
                    break;

                case EventPhase.Peak:
                    options.Add(new Pair<ThingDef, float>(VGEDefOf.VGE_SmallDebris, 0.25f));
                    options.Add(new Pair<ThingDef, float>(VGEDefOf.VGE_MediumDebris, 0.35f));
                    options.Add(new Pair<ThingDef, float>(VGEDefOf.VGE_LargeDebris, 0.40f));
                    break;

                default:
                    options.Add(new Pair<ThingDef, float>(VGEDefOf.VGE_SmallDebris, 1.0f));
                    break;
            }

            return options.RandomElementByWeight(pair => pair.Second).First;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref spawnDirection, "spawnDirection", Rot4.Invalid);
            Scribe_Values.Look(ref baseAngle, "baseAngle", 0f);
        }
    }
}
