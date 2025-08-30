
using RimWorld;
using System.Collections.Generic;
using Verse;
namespace VanillaGravshipExpanded
{
    public class DamageWorker_Astrofire : DamageWorker_AddInjury
    {
        public override DamageResult Apply(DamageInfo dinfo, Thing victim)
        {
            Pawn pawn = victim as Pawn;
            if (pawn != null && pawn.Faction == Faction.OfPlayer)
            {
                Find.TickManager.slower.SignalForceNormalSpeedShort();
            }
            Map map = victim.Map;
            DamageResult damageResult = base.Apply(dinfo, victim);
            if (map == null)
            {
                return damageResult;
            }
            if (!damageResult.deflected && !dinfo.InstantPermanentInjury && Rand.Chance(AstrofireUtility.ChanceToAttachAstrofireFromEvent(victim)))
            {
                victim.TryAttachAstrofire(Rand.Range(0.15f, 0.25f), dinfo.Instigator);
            }
            if (victim.Destroyed && pawn == null)
            {
                foreach (IntVec3 item in victim.OccupiedRect())
                {
                    FilthMaker.TryMakeFilth(item, map, ThingDefOf.Filth_Ash);
                }
                return damageResult;
            }
            return damageResult;
        }

        public override void ExplosionAffectCell(Explosion explosion, IntVec3 c, List<Thing> damagedThings, List<Thing> ignoredThings, bool canThrowMotes)
        {
            base.ExplosionAffectCell(explosion, c, damagedThings, ignoredThings, canThrowMotes);
            if (def == VGEDefOf.VGE_AstrofireDamage && Rand.Chance(AstrofireUtility.ChanceToStartAstrofireIn(c, explosion.Map)))
            {
                AstrofireUtility.TryStartAstrofireIn(c, explosion.Map, Rand.Range(0.2f, 0.6f), explosion.instigator);
            }
        }
    }
}
