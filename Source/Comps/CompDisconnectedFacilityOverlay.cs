using RimWorld;
using VEF.Graphics;
using Verse;

namespace VanillaGravshipExpanded;

public class CompDisconnectedFacilityOverlay : ThingComp
{
    protected CustomOverlayDrawer overlayDrawer;
    protected CompFacility facility;

    protected CompProperties_DisconnectedFacilityOverlay Props => (CompProperties_DisconnectedFacilityOverlay)props;

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        overlayDrawer = parent.Map.GetComponent<CustomOverlayDrawer>();
        facility.OnLinkAdded += Notify_LinkAdded;
        facility.OnLinkRemoved += Notify_LinkRemoved;

        // Handle situation where the facility is linked already
        if (facility.LinkedBuildings.Count > 0)
            Notify_LinkAdded(facility, null);
        else
            Notify_LinkRemoved(facility, null);
    }

    public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
    {
        overlayDrawer = null;
        facility.OnLinkAdded -= Notify_LinkAdded;
        facility.OnLinkRemoved -= Notify_LinkRemoved;
    }

    public override void PostPostMake()
    {
        InitComps();
    }

    public override void PostExposeData()
    {
        InitComps();
    }

    private void InitComps()
    {
        facility = parent.GetComp<CompFacility>();
    }

    protected virtual void Notify_LinkAdded(CompFacility facility, Thing thing)
    {
        overlayDrawer?.Disable(parent, Props.overlayDef);
    }

    protected virtual void Notify_LinkRemoved(CompFacility facility, Thing thing)
    {
        if (facility.LinkedBuildings.Count <= 0)
            overlayDrawer?.Enable(parent, Props.overlayDef);
    }
}