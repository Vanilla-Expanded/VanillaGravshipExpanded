using System.Text;
using PipeSystem;
using VEF.Graphics;
using Verse;

namespace VanillaGravshipExpanded;

public class CompResourceThruster : CompResource
{
    public CustomOverlayDrawer overlayDrawer;

    public new CompProperties_ResourceThruster Props => (CompProperties_ResourceThruster)props;

    public AstrofuelPipeNet AstrofuelNet => (AstrofuelPipeNet)base.PipeNet;

    public bool HasFuel => AstrofuelNet is { HasFuel: true };

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);

        overlayDrawer = parent.Map.GetComponent<CustomOverlayDrawer>();

        if (!HasFuel)
            overlayDrawer.Enable(parent, Props.outOfFuelOverlay);
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