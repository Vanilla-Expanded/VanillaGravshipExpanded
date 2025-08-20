using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

public class CompFacility_Astropurifier : CompFacility
{
    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        OnLinkAdded += Notify_LinkAdded;
        OnLinkRemoved += Notify_LinkRemoved;

        base.PostSpawnSetup(respawningAfterLoad);
    }

    private void Notify_LinkAdded(CompFacility facility, Thing link) => link.TryGetComp<CompResourceTrader_AstrofuelSynthesizer>()?.Notify_LinkAdded(this);

    private void Notify_LinkRemoved(CompFacility facility, Thing link) => link.TryGetComp<CompResourceTrader_AstrofuelSynthesizer>()?.Notify_LinkRemoved(this);
}