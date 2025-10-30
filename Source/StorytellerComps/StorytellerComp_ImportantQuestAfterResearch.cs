
using RimWorld;
using System.Collections.Generic;
using Verse;
namespace VanillaGravshipExpanded
{
    public class StorytellerComp_ImportantQuestAfterResearch : StorytellerComp
    {
        private static int IntervalsPassed=> Find.TickManager.TicksGame / 1000;
        public const int countDownSinceElectricity = 180000; //3 days

        private StorytellerCompProperties_ImportantQuestAfterResearch Props => (StorytellerCompProperties_ImportantQuestAfterResearch)props;

        private bool BeenGivenQuest => Find.QuestManager.QuestsListForReading.Any((Quest q) => q.root == Props.questDef);
 
        public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
        {
            if (!VGEDefOf.Electricity.IsFinished) {
                yield break;
            }
            else
            {
                World_ExposeData_Patch.countDownSinceElectricityTickCounter +=1000;
            }

            if (World_ExposeData_Patch.countDownSinceElectricityTickCounter > countDownSinceElectricity&&IntervalsPassed > Props.fireAfterDaysPassed * 60 && !BeenGivenQuest)
            {
                IncidentDef questIncident = Props.questIncident;
                if (questIncident.TargetAllowed(target))
                {
                    yield return new FiringIncident(questIncident, this, GenerateParms(questIncident.category, target));
                }
            }
        }
    }
}
