using RimWorld.Planet;
using Verse;

namespace VanillaGravshipExpanded
{
    [HotSwappable]
    public class GravshipGenerationSite : MapParent
    {
        public override MapGeneratorDef MapGeneratorDef => VGEDefOf.VGE_GravshipGeneration;
        public override bool ShouldRemoveMapNow(out bool alsoRemoveWorldObject)
        {
            alsoRemoveWorldObject = false;
            return false;
        }
    }
}
