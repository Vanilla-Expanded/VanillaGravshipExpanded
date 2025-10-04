using VEF.Graphics;
using Verse;

namespace VanillaGravshipExpanded;

public class CompProperties_DisconnectedFacilityOverlay : CompProperties
{
    public CustomOverlayDef overlayDef;

    public CompProperties_DisconnectedFacilityOverlay() => compClass = typeof(CompDisconnectedFacilityOverlay);
}