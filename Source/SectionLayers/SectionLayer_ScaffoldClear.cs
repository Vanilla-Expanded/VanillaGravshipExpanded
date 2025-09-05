using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded
{
    [HotSwappable]
    public class SectionLayer_ScaffoldClear : SectionLayer_ScaffoldBase
    {
        public SectionLayer_ScaffoldClear(Section section) : base(section)
        {
        }

        protected override Material GetScaffoldingMaterial()
        {
            return GravshipHelper.TileMaterial;
        }
    }
}
