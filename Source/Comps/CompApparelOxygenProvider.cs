using System.Collections.Generic;
using RimWorld;
using RimWorld.Utility;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded;

public class CompApparelOxygenProvider : CompApparelReloadable, IReloadableComp
{
    private static int LastCheckTick = -1;
    private static Pawn LastCheckPawn = null;

    private float useProgress;
    public float rechargeAtCharges;
    private bool automaticRechargeEnabled = true;
    [Unsaved] private Gizmo_OxygenProvider oxygenConfigurationGizmo;

    public new CompProperties_ApparelOxygenProvider Props => (CompProperties_ApparelOxygenProvider)props;

    public new string LabelRemaining => $"{RemainingChargesExact} / {MaxCharges}";

    public float RemainingChargesExact => Mathf.Max(RemainingCharges - useProgress, 0);

    public float ValuePercent => Mathf.Clamp01((RemainingCharges - useProgress) / Props.maxCharges);

    public bool AutomaticRechargeEnabled
    {
        get => automaticRechargeEnabled;
        set => automaticRechargeEnabled = value;
    }

    public override void PostPostMake()
    {
        base.PostPostMake();
        remainingCharges = MaxCharges;
        rechargeAtCharges = Mathf.Clamp(GenMath.RoundTo(MaxCharges * Props.percentageToAutoRefuel, MaxCharges / 20f), 0, MaxCharges);
    }

    public override void CompTickInterval(int delta)
    {
        base.CompTickInterval(delta);

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
        var baseVacuumResistance = StatDefOf.VacuumResistance.Worker.GetValueUnfinalized(StatRequest.For(pawn), false);
        if (baseVacuumResistance < Props.minResistanceToActivate)
        {
            // Special condition - not enough resistance for this pawn, allow other pieces of equipment to try and provide it
            LastCheckPawn = null;
            return;
        }

        // Don't run if pawn has 100% resistance, as it would just waste oxygen
        if (baseVacuumResistance >= 1)
            return;

        useProgress += Props.consumptionPerTick * 60;
        if (useProgress >= 1)
        {
            remainingCharges--;
            useProgress -= 1;
        }
    }

    public new bool NeedsReload(bool allowForceReload)
    {
        // Just in case
        if (AmmoDef == null)
            return false;
        // Make sure non-player pawns attempt to reload their apparel regardless of player configuration
        if (Wearer is { Faction.IsPlayer: false })
            return RemainingCharges <= Props.percentageToAutoRefuel * MaxCharges;
        // If automatic recharge is disabled, don't allow reloading unless forced
        if (!allowForceReload && !AutomaticRechargeEnabled)
            return false;
        return RemainingCharges != MaxCharges && (!allowForceReload || RemainingCharges <= rechargeAtCharges);
    }

    public new bool CanBeUsed(out string reason)
    {
        reason = null;
        return false;
    }

    public new string DisabledReason(int minNeeded, int maxNeeded) => null;

    public override void PostExposeData()
    {
        base.PostExposeData();

        Scribe_Values.Look(ref useProgress, nameof(useProgress));
        Scribe_Values.Look(ref rechargeAtCharges, nameof(rechargeAtCharges));
        Scribe_Values.Look(ref automaticRechargeEnabled, nameof(automaticRechargeEnabled), true);
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
            defaultLabel = "DEV: Set to empty",
            action = () => remainingCharges = 0,
        };
    }

    public void SetRechargeValuePct(float val) => rechargeAtCharges = val * MaxCharges;

    // All 3 methods are the same as parent method, but we call our own LabelRemaining/RemainingChargesExact which includes *fractions*.
    public override string CompInspectStringExtra() => "ChargesRemaining".Translate(Props.ChargeNounArgument) + ": " + LabelRemaining;

    public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
    {
        yield return new StatDrawEntry(StatCategoryDefOf.Apparel, "Stat_Thing_ReloadChargesRemaining_Name".Translate(Props.ChargeNounArgument), LabelRemaining, "Stat_Thing_ReloadChargesRemaining_Desc".Translate(Props.ChargeNounArgument), 2749);
    }

    public override string CompTipStringExtra() => $"\n\n{"Stat_Thing_ReloadChargesRemaining_Name".Translate(Props.ChargeNounArgument).CapitalizeFirst()}: {RemainingChargesExact} / {MaxCharges}";
}