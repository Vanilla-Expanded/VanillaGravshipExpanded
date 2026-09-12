using RimWorld;
using RimWorld.Planet;
using Verse;

namespace VanillaGravshipExpanded
{
    public class LandingOutcomeWorker_CriticalHeatSpike : LandingOutcomeWorker_GravshipBase
    {
        public LandingOutcomeWorker_CriticalHeatSpike(LandingOutcomeDef def)
            : base(def)
        {
        }

        public override bool CanTrigger(Gravship gravship)
        {
            return true;
        }

        public override void ApplyOutcome(Gravship gravship)
        {
            if (gravship.Engine.launchInfo.ExtendedInfo(false) is { lastCost: > 0 } extendedInfo)
            {
                var heatManager = gravship.Engine.GetComp<CompHeatManager>();
                heatManager.AddHeat(heatManager.HeatGeneratedFromFuel(extendedInfo.lastCost));
                SendStandardLetter(gravship.Engine, null, gravship.Engine);
            }
        }
    }
}
