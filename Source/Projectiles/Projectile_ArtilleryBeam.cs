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
        public LocalTargetInfo target;

        public Building_GravshipTurret GravshipTurret
        {
            get
            {
                if (launcher is Building_GravshipTurret gravshipTurret)
                {
                    return gravshipTurret;
                }
                return equipment as Building_GravshipTurret;
            }
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref targetTile, "targetTile", PlanetTile.Invalid);
            Scribe_TargetInfo.Look(ref target, "target");
        }

        public override Vector3 ExactPosition => destination + Vector3.up * def.Altitude;
        public override void Launch(Thing launcher, Vector3 origin, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, bool preventFriendlyFire = false, Thing equipment = null, ThingDef targetCoverDef = null)
        {
            this.launcher = launcher;
            this.equipment = equipment;
            var turret = GravshipTurret;
            var comp = turret.TryGetComp<CompWorldArtillery>();
            var originTarget = new TargetInfo(origin.ToIntVec3(), base.Map);
            if (comp.worldTarget.IsValid && comp.worldTarget.Tile != this.Tile)
            {
                var edgeCell = comp.FindEdgeCell(launcher.Map, comp.worldTarget);
                this.targetTile = comp.worldTarget.Tile;
                this.target = comp.target;
                intendedTarget = edgeCell;
                base.Launch(launcher, origin, edgeCell, intendedTarget, hitFlags, preventFriendlyFire, equipment, targetCoverDef);
                SpawnWorldProjectile();
                SpawnMote(originTarget, usedTarget);
                Destroy();
            }
            else
            {
                base.Launch(launcher, origin, usedTarget, intendedTarget, hitFlags, preventFriendlyFire, equipment, targetCoverDef);
                var position = ExactPosition.ToIntVec3();
                SpawnMote(originTarget, usedTarget);
                def.projectile.flyOverhead = false;
                if (position.InBounds(base.Map))
                {
                    base.Position = ExactPosition.ToIntVec3();
                }
                ImpactSomething();
                def.projectile.flyOverhead = true;
            }
        }

        public override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            var verbProps = GravshipTurret.AttackVerb.verbProps;
            if (hitThing != null)
            {
                if (hitThing.CanEverAttachFire())
                {
                    float chance = ((verbProps.flammabilityAttachFireChanceCurve == null) ? verbProps.beamChanceToAttachFire : verbProps.flammabilityAttachFireChanceCurve.Evaluate(hitThing.GetStatValue(StatDefOf.Flammability)));
                    if (Rand.Chance(chance))
                    {
                        hitThing.TryAttachFire(verbProps.beamFireSizeRange.RandomInRange, launcher);
                    }
                }
                else if (Rand.Chance(verbProps.beamChanceToStartFire))
                {
                    FireUtility.TryStartFireIn(Position, Map, verbProps.beamFireSizeRange.RandomInRange, launcher, verbProps.flammabilityAttachFireChanceCurve);
                }
            }
            else if (Rand.Chance(verbProps.beamChanceToStartFire))
            {
                FireUtility.TryStartFireIn(Position, Map, verbProps.beamFireSizeRange.RandomInRange, launcher, verbProps.flammabilityAttachFireChanceCurve);
            }
            base.Impact(hitThing, blockedByShield);
        }

        private void SpawnMote(TargetInfo origin, LocalTargetInfo usedTarget)
        {
            Vector3 offsetA = (ExactPosition - origin.CenterVector3).Yto0().normalized * def.projectile.beamStartOffset;
            if (launcher.Map != this.Map)
            {
                float angle = ArtilleryUtility.GetAngle(launcher.Map.Tile, this.Map.Tile);
                offsetA -= new Vector3(2, 0, 2).RotatedBy(angle);
            }
            if (def.projectile.beamMoteDef != null)
            {
                MoteMaker.MakeInteractionOverlay(def.projectile.beamMoteDef, origin, usedTarget.ToTargetInfo(base.Map), offsetA, Vector3.zero);
            }
        }

        public void SpawnWorldProjectile()
        {
            var turret = GravshipTurret;
            Map targetMap = Find.Maps.Find(m => m.Tile == targetTile);
            var comp = turret.TryGetComp<CompWorldArtillery>();
            var globalTarget = target.HasThing ? new GlobalTargetInfo(target.Thing) : new GlobalTargetInfo(target.Cell, targetMap);
            var hitChance = comp.GetHitChance(globalTarget);
            ArtilleryUtility.SpawnArtilleryProjectile(targetTile, Tile, def, launcher, globalTarget.Cell, 0f, hitChance);
        }
    }
}
