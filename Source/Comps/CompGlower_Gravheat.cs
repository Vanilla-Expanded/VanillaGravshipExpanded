using RimWorld;
using Verse;

namespace VanillaGravshipExpanded
{
    public class CompProperties_Glower_Gravheat : CompProperties_Glower
    {
        public CompProperties_Glower_Gravheat()
        {
            compClass = typeof(CompGlower_Gravheat);
        }
    }
    public class CompGlower_Gravheat : CompGlower
    {
        public override bool ShouldBeLitNow
        {
            get
            {
                var heatsinkComp = parent.GetComp<CompHeatsink>();
                if (heatsinkComp != null)
                {
                    return heatsinkComp.StoredHeat > 0;
                }

                var absorberComp = parent.GetComp<CompGravheatAbsorber>();
                if (absorberComp != null)
                {
                    return absorberComp.IsOnCooldown;
                }

                return false;
            }
        }
    }

}

