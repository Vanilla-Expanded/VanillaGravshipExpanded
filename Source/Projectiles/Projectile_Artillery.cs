using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded
{
    [HotSwappable]
    public class Projectile_Artillery : Projectile_Explosive
    {
        public PlanetTile targetTile = PlanetTile.Invalid;
        public IntVec3 targetCell;
        public float missRadius;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref targetTile, "targetTile", PlanetTile.Invalid);
            Scribe_Values.Look(ref targetCell, "targetCell");
            Scribe_Values.Look(ref missRadius, "missRadius");
        }

        public override int UpdateRateTicks => 1;

        public override void TickInterval(int delta)
        {
            if (targetTile.Valid && targetTile != this.Tile)
            {
                lifetime -= delta;
                ticksToImpact -= delta;
                if (!ExactPosition.InBounds(base.Map))
                {
                    SpawnWorldProjectile();
                }
                lifetime += delta;
                ticksToImpact += delta;
            }
            base.TickInterval(delta);
        }

        public virtual void SpawnWorldProjectile()
        {
            if (targetTile.Valid)
            {
                var worldProjectile = (WorldObject_ArtilleryProjectile)WorldObjectMaker.MakeWorldObject(VGEDefOf.VGE_ArtilleryProjectile);
                worldProjectile.Tile = this.Map.Tile;
                worldProjectile.SetFaction(this.Faction);
                worldProjectile.startTile = this.Map.Tile;
                worldProjectile.targetTile = targetTile;
                worldProjectile.targetCell = targetCell;
                worldProjectile.missRadius = missRadius;
                worldProjectile.projectileDef = this.def;
                worldProjectile.launcher = this.launcher;
                Find.WorldObjects.Add(worldProjectile);
            }
        }

        public override void Launch(Thing launcher, Vector3 origin, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, bool preventFriendlyFire = false, Thing equipment = null, ThingDef targetCoverDef = null)
        {
            var comp = launcher.TryGetComp<CompWorldArtillery>();
            var turret = launcher as Building_GravshipTurret;
            if (comp.worldTarget.IsValid && comp.worldTarget.Tile != this.Tile)
            {
                var edgeCell = comp.FindEdgeCell(launcher.Map, comp.worldTarget);
                this.targetTile = comp.worldTarget.Tile;
                this.targetCell = comp.targetCell;
                var shooter = turret.ManningPawn;
                float newMiss = comp.FinalForcedMissRadius(comp.worldTarget, shooter);
                this.missRadius = newMiss;
                intendedTarget = edgeCell;
                base.Launch(launcher, origin, edgeCell, intendedTarget, hitFlags, preventFriendlyFire, equipment, targetCoverDef);
            }
            else
            {
                base.Launch(launcher, origin, usedTarget, intendedTarget, hitFlags, preventFriendlyFire, equipment, targetCoverDef);
            }
            turret.TrySwitchBarrel();
        }

        public override void ImpactSomething()
        {
            if (targetTile.Valid is false)
            {
                base.ImpactSomething();
            }
        }
    }
}
