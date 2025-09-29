using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded;

public class CompPowerPlantGravEngine : CompPowerPlant
{
    public override float DesiredPowerOutput
    {
        get
        {
            if (parent is not Building_GravEngine gravEngine)
                return 0f;

            var count = gravEngine.ValidSubstructure.Count;
            var max = gravEngine.GetStatValue(StatDefOf.SubstructureSupport, cacheStaleAfterTicks: GenTicks.TickLongInterval);

            return Mathf.Max(base.DesiredPowerOutput * (1 - (count / max)), 0f);
        }
    }
}