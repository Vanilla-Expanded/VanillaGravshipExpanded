using PipeSystem;
using Verse;

namespace VanillaGravshipExpanded;

public class CompResourceTrader_NotInVacuumBiome : CompResourceTrader
{
    // TODO: Move to custom PipeNet itself, which is still WIP
    private bool noAtmosphere;

    protected new CompProperties_ResourceTrader_NotInVacuumBiome Props => (CompProperties_ResourceTrader_NotInVacuumBiome)props;

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);

        pipeNetOverlayDrawer = parent.Map.GetComponent<PipeNetOverlayDrawer>();
        noAtmosphere = parent.Map.Biome.inVacuum;

        if (noAtmosphere)
        {
            Log.Error($"Toggled pulsing for {parent}, overlay: {Props.lowOxygenEnvironmentOverlay} (null={Props.lowOxygenEnvironmentOverlay == null})");
            pipeNetOverlayDrawer.TogglePulsing(parent, Props.lowOxygenEnvironmentOverlay, true);
        }
    }

    public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
    {
        pipeNetOverlayDrawer.TogglePulsing(parent, Props.lowOxygenEnvironmentOverlay, false);
        noAtmosphere = false;

        base.PostDeSpawn(map, mode);
    }

    public override string CompInspectStringExtra()
    {
        if (noAtmosphere)
            return $"{"VGSE_DisabledNoAtmosphere".Translate()}\n{base.CompInspectStringExtra()}";
        return base.CompInspectStringExtra();
    }

    public override bool CanBeOn() => !noAtmosphere && base.CanBeOn();
}