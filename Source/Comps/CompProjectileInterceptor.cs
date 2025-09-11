using RimWorld;
using System;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace VanillaGravshipExpanded
{
    public class CompProperties_ProjectileInterceptor : CompProperties
    {
        public float interceptionRadius;

        public CompProperties_ProjectileInterceptor()
        {
            compClass = typeof(CompProjectileInterceptor);
        }
    }
    
    [HotSwappable]
    public class CompProjectileInterceptor : ThingComp
    {
        private CompRefuelable refuelableComp;
        private int ticksUntilNextShot;
        public CompProperties_ProjectileInterceptor Props => (CompProperties_ProjectileInterceptor)props;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            refuelableComp = parent.GetComp<CompRefuelable>();
        }

        public override void CompTickInterval(int delta)
        {
            var turret = parent as Building_TurretGun;
            if (!turret.Active)
            {
                return;
            }

            if (ticksUntilNextShot > 0)
            {
                ticksUntilNextShot -= delta;
            }

            var target = FindTarget();
            if (target != null)
            {
                if (ticksUntilNextShot <= 0)
                {
                    if (refuelableComp.HasFuel)
                    {
                        VGEDefOf.Gun_MiniTurret.verbs[0].soundCast.PlayOneShot(new TargetInfo(parent.Position, parent.Map));
                        refuelableComp.ConsumeFuel(1);
                        FleckMaker.Static(parent.Position, parent.Map, FleckDefOf.ShotFlash, 9);
                        TryIntercept(target);
                        turret.Top.CurRotation = (target.DrawPos - parent.DrawPos).AngleFlat();
                    }
                    ticksUntilNextShot = 20;
                }
            }
        }

        private Thing FindTarget()
        {
            var allThings = parent.Map.listerThings.AllThings.Where(t =>
                !t.Destroyed && t.DrawPos.ToIntVec3().DistanceTo(parent.Position) <= Props.interceptionRadius);

            var potentialTargets = allThings.Where(t => IsValidTarget(t)).ToList();

            return potentialTargets.OrderBy(t => t.DrawPos.ToIntVec3().DistanceTo(parent.Position)).FirstOrDefault();
        }

        private bool IsValidTarget(Thing t)
        {
            if (t is Projectile_Space || t is Projectile projectile && projectile.def.projectile.explosionRadius > 0 && projectile.launcher.HostileTo(parent.Faction))
            {
                return true;
            }

            if (t is DropPodIncoming dropPod)
            {
                var allPawns = dropPod.innerContainer.Where(thing => thing is Pawn).Cast<Pawn>().ToList();
                var transporters = dropPod.innerContainer.Where(thing => thing is ActiveTransporter).Cast<ActiveTransporter>().ToList();
                foreach (var transporter in transporters)
                {
                    var transporterPawns = transporter.Contents.innerContainer.Where(thing => thing is Pawn).Cast<Pawn>().ToList();
                    allPawns.AddRange(transporterPawns);
                }
                var hostilePawns = allPawns.Where(pawn => pawn.HostileTo(parent.Faction)).ToList();
                if (hostilePawns.Any())
                {
                    return true;
                }
            }

            return false;
        }

        private void TryIntercept(Thing target)
        {
            if (InterceptChance(target))
            {
                Effecter eff = new Effecter(EffecterDefOf.Interceptor_BlockedProjectile);
                eff.Trigger(parent, new TargetInfo(target.Position, parent.Map));
                eff.Cleanup();
                FleckMaker.ThrowSmoke(target.Position.ToVector3(), parent.Map, 1.2f);
                if (target is DropPodIncoming)
                {
                    GenSpawn.Spawn(ThingDefOf.ChunkSlagSteel, target.Position, parent.Map);
                }
                target.Destroy(DestroyMode.Vanish);
            }
        }

        private bool InterceptChance(Thing target)
        {
            bool success = false;
            if (target is Projectile projectile)
            {
                float velocity = projectile.def.projectile.speed;
                float chance = 0.98f * (float)Math.Exp(-Math.Max(0, velocity - 30) / 60f);
                chance = Mathf.Clamp(chance, 0.05f, 0.98f);
                if (Rand.Chance(chance))
                {
                    success = true;
                }
            }
            else if (target is DropPodIncoming)
            {
                float chance = 0.25f;
                if (Rand.Chance(chance))
                {
                    success = true;
                }
            }
            return success;
        }

        public override void PostDrawExtraSelectionOverlays()
        {
            base.PostDrawExtraSelectionOverlays();
            GenDraw.DrawRadiusRing(parent.Position, Props.interceptionRadius);
        }
    }
}
