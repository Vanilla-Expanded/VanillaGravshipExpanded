using System.Collections.Generic;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

public class CompProperties_ApparelOxygenProvider : CompProperties
{
    public ThingDef fuelDef;
    public int fuelCountToRefill;
    public int fuelCountPerCharge;
    public int baseRefillTicks = 60;
    public int maxCharges;
    public float percentageToAutoRefill;
    public bool destroyOnEmpty;
    public bool replenishAfterCooldown;

    public float minResistanceToActivate = 0.12f;
    public float consumptionPerTick = 1f / GenTicks.TickLongInterval;

    public SoundDef soundRefill;

    [MustTranslate]
    public string chargeNoun = "charge";

    public NamedArgument ChargeNounArgument => chargeNoun.Named("CHARGENOUN");

    public CompProperties_ApparelOxygenProvider() => compClass = typeof(CompApparelOxygenProvider);

    public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
    {
        foreach (var error in base.ConfigErrors(parentDef))
            yield return error;

        if (fuelDef != null && fuelCountToRefill == 0 && fuelCountPerCharge == 0)
            yield return $"Oxygen provider has {nameof(fuelDef)} but one of {nameof(fuelCountToRefill)} or {nameof(fuelCountPerCharge)} must be set";
        if (fuelCountToRefill != 0 && fuelCountPerCharge != 0)
            yield return $"Oxygen provider: specify only one of {nameof(fuelCountToRefill)} and {nameof(fuelCountPerCharge)}";
        if (parentDef.tickerType != TickerType.Normal)
            yield return $"Oxygen provider requires the parent apparel's {nameof(ThingDef.tickerType)} to be {nameof(TickerType.Normal)}, but it currently is set to {parentDef.tickerType}";
    }

    public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
    {
        foreach (var stat in base.SpecialDisplayStats(req))
            yield return stat;

        if (!req.HasThing)
            yield return new StatDrawEntry(StatCategoryDefOf.Apparel, "Stat_Thing_ReloadMaxCharges_Name".Translate(ChargeNounArgument), maxCharges.ToString(), "Stat_Thing_ReloadMaxCharges_Desc".Translate(ChargeNounArgument), 2749);
        if (fuelDef != null)
        {
            if (fuelCountToRefill != 0)
                yield return new StatDrawEntry(StatCategoryDefOf.Apparel, "Stat_Thing_ReloadRefill_Name".Translate(ChargeNounArgument), $"{fuelCountToRefill} {fuelDef.label}", "Stat_Thing_ReloadRefill_Desc".Translate(ChargeNounArgument), 2749);
            else
                yield return new StatDrawEntry(StatCategoryDefOf.Apparel, "Stat_Thing_ReloadPerCharge_Name".Translate(ChargeNounArgument), $"{fuelCountPerCharge} {fuelDef.label}", "Stat_Thing_ReloadPerCharge_Desc".Translate(ChargeNounArgument), 2749);
        }
        if (destroyOnEmpty)
            yield return new StatDrawEntry(StatCategoryDefOf.Apparel, "Stat_Thing_ReloadDestroyOnEmpty_Name".Translate(ChargeNounArgument), "Yes".Translate(), "Stat_Thing_ReloadDestroyOnEmpty_Desc".Translate(ChargeNounArgument), 2749);
    }
}