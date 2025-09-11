using Verse;

namespace VanillaGravshipExpanded;

public interface IGravEngineGraphic
{
    CachedMaterial OrbNormalMat { get; }
    CachedMaterial OrbCooldownMat { get; }
}