using RimWorld;

namespace VanillaGravshipExpanded;

public class CompRefuelable_IgnoreFlickableComp : CompRefuelable
{
    public override void CompTick()
    {
        // Temporarily set the flickComp to null so the fuel is consumed even if flicked off.
        // We could technically permanently set it to null in PostSpawnSetup, but pawns we
        // need it for ShouldAutoRefuelNowIgnoringFuelPct getter, which would allow pawns
        // to auto-refuel the refuelable despite it being wasteful - we only want to
        // auto-refuel when flicked on so the resources are processed.
        var comp = flickComp;
        try
        {
            flickComp = null;
            base.CompTick();
        }
        finally
        {
            flickComp = comp;
        }
    }
}