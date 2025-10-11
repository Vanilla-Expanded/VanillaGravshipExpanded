using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded;

public class StatPart_LinkedGlobalMaintenanceSensitivity : StatPart
{
    public override void TransformValue(StatRequest req, ref float val)
    {
        var engine = GetEngine(ref req);
        if (engine != null)
            val += engine.GetStatValue(VGEDefOf.VGE_GlobalMaintenanceSensitivity);
    }

    public override string ExplanationPart(StatRequest req)
    {
        var engine = GetEngine(ref req);
        if (engine != null)
        {
            var val = engine.GetStatValue(VGEDefOf.VGE_GlobalMaintenanceSensitivity);
            if (!Mathf.Approximately(val, 0))
                return $"{"VGE_StatsReport_GravEngineGlobalMaintenanceSensitivity".Translate()}: {VGEDefOf.VGE_GlobalMaintenanceSensitivity.ValueToString(val)}";
        }

        return null;
    }

    private static Building_GravEngine GetEngine(ref readonly StatRequest req)
    {
        if (!req.HasThing)
            return null;
        if (req.Thing is Building_GravEngine engine)
            return engine;
        if (req.Thing.TryGetComp<CompGravshipFacility>(out var comp) && comp.engine != null)
            return comp.engine;
        return null;
    }
}