
using RimWorld;
using Verse;
namespace VanillaGravshipExpanded
{
    public class DamageWorker_ExtinguishAstrofire : DamageWorker
    {
        private const float DamageAmountToFireSizeRatio = 0.01f;

        public override DamageResult Apply(DamageInfo dinfo, Thing victim)
        {
            DamageResult result = new DamageResult();
            Astrofire fire = victim as Astrofire;
            if (fire == null || fire.Destroyed)
            {
                Thing thing = victim?.GetAttachment(VGEDefOf.VGE_Astrofire);
                if (thing != null)
                {
                    fire = (Astrofire)thing;
                }
            }
            if (fire != null && !fire.Destroyed)
            {
                base.Apply(dinfo, victim);
                fire.fireSize -= dinfo.Amount * 0.01f;
                if (fire.fireSize < 0.1f)
                {
                   
                    fire.Destroy();
                }
            }
            Pawn pawn = victim as Pawn;
            if (pawn != null)
            {
                Hediff hediff = HediffMaker.MakeHediff(dinfo.Def.hediff, pawn);
                hediff.Severity = dinfo.Amount;
                pawn.health.AddHediff(hediff, null, dinfo);
            }
            return result;
        }
    }
}