using PipeSystem;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded;

public class CompProperties_ResourceTrader_NotInVacuumBiome : CompProperties_ResourceTrader
{
    [NoTranslate]
    public string lowOxygenEnvironmentOverlayPath;
    [Unsaved]
    public Material lowOxygenEnvironmentOverlay;

    public CompProperties_ResourceTrader_NotInVacuumBiome() => compClass = typeof(CompResourceTrader_NotInVacuumBiome);

    public override void ResolveReferences(ThingDef parentDef)
    {
        base.ResolveReferences(parentDef);

        LongEventHandler.ExecuteWhenFinished(() => lowOxygenEnvironmentOverlay = MaterialPool.MatFrom(lowOxygenEnvironmentOverlayPath, ShaderDatabase.MetaOverlay));
    }
}