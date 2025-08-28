using System.Text;
using PipeSystem;
using Verse;

namespace VanillaGravshipExpanded;

public class CompResourceThruster : CompResource
{
    public PipeNetOverlayDrawer pipeNetOverlayDrawer;

    public new CompProperties_ResourceThruster Props => (CompProperties_ResourceThruster)props;

    public AstrofuelPipeNet AstrofuelNet => (AstrofuelPipeNet)base.PipeNet;

    public bool HasFuel => AstrofuelNet is { HasFuel: true };

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);

        pipeNetOverlayDrawer = parent.Map.GetComponent<PipeNetOverlayDrawer>();

        if (!HasFuel)
            pipeNetOverlayDrawer.TogglePulsing(parent, Props.outOfFuelOverlay, true);
    }

    public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
    {
        base.PostDeSpawn(map, mode);

        pipeNetOverlayDrawer.TogglePulsing(parent, Props.outOfFuelOverlay, false);
    }

    public override string CompInspectStringExtra()
    {
        var builder = new StringBuilder();

        if (!HasFuel)
            builder.Append("VGE_DisabledNoFuel".Translate());

        builder.AppendInNewLine(base.CompInspectStringExtra());
        return builder.ToString();
    }
}