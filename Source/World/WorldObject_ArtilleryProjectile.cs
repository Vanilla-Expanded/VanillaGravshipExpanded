using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded
{
    [HotSwappable]
    public class WorldObject_ArtilleryProjectile : WorldObject
    {
        public PlanetTile targetTile;
        public IntVec3 targetCell;
        public ThingDef projectileDef;
        public float missRadius;
        public Thing launcher;
        public override Texture2D ExpandingIcon => projectileDef.uiIcon;
        public override Color ExpandingIconColor => projectileDef.graphic.color;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref targetTile, "targetTile");
            Scribe_Values.Look(ref targetCell, "targetCell");
            Scribe_Defs.Look(ref projectileDef, "projectileDef");
            Scribe_Values.Look(ref missRadius, "missRadius");
            Scribe_References.Look(ref launcher, "launcher");
        }

        private const float TravelSpeed = 0.00025f * 2f;
        private float traveledPct;
        private float TraveledPctStepPerTick
        {
            get
            {
                Vector3 start = Start;
                Vector3 end = End;
                if (start == end)
                {
                    return 1f;
                }
                float num = GenMath.SphericalDistance(start.normalized, end.normalized);
                if (num == 0f)
                {
                    return 1f;
                }
                return TravelSpeed / num;
            }
        }
        private Vector3 Start => Find.WorldGrid.GetTileCenter(base.Tile);
        private Vector3 End => Find.WorldGrid.GetTileCenter(targetTile);
        public override Vector3 DrawPos => Vector3.Slerp(Start, End, traveledPct);
        public override void Tick()
        {
            base.Tick();
            traveledPct += TraveledPctStepPerTick;
            if (traveledPct >= 1f)
            {
                traveledPct = 1f;
                OnArrival();
            }
        }

        private void OnArrival()
        {
            Map targetMap = GetMap();
            if (targetMap != null)
            {
                SpawnProjectile(targetMap);
            }
            Destroy();
        }

        private void SpawnProjectile(Map map)
        {
            IntVec3 spawnCell = FindSpawnCell(map);
            IntVec3 finalTargetCell = targetCell + (Rand.InsideUnitCircle * missRadius).ToVector3().ToIntVec3();
            Projectile projectile = (Projectile)GenSpawn.Spawn(projectileDef, spawnCell, map);
            projectile.Launch(launcher, spawnCell.ToVector3(), finalTargetCell, targetCell, ProjectileHitFlags.IntendedTarget);
        }

        private Map GetMap()
        {
            return Find.Maps.Find(m => m.Tile == targetTile);
        }

        private IntVec3 FindSpawnCell(Map map)
        {
            float angle = Find.WorldGrid.GetHeadingFromTo(targetTile, Tile);
            var edgeCells = new CellRect(0, 0, map.Size.x, map.Size.z).EdgeCells;
            var centerPos = map.Center.ToVector3();
            IntVec3 targetCell = edgeCells.MinBy(c => Mathf.Abs(angle - (c.ToVector3() - centerPos).AngleFlat()));
            return targetCell;
        }

    }
}
