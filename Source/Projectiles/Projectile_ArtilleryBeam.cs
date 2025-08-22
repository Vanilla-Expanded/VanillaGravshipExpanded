using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace VanillaGravshipExpanded
{
    [HotSwappable]
    public class Projectile_ArtilleryBeam : Bullet
    {
        public PlanetTile targetTile = PlanetTile.Invalid;
        public IntVec3 targetCell;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref targetTile, "targetTile", PlanetTile.Invalid);
            Scribe_Values.Look(ref targetCell, "targetCell");
        }

        public override Vector3 ExactPosition => destination + Vector3.up * def.Altitude;
        public override void Launch(Thing launcher, Vector3 origin, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, bool preventFriendlyFire = false, Thing equipment = null, ThingDef targetCoverDef = null)
        {
            var comp = launcher.TryGetComp<CompWorldArtillery>();
            var originTarget = new TargetInfo(origin.ToIntVec3(), base.Map);
            if (comp.worldTarget.IsValid && comp.worldTarget.Tile != this.Tile)
            {
                var edgeCell = comp.FindEdgeCell(launcher.Map, comp.worldTarget);
                this.targetTile = comp.worldTarget.Tile;
                this.targetCell = comp.targetCell;
                intendedTarget = edgeCell;
                base.Launch(launcher, origin, edgeCell, intendedTarget, hitFlags, preventFriendlyFire, equipment, targetCoverDef);
                SpawnWorldProjectile();
                SpawnMote(originTarget, usedTarget);
                Destroy();
            }
            else
            {
                base.Launch(launcher, origin, usedTarget, intendedTarget, hitFlags, preventFriendlyFire, equipment, targetCoverDef);
                SpawnMote(originTarget, usedTarget);
                def.projectile.flyOverhead = false;
                base.Position = ExactPosition.ToIntVec3();
                ImpactSomething();
                def.projectile.flyOverhead = true;
            }
        }

        private void SpawnMote(TargetInfo origin, LocalTargetInfo usedTarget)
        {
            Vector3 offsetA = (ExactPosition - origin.CenterVector3).Yto0().normalized * def.projectile.beamStartOffset;
            if (launcher.Map != this.Map)
            {
                float angle = Find.WorldGrid.GetHeadingFromTo(launcher.Map.Tile, this.Map.Tile);
                offsetA -= new Vector3(2, 0, 2).RotatedBy(angle);
            }
            if (def.projectile.beamMoteDef != null)
            {
                MoteMaker.MakeInteractionOverlay(def.projectile.beamMoteDef, origin, usedTarget.ToTargetInfo(base.Map), offsetA, Vector3.zero);
            }
        }

        public void SpawnWorldProjectile()
        {
            Map targetMap = Find.Maps.Find(m => m.Tile == targetTile);
            var comp = launcher.TryGetComp<CompWorldArtillery>();
            var turret = launcher as Building_GravshipTurret;
            var shooter = turret?.ManningPawn;
            var hitChance = comp.GetHitChance(new GlobalTargetInfo(targetCell, targetMap), shooter);
            ArtilleryUtility.SpawnArtilleryProjectile(targetTile, Tile, def, launcher, targetCell, 0f, hitChance);
        }
    }
}
