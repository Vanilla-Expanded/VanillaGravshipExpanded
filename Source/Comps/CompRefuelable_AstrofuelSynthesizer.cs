using RimWorld;

namespace VanillaGravshipExpanded;

public class CompRefuelable_AstrofuelSynthesizer : CompRefuelable
{
    public CompResourceTrader_AstrofuelSynthesizer synthesizer;

    public override void CompTick()
    {
        // If resource is not, don't drain resources
        if (!synthesizer.ResourceOn || synthesizer.LowPowerModeOn || synthesizer.PipeNet.AvailableCapacityLastTick <= 0f)
            return;

        var prevRate = Props.fuelConsumptionRate;
        try
        {
            // If astropurifier is connected, divide consumption 4. Normally, the consumption to production ratio is 2-to-1.
            // We want to match production 1-to-1, and the output is already halved, so we basically need to halve it twice.
            if (synthesizer.astropurifier != null)
                Props.fuelConsumptionRate /= 4f;
            base.CompTick();
        }
        finally
        {
            // Cleanup once done
            Props.fuelConsumptionRate = prevRate;
        }
    }

    public override string CompInspectStringExtra()
    {
        var prevRate = Props.fuelConsumptionRate;
        var prevConsumeWhenUsed = Props.consumeFuelOnlyWhenUsed;

        try
        {
            // Update consumption like for ticking.
            if (synthesizer.astropurifier != null)
                Props.fuelConsumptionRate /= 4f;
            // If resource is off, disable refuelable drain display
            if (!synthesizer.ResourceOn || synthesizer.LowPowerModeOn || synthesizer.PipeNet.AvailableCapacityLastTick <= 0f)
                Props.consumeFuelOnlyWhenUsed = true;

            return base.CompInspectStringExtra();
        }
        finally
        {
            // Cleanup once done
            Props.fuelConsumptionRate = prevRate;
            Props.consumeFuelOnlyWhenUsed = prevConsumeWhenUsed;
        }
    }
}