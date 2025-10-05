using PipeSystem;
using UnityEngine;
using VEF.Graphics;
using Verse;

namespace VanillaGravshipExpanded;

public class CompProperties_ResourceThruster : CompProperties_Resource
{
    public CustomOverlayDef outOfFuelOverlay;

    public CompProperties_ResourceThruster() => compClass = typeof(CompResourceThruster);
}