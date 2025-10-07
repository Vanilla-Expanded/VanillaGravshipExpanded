using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

public class RitualPosition_EngineeringConsole : RitualPosition_GravshipLaunchBase
{
    protected override CompGravshipFacility GetRelevantComp(Thing thing)
    {
        var comp = thing.TryGetComp<CompEngineeringConsole>();
        if (comp is { CanBeActive: true })
            return comp;
        return null;
    }
}