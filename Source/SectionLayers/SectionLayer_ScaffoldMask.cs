using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded
{
    [HotSwappable]
    public class SectionLayer_ScaffoldMask : SectionLayer_ScaffoldBase
    {
        public SectionLayer_ScaffoldMask(Section section) : base(section)
        {
        }

        protected override Material GetScaffoldingMaterial()
        {
            return GravshipHelper.MaskOverlayMaterial;
        }
        
        protected override Material GetEdgeMaterial(CachedMaterial cachedMaterial, Rot4 rotation)
        {
            return GravshipHelper.MaskOverlayMaterial;
        }
        
        protected override Material GetCornerMaterial(CachedMaterial cachedMaterial)
        {
            return GravshipHelper.MaskOverlayMaterial;
        }
        
        protected override Material GetGravshipMaskMaterial(Material originalMaterial)
        {
            return GravshipHelper.MaskOverlayMaterial;
        }
        
        protected override AltitudeLayer GetAltitudeLayer()
        {
            return AltitudeLayer.TerrainScatter;
        }
    }
}
