using RimWorld;
using RimWorld.Planet;
using Verse;

namespace VanillaGravshipExpanded
{
    public class LandingOutcomeWorker_GravdataCorruption : LandingOutcomeWorker_GravshipBase
    {
        public LandingOutcomeWorker_GravdataCorruption(LandingOutcomeDef def)
            : base(def)
        {
        }

        public override bool CanTrigger(Gravship gravship)
        {
            return gravship.engine.launchInfo.quality > 0 && LaunchInfo_ExposeData_Patch.gravtechResearcherPawns.TryGetValue(gravship.engine.launchInfo, out var researcherPawn) && researcherPawn != null;
        }

        public override void ApplyOutcome(Gravship gravship)
        {
            WorldComponent_GravshipController_LandingEnded_Patch.gravdataCorruptionOccurred[gravship.Engine] = true;
            SendStandardLetter(gravship.Engine, null, gravship.Engine);
        }
    }
}
