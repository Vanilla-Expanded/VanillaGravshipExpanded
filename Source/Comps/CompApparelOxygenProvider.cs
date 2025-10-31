using System.Collections.Generic;
using RimWorld;
using RimWorld.Utility;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace VanillaGravshipExpanded;

public class CompApparelOxygenProvider : ThingComp, IReloadableComp
{
    private static int LastCheckTick = -1;
    private static Pawn LastCheckPawn = null;

    private float remainingCharges;
    public float rechargeAtCharges;
    private bool automaticRechargeEnabled = true;
    private int replenishInTicks = -1;
    [Unsaved] private Gizmo_OxygenProvider oxygenConfigurationGizmo;

    public CompProperties_ApparelOxygenProvider Props => (CompProperties_ApparelOxygenProvider)props;

    public int RemainingCharges => Mathf.CeilToInt(remainingCharges);

    public int MaxCharges => Props.maxCharges;

    public string LabelRemaining => $"{RemainingChargesExactString} / {MaxCharges}";

    public float RemainingChargesExact => remainingCharges;

    public string RemainingChargesExactString => $"{RemainingChargesExact:0.00}";

    public float ValuePercent => Mathf.Clamp01(remainingCharges / Props.maxCharges);

    public ThingDef AmmoDef => Props.fuelDef;

    public Thing ReloadableThing => parent;

    public int BaseReloadTicks => Props.baseRefillTicks;

    public bool AutomaticRechargeEnabled
    {
        get => automaticRechargeEnabled;
        set => automaticRechargeEnabled = value;
    }

    public Pawn Wearer => (ParentHolder as Pawn_ApparelTracker)?.pawn;

    public override void PostPostMake()
    {
        base.PostPostMake();
        remainingCharges = MaxCharges;
        rechargeAtCharges = Mathf.Clamp(GenMath.RoundTo(MaxCharges * Props.percentageToAutoRefill, MaxCharges / 20f), 0, MaxCharges);
    }

    public override void CompTickInterval(int delta)
    {
        base.CompTickInterval(delta);

        if (Props.replenishAfterCooldown && RemainingCharges == 0)
        {
            replenishInTicks -= delta;
            if (replenishInTicks <= 0)
                remainingCharges = MaxCharges;
        }

        // Don't run this code too often, and don't run it if we don't have enough charges.
        // Also, specifically use the wearer for ticking so each worn oxygen provider ticks at the
        // same time, as we rely on it to prevent multiple pieces from running at the same time.
        var pawn = Wearer;
        if (pawn == null || !pawn.IsHashIntervalTick(60, delta) || RemainingCharges <= 0)
            return;

        // Clear the last processed pawn this tick if the current tick changed
        if (LastCheckTick != Find.TickManager.TicksGame)
            LastCheckPawn = null;
        // Return early if a different piece of equipment already provided oxygen, or determined that it can't
        else if (pawn == LastCheckPawn)
            return;

        LastCheckTick = Find.TickManager.TicksGame;
        LastCheckPawn = pawn;

        // Return early if there's never vacuum in the biome
        if (pawn.MapHeld?.Biome?.inVacuum != true)
            return;

        // Return early if the wearer doesn't need to breathe
        if (pawn.RaceProps.IsMechanoid || (pawn.IsMutant && !pawn.mutant.Def.breathesAir))
            return;

        // Check if the pawn has a valid position
        var position = pawn.PositionHeld;
        if (!position.IsValid)
            return;

        // Don't do anything if less than 50% vacuum, as it's safe
        var vacuum = position.GetVacuum(parent.MapHeld);
        if (vacuum < 0.5f)
            return;

        // Don't run if the pawn doesn't have enough oxygen resistance as a base
        var baseVacuumResistance = StatPart_OxygenPack.UncachedVacuumResistanceIgnoringOxygenPacks(pawn);
        if (baseVacuumResistance < Props.minResistanceToActivate)
        {
            // Special condition - not enough resistance for this pawn, allow other pieces of equipment to try and provide it
            LastCheckPawn = null;
            return;
        }

        // Don't run if pawn has 100% resistance, as it would just waste oxygen
        if (baseVacuumResistance >= 1)
            return;

        remainingCharges -= Props.consumptionPerTick * 60;
    }

    public bool NeedsReload(bool allowForceReload)
    {
        // Just in case
        if (AmmoDef == null)
            return false;
        // Make sure non-player pawns attempt to reload their apparel regardless of player configuration
        if (Wearer is { Faction.IsPlayer: false })
            return RemainingCharges <= Props.percentageToAutoRefill * MaxCharges;
        // If automatic recharge is disabled, don't allow reloading unless forced
        if (!allowForceReload && !AutomaticRechargeEnabled)
            return false;

        return RemainingCharges != MaxCharges && (allowForceReload || RemainingCharges <= rechargeAtCharges);
    }

    public void ReloadFrom(Thing ammo)
    {
        if (!NeedsReload(true))
            return;

        if (Props.fuelCountToRefill != 0)
        {
            if (ammo.stackCount < Props.fuelCountToRefill)
            {
                return;
            }
            ammo.SplitOff(Props.fuelCountToRefill).Destroy();
            remainingCharges = MaxCharges;
        }
        else
        {
            if (ammo.stackCount < Props.fuelCountPerCharge)
                return;

            var num = Mathf.Clamp(ammo.stackCount / Props.fuelCountPerCharge, 0, MaxCharges - RemainingCharges);
            ammo.SplitOff(num * Props.fuelCountPerCharge).Destroy();
            remainingCharges += num;
        }

        Props.soundRefill?.PlayOneShot(new TargetInfo(Wearer.Position, Wearer.Map));
    }

    public override void PostExposeData()
    {
        base.PostExposeData();

        Scribe_Values.Look(ref remainingCharges, nameof(remainingCharges), -999);
        Scribe_Values.Look(ref rechargeAtCharges, nameof(rechargeAtCharges));
        Scribe_Values.Look(ref automaticRechargeEnabled, nameof(automaticRechargeEnabled), true);
        Scribe_Values.Look(ref replenishInTicks, nameof(replenishInTicks), -1);

        if (Scribe.mode == LoadSaveMode.PostLoadInit && Mathf.Approximately(remainingCharges, -999))
            remainingCharges = remainingCharges = MaxCharges;
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        foreach (var gizmo in base.CompGetGizmosExtra())
            yield return gizmo;

        // Only show gizmo if wearer/parent is the only selected thing
        if (Find.Selector.SingleSelectedObject == parent)
            yield return oxygenConfigurationGizmo ??= new Gizmo_OxygenProvider(this);
    }

    public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
    {
        foreach (var gizmo in base.CompGetWornGizmosExtra())
            yield return gizmo;

        // Only show gizmo if wearer/parent is the only selected thing
        if (Find.Selector.SingleSelectedObject == Wearer)
            yield return oxygenConfigurationGizmo ??= new Gizmo_OxygenProvider(this);

        if (!DebugSettings.ShowDevGizmos)
            yield break;

        yield return new Command_Action
        {
            defaultLabel = "DEV: Set oxygen to empty",
            action = () => remainingCharges = 0,
        };
        yield return new Command_Action
        {
            defaultLabel = "DEV: Oxygen -20%",
            action = () => remainingCharges = Mathf.Max(0, remainingCharges - Props.maxCharges * 0.2f)
        };
        yield return new Command_Action
        {
            defaultLabel = "DEV: Set oxygen to full",
            action = () => remainingCharges = MaxCharges,
        };
    }

    public void SetRechargeValuePct(float val) => rechargeAtCharges = val * MaxCharges;

    public string DisabledReason(int minNeeded, int maxNeeded) => null;

    public int MinAmmoNeeded(bool allowForcedReload)
    {
        if (!NeedsReload(allowForcedReload))
            return 0;
        if (Props.fuelCountToRefill != 0)
            return Props.fuelCountToRefill;
        return Props.fuelCountPerCharge;
    }

    public int MaxAmmoNeeded(bool allowForcedReload)
    {
        if (!NeedsReload(allowForcedReload))
            return 0;
        if (Props.fuelCountToRefill != 0)
            return Props.fuelCountToRefill;
        return Props.fuelCountPerCharge * (MaxCharges - RemainingCharges);
    }

    public int MaxAmmoAmount()
    {
        if (AmmoDef == null)
            return 0;
        if (Props.fuelCountToRefill == 0)
            return Props.fuelCountPerCharge * MaxCharges;
        return Props.fuelCountToRefill;
    }

    public bool CanBeUsed(out string reason)
    {
        reason = null;
        return true;
    }

    public override string CompInspectStringExtra() => "ChargesRemaining".Translate(Props.ChargeNounArgument) + ": " + LabelRemaining;

    public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
    {
        yield return new StatDrawEntry(StatCategoryDefOf.Apparel, "Stat_Thing_ReloadChargesRemaining_Name".Translate(Props.ChargeNounArgument), LabelRemaining, "Stat_Thing_ReloadChargesRemaining_Desc".Translate(Props.ChargeNounArgument), 2749);
    }

    public override string CompTipStringExtra() => $"\n\n{"Stat_Thing_ReloadChargesRemaining_Name".Translate(Props.ChargeNounArgument).CapitalizeFirst()}: {RemainingChargesExactString} / {MaxCharges}";
}