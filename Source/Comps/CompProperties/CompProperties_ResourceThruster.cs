using PipeSystem;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded;

public class CompProperties_ResourceThruster : CompProperties_Resource
{
    [NoTranslate]
    public string outOfFuelOverlayPath;
    [Unsaved]
    public Material outOfFuelOverlay;

    public CompProperties_ResourceThruster() => compClass = typeof(CompResourceThruster);

    public override void ResolveReferences(ThingDef parentDef)
    {
        base.ResolveReferences(parentDef);

        LongEventHandler.ExecuteWhenFinished(() => outOfFuelOverlay = MaterialPool.MatFrom(outOfFuelOverlayPath, ShaderDatabase.MetaOverlay));
    }
}