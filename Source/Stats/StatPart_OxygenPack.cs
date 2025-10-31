using System.Collections.Generic;
using System.Text;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

public class StatPart_OxygenPack : StatPart
{
    private static bool SkipOxygenPacks = false;

    public static float UncachedVacuumResistanceIgnoringOxygenPacks(Pawn pawn)
    {
        try
        {
            SkipOxygenPacks = true;
            // Don't call GetValue(pawn), as that one is cached, and we don't want that since we temporarily disabled one part of the equation (oxygen packs).
            return StatDefOf.VacuumResistance.Worker.GetValue(StatRequest.For(pawn));
        }
        finally
        {
            SkipOxygenPacks = false;
        }
    }

    public override void TransformValue(StatRequest req, ref float val)
    {
        if (SkipOxygenPacks || req.Thing is not Pawn pawn || val >= 1f)
            return;

        if (GetFirstActiveRelevantApparel(pawn, val) != null)
            val = 1000000f; // The value is clamped to 0~1 range.
    }

    public override string ExplanationPart(StatRequest req)
    {
        if (SkipOxygenPacks || req.Thing is not Pawn pawn)
            return null;

        var builder = new StringBuilder();

        var resistance = StatDefOf.VacuumResistance.Worker.GetValueUnfinalized(req, false);

        foreach (var apparel in GetAllRelevantApparel(pawn))
        {
            if (apparel.RemainingCharges > 0 && apparel.Props.minResistanceToActivate <= resistance)
                builder.AppendLine($"{apparel.parent.LabelCap}: {"min".Translate().CapitalizeFirst()} 100%");
            else
                builder.AppendLine($"{apparel.parent.LabelCap}: {"VGE_OxygenPackInactive".Translate().CapitalizeFirst()}");
        }

        return builder.ToString();
    }

    private static CompApparelOxygenProvider GetFirstActiveRelevantApparel(Pawn pawn, float baseVacuumResistance)
    {
        foreach (var apparel in GetAllRelevantApparel(pawn))
        {
            if (apparel.RemainingCharges > 0 && apparel.Props.minResistanceToActivate <= baseVacuumResistance)
                return apparel;
        }

        return null;
    }

    private static IEnumerable<CompApparelOxygenProvider> GetAllRelevantApparel(Pawn pawn)
    {
        if (pawn?.apparel?.WornApparel == null)
            yield break;

        foreach (var apparel in pawn.apparel.WornApparel)
        {
            var comp = apparel.GetComp<CompApparelOxygenProvider>();
            if (comp != null)
                yield return comp;
        }
    }
}