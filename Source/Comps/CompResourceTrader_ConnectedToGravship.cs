using PipeSystem;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

public class CompResourceTrader_ConnectedToGravship : CompResourceTrader
{
    private CompGravshipFacility facility;

    public override bool CanBeOn()
    {
        return base.CanBeOn() && (facility == null || facility.CanBeActive);
    }

    public override void PostPostMake()
    {
        base.PostPostMake();
        InitializeComps();
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        if (Scribe.mode == LoadSaveMode.LoadingVars)
            InitializeComps();
    }

    private void InitializeComps()
    {
        facility = parent.GetComp<CompGravshipFacility>();
        
        // facility.OnLinkAdded += FacilityOnOnLinkAdded;
        // facility.OnLinkRemoved += FacilityOnOnLinkRemoved;
    }

    // private void FacilityOnOnLinkRemoved(CompFacility arg1, Thing arg2)
    // {
    //     throw new System.NotImplementedException();
    // }
    //
    // private void FacilityOnOnLinkAdded(CompFacility arg1, Thing arg2)
    // {
    //     throw new System.NotImplementedException();
    // }
}