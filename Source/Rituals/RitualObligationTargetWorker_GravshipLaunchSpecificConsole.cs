using System.Collections.Generic;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

// Inherit from vanilla GravshipLaunch in case some mods care about that. It also handles CanUseTargetInternal method.
// As opposed to the vanilla class, we require this obligation's Def to specify buildings in thingDefs list.
public class RitualObligationTargetWorker_GravshipLaunchSpecificConsole : RitualObligationTargetWorker_GravshipLaunch
{
    public RitualObligationTargetWorker_GravshipLaunchSpecificConsole()
    {
    }

    public RitualObligationTargetWorker_GravshipLaunchSpecificConsole(RitualObligationTargetFilterDef def) : base(def)
    {
    }

    public override IEnumerable<TargetInfo> GetTargets(RitualObligation obligation, Map map)
    {
        foreach (var targetDef in def.thingDefs)
        {
            foreach (var thing in map.listerThings.ThingsOfDef(targetDef))
            {
                if (thing.TryGetComp(out CompPilotConsole compPilotConsole) && compPilotConsole.CanUseNow())
                    yield return thing;
            }
        }
    }

    public override RitualTargetUseReport CanUseTargetInternal(TargetInfo target, RitualObligation obligation)
    {
        if (!target.HasThing || !def.thingDefs.Contains(target.Thing.def))
            return false;

        if (CompMultipleGravEnginesHandler.MultipleGravEnginesPresent)
            return "VGE_MultipleGravEnginesPresent".Translate().CapitalizeFirst();
        if (Find.CurrentGravship != null)
            return "VGE_GravshipCurrentlyActive".Translate().CapitalizeFirst();
        if (Find.Maps.Any(x => x.lordManager.lords.Any(lord => lord.LordJob is LordJob_Ritual { ritual: Precept_GravshipLaunch })))
            return "VGE_GravshipLaunchCurrentlyActive".Translate().CapitalizeFirst();

        return base.CanUseTargetInternal(target, obligation);
    }

    public override IEnumerable<string> GetTargetInfos(RitualObligation obligation)
    {
        foreach (var thing in def.thingDefs)
            yield return "VGE_ValidThing".Translate(thing.Named("THING"));
    }
}