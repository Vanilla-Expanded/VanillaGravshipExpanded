using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded
{
    public class Projectile_JavelinRocket : Projectile_Artillery
    {
        private int smokeTicksLeft = 0;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref smokeTicksLeft, "smokeTicksLeft");
        }

        public override void Launch(Thing launcher, Vector3 origin, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, bool preventFriendlyFire = false, Thing equipment = null, ThingDef targetCoverDef = null)
        {
            smokeTicksLeft = 180;
            base.Launch(launcher, origin, usedTarget, intendedTarget, hitFlags, preventFriendlyFire, equipment, targetCoverDef);
            if (launcher is Building_JavelinLauncher javelinLauncher)
            {
                javelinLauncher.SwitchBarrel();
            }
        }

        public override void Tick()
        {
            base.Tick();
            if (smokeTicksLeft > 0 && this.Map != null)
            {
                smokeTicksLeft--;
                if (Find.TickManager.TicksGame % 4 == 0)
                {
                    FleckMaker.ThrowSmoke(this.ExactPosition, this.Map, 0.6f);
                }
            }
        }
    }
}
