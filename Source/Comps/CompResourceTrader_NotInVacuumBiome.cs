using PipeSystem;
using Verse;

namespace VanillaGravshipExpanded;

public class CompResourceTrader_NotInVacuumBiome : CompResourceTrader
{
    protected new CompProperties_ResourceTrader_NotInVacuumBiome Props => (CompProperties_ResourceTrader_NotInVacuumBiome)props;
    protected OxygenPipeNet OxygenPipeNet => (OxygenPipeNet)PipeNet;

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);

        pipeNetOverlayDrawer = parent.Map.GetComponent<PipeNetOverlayDrawer>();

        if (OxygenPipeNet.noAtmosphere && Props.lowOxygenEnvironmentOverlay != null)
            pipeNetOverlayDrawer.TogglePulsing(parent, Props.lowOxygenEnvironmentOverlay, true);
    }

    public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
    {
        pipeNetOverlayDrawer.TogglePulsing(parent, Props.lowOxygenEnvironmentOverlay, false);

        base.PostDeSpawn(map, mode);
    }

    public override string CompInspectStringExtra()
    {
        if (OxygenPipeNet.noAtmosphere)
            return $"{"VGE_DisabledNoAtmosphere".Translate()}\n{base.CompInspectStringExtra()}";
        return base.CompInspectStringExtra();
    }

    public override bool CanBeOn() => !OxygenPipeNet.noAtmosphere && base.CanBeOn();
}