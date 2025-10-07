using Verse;
using RimWorld;

namespace VanillaGravshipExpanded;

public class RitualPosition_GravtechConsole : RitualPosition_GravshipLaunchBase
{
    protected override CompGravshipFacility GetRelevantComp(Thing thing)
    {
        var comp = thing.TryGetComp<CompGravtechConsole>();
        if (comp is { CanBeActive: true })
            return comp;
        return null;
    }
}