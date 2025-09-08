
using RimWorld;
using Verse;
namespace VanillaGravshipExpanded
{
    public class AstrofireSpark : Projectile
    {
        public override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            Map map = base.Map;
            base.Impact(hitThing, blockedByShield);
            Thing instigator = launcher;
            Astrofire fire;
            if ((fire = launcher as Astrofire) != null)
            {
                instigator = fire.instigator;
            }
            AstrofireUtility.TryStartAstrofireIn(base.Position, map, 0.1f, instigator);
        }
    }
}