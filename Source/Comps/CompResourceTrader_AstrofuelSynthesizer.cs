using PipeSystem;

namespace VanillaGravshipExpanded;

public class CompResourceTrader_AstrofuelSynthesizer : CompResourceTrader
{
    public CompFacility_Astropurifier astropurifier;

    public void Notify_LinkAdded(CompFacility_Astropurifier facility)
    {
        if (astropurifier == facility)
            return;

        astropurifier = facility;
        BaseConsumption = Props.consumptionPerTick / 2f;
    }

    public void Notify_LinkRemoved(CompFacility_Astropurifier facility)
    {
        if (astropurifier != facility)
            return;

        astropurifier = null;
        BaseConsumption = Props.consumptionPerTick;
    }
}