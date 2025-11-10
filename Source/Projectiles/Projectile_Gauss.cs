namespace VanillaGravshipExpanded
{
    using RimWorld;
    using UnityEngine;
    using Verse;

    internal class Projectile_Gauss : Projectile_Artillery
    {
        private int               breakTicksLeft;

        public override void Launch(Thing launcher, Vector3 origin, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, bool preventFriendlyFire = false, Thing equipment = null, ThingDef targetCoverDef = null)
        {
            base.Launch(launcher, origin, usedTarget, intendedTarget, hitFlags, preventFriendlyFire, equipment, targetCoverDef);

            this.breakTicksLeft = Rand.Range(10, 30);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref this.breakTicksLeft, nameof(this.breakTicksLeft));
        }

        public override void Tick()
        {
            base.Tick();
        }

        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);

            if (!Find.TickManager.Paused)
            {
                Vector3 velocityDirection = this.ExactRotation * Vector3.forward;

                Vector3 effectPos = drawLoc + velocityDirection;
                {
                    this.Map.flecks.CreateFleck(new FleckCreationData()
                                                {
                                                    def              = FleckDefOf.LightningGlow,
                                                    spawnPosition    = drawLoc,
                                                    scale            = Rand.Range(0.1f, 0.2f) * 3,
                                                    ageTicksOverride = -1,
                                                    rotationRate     = 0,
                                                    velocityAngle    = this.ExactPosition.AngleToFlat(effectPos) + 90,
                                                    velocitySpeed    = 0.1f * this.def.projectile.speed
                                                });
                }

                if (this.IsHashIntervalTick(5))
                {
                    FleckCreationData data = FleckMaker.GetDataStatic(drawLoc + velocityDirection, this.Map, VGEDefOf.VGE_GaussDistortion, Rand.Range(0.1f, 0.25f) * 2);
                    data.rotationRate  = 0;
                    data.velocityAngle = this.ExactPosition.AngleToFlat(effectPos) - 90 + Rand.Range(-15, 15);
                    data.velocitySpeed = this.def.projectile.speed;
                    this.Map.flecks.CreateFleck(data);
                }

                if (this.breakTicksLeft-- == 0)
                {
                    FleckCreationData data = FleckMaker.GetDataStatic(drawLoc + velocityDirection * 2, this.Map, VGEDefOf.Fleck_HeatWaveDistortion, Rand.Range(0.001f, 0.002f));
                    data.rotationRate  = 0;
                    data.velocityAngle = this.ExactPosition.AngleToFlat(effectPos) + 90 + Rand.Range(-15, 15);
                    data.velocitySpeed = this.def.projectile.speed / 20;
                    this.Map.flecks.CreateFleck(data);
                }
            }
        }

        public override void ImpactSomething()
        {
            FleckCreationData data = FleckMaker.GetDataStatic(this.ExactPosition, this.Map, VGEDefOf.VGE_GaussImpact, Rand.Range(1, 2f) * 2);
            this.Map.flecks.CreateFleck(data);
            base.ImpactSomething();
        }
    }
}
