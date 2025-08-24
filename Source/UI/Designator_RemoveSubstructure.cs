using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded
{
    public class Designator_RemoveSubstructure : Designator_RemoveFloor
    {
        public Designator_RemoveSubstructure()
        {
            defaultLabel = "VGE_DesignatorRemoveSubstructure".Translate();
            defaultDesc = "VGE_DesignatorRemoveSubstructureDesc".Translate();
            icon = ContentFinder<Texture2D>.Get("UI/MenuIcons/RemoveSubstructure_Designator");
            useMouseIcon = true;
            soundDragSustain = SoundDefOf.Designate_DragStandard;
            soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
            soundSucceeded = SoundDefOf.Designate_RemoveBridge;
            hotKey = KeyBindingDefOf.Misc5;
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            if (!c.InBounds(Map) || c.Fogged(Map))
            {
                return false;
            }

            var report = CanDesignateSubstructure(c, Map.terrainGrid.FoundationAt(c));
            if (!report.Accepted)
            {
                return report;
            }

            Building edifice = c.GetEdifice(Map);
            if (edifice != null && edifice.def.Fillage == FillCategory.Full && edifice.def.passability == Traversability.Impassable)
            {
                return false;
            }

            return AcceptanceReport.WasAccepted;
        }

        private AcceptanceReport CanDesignateSubstructure(IntVec3 c, TerrainDef terrainDef)
        {
            if (terrainDef == null || !terrainDef.IsSustructureOrScaffold())
            {
                return false;
            }

            if (WorkGiver_ConstructRemoveFoundation.AnyBuildingBlockingFoundationRemoval(c, Map))
            {
                return "MessageCannotRemoveSupportingFoundation".Translate();
            }
            if (Map.designationManager.DesignationAt(c, DesignationDefOf.RemoveFoundation) != null)
            {
                return false;
            }
            return Map.terrainGrid.CanRemoveFoundationAt(c);
        }

        public override void DesignateSingleCell(IntVec3 c)
        {
            TerrainDef foundation = Map.terrainGrid.FoundationAt(c);
            if (foundation != null && foundation.IsSustructureOrScaffold())
            {
                if (DebugSettings.godMode)
                {
                    Map.terrainGrid.RemoveFoundation(c, doLeavings: false);
                }
                else
                {
                    Map.designationManager.AddDesignation(new Designation(c, DesignationDefOf.RemoveFoundation));
                }
            }
        }
    }
}
