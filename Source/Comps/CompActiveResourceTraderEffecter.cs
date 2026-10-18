using PipeSystem;
using Verse;

namespace VanillaGravshipExpanded;

[StaticConstructorOnStartup]
public class CompActiveResourceTraderEffecter : ThingComp
{
    public CompProperties_ActiveResourceTraderEffecter Props => (CompProperties_ActiveResourceTraderEffecter)props;

    protected Effecter effecter;
    protected CompResourceTrader trader;

    public override void CompTickInterval(int delta)
    {
        base.CompTickInterval(delta);

        HandleEffecter(parent.MaxTickIntervalRate);
    }

    public override void CompTickRare()
    {
        base.CompTickRare();

        HandleEffecter(GenTicks.TickRareInterval);
    }

    public override void CompTickLong()
    {
        base.CompTickLong();

        HandleEffecter(GenTicks.TickLongInterval);
    }

    protected void HandleEffecter(int ticksMaintained)
    {
        if (ticksMaintained > 0 && parent.Spawned && !trader.LowPowerModeOn && trader.CanBeOn())
        {
            // If null or ended, create maintainer
            if (effecter is not { ticksLeft: > 0 })
            {
                effecter = Props.effecterDef.SpawnMaintained(parent, parent.Map);
                foreach (var subEffecter in effecter.children)
                {
                    if (subEffecter is SubEffecter_SprayerContinuous_MoveTowardsSource se)
                        se.targetOffset = Props.offset;
                }
            }
            effecter.ticksLeft = ticksMaintained + 5;
        }
        else if (effecter != null)
        {
            effecter.ForceEnd();
            effecter = null;
        }
    }

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);

        trader = parent.GetComp<CompResourceTrader>();

        HandleEffecter(parent.def.tickerType switch
        {
            TickerType.Rare => GenTicks.TickRareInterval,
            TickerType.Long => GenTicks.TickLongInterval,
            _ => 15,
        });
    }

    public override void PostDestroy(DestroyMode mode, Map previousMap)
    {
        base.PostDestroy(mode, previousMap);
        HandleEffecter(-1);
    }
}