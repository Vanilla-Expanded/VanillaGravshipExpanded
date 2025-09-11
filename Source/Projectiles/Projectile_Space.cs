using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded
{
    [HotSwappable]
    public class Projectile_Space : Projectile_Explosive
    {
        protected bool noLoot;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref noLoot, "noLoot", false);
        }

        public override void TickInterval(int delta)
        {
            lifetime -= delta;
            ticksToImpact -= delta;
            if (!ExactPosition.InBounds(base.Map))
            {
                noLoot = true;
            }
            lifetime += delta;
            ticksToImpact += delta;
            base.TickInterval(delta);
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (noLoot is false && this.Map != null)
            {
                TryDropLoot();
            }
            base.Destroy(mode);
        }

        protected virtual void TryDropLoot()
        {

        }
    }
}
