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
            yield return "Reloadable component has ammoDef but one of ammoCountToRefill or ammoCountPerCharge must be set";
        if (fuelCountToRefill != 0 && fuelCountPerCharge != 0)
            yield return "Reloadable component: specify only one of ammoCountToRefill and ammoCountPerCharge";
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