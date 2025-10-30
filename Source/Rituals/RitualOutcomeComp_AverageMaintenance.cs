using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

public class RitualOutcomeComp_AverageMaintenance : RitualOutcomeComp_QualitySingleOffset
{

  

    public override QualityFactor GetQualityFactor(Precept_Ritual ritual, TargetInfo ritualTarget, RitualObligation obligation, RitualRoleAssignments assignments, RitualOutcomeComp_Data data)
    {
        float averageMaintenance=1;

        if (ritualTarget.Map != null)
        {
            averageMaintenance = ritualTarget.Map.GetComponent<MaintenanceAndDeterioration_MapComponent>()?.AverageMaintenanceInMap() ?? 1;
        }

        return new QualityFactor
        {
            label = LabelForDesc.CapitalizeFirst(),
            qualityChange = ExpectedOffsetDesc(positive: false, curve.Evaluate(averageMaintenance)),
            count = averageMaintenance.ToStringPercent(),
            positive = averageMaintenance>0.9f,
            priority = 4f,
            toolTip = LabelForDesc.CapitalizeFirst(),
           
        };
    }
}