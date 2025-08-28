using System.Collections.Generic;
using PipeSystem;

namespace VanillaGravshipExpanded;

public class AstrofuelPipeNet : PipeNet
{
    public List<CompResourceThruster> thrusters = [];

    public bool thrustersDirty = true;

    public bool HasFuel { get; private set; }

    public override void RegisterComp(CompResource comp)
    {
        base.RegisterComp(comp);

        if (comp is CompResourceThruster thruster)
            thrusters.Add(thruster);
    }

    public override void UnregisterComp(CompResource comp)
    {
        base.UnregisterComp(comp);

        if (comp is CompResourceThruster thruster)
            thrusters.Remove(thruster);
    }

    public override void Merge(PipeNet otherNet)
    {
        base.Merge(otherNet);

        thrustersDirty = true;
    }

    public override void PipeSystemTick()
    {
        base.PipeSystemTick();

        var hadFuelLastTick = HasFuel;
        HasFuel = Stored > 0.9999999999f;
        if (thrustersDirty || HasFuel != hadFuelLastTick)
        {
            for (var i = 0; i < thrusters.Count; i++)
            {
                var thruster = thrusters[i];
                thruster.pipeNetOverlayDrawer.TogglePulsing(thruster.parent, thruster.Props.outOfFuelOverlay, !HasFuel);
            }
        }
    }
}