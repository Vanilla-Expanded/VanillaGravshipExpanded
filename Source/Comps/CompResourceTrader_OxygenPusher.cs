using PipeSystem;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

public class CompResourceTrader_OxygenPusher : CompResourceTrader
{
    private bool lowPowerMode = false;
    private CompPowerTrader intPowerTrader;

    protected new CompProperties_ResourceTrader_OxygenPusher Props => (CompProperties_ResourceTrader_OxygenPusher)props;

    public CompPowerTrader PowerTrader => intPowerTrader ??= parent.GetComp<CompPowerTrader>();

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);

        if (parent.Map?.Biome.inVacuum != true)
            EnableLowPowerMode();
    }

    public override void CompTickRare()
    {
        base.CompTickRare();

        // Never work in a non-vacuum biome
        if (parent.Map?.Biome.inVacuum != true)
            return;

        // Make sure the power is on
        if (Props.requiresPower && PowerTrader.Off)
            return;

        // Don't do anything if no pipe net or turned off
        if (PipeNet == null || !ResourceOn)
        {
            EnableLowPowerMode();
            return;
        }
        // Don't do anything if there's not enough oxygen stored
        var available = PipeNet.TotalProductionThisTick + PipeNet.Stored - PipeNet.TotalConsumptionThisTick;
        if (available <= 0f)
        {
            EnableLowPowerMode();
            return;
        }

        // If disabled due to no more vacuum/exposed to vacuum, skip most other checks
        if (lowPowerMode)
        {
            var r = parent.GetRoom();
            if (r.ExposedToSpace || r.Vacuum <= 0)
                return;

            DisableLowPowerMode();
        }

        var room = parent.GetRoom();
        if (room.ExposedToSpace)
        {
            EnableLowPowerMode();
            return;
        }

        var vacuum = room.Vacuum;
        if (vacuum <= 0)
        {
            EnableLowPowerMode();
            return;
        }

        var maxConsumption = Props.consumptionPerTick / 100f * GenTicks.TickRareInterval;
        var consumption = maxConsumption;
        // Don't draw too much
        if (consumption > available)
            consumption = available;

        var change = 100f / room.CellCount * Props.airPerSecondPerHundredCells * CompOxygenPusher.IntervalToPerSecond * (consumption / maxConsumption);
        var vacuumAfter = room.UnsanitizedVacuum - change;
        // Scale the 
        if (vacuumAfter < 0)
            consumption *= (change + vacuumAfter) / change;

        room.Vacuum = vacuumAfter;

        PipeNet.ExtraConsumptionThisTick += consumption;
    }

    public override string CompInspectStringExtra()
    {
        if (!Props.requiresPower)
            return string.Empty;

        string text;
        if (PowerTrader.Off)
            text = "PowerConsumptionOff".Translate();
        else if (lowPowerMode)
            text = "PowerConsumptionLow".Translate();
        else
            text = "PowerConsumptionHigh".Translate();

        return $"{"PowerConsumptionMode".Translate()}: {text.CapitalizeFirst()}";
    }

    private void EnableLowPowerMode()
    {
        if (lowPowerMode)
            return;

        lowPowerMode = true;
        if (Props.requiresPower)
            PowerTrader.PowerOutput = PowerTrader.Props.PowerConsumption * Props.lowPowerConsumptionFactor;
    }

    private void DisableLowPowerMode()
    {
        if (!lowPowerMode)
            return;

        lowPowerMode = false;
        if (Props.requiresPower)
            PowerTrader.PowerOutput = PowerTrader.Props.PowerConsumption;
    }
}