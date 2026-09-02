using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded
{
    public class CompProperties_GravMaintainable : CompProperties
    {

        public float minMaintenanceForAlert = 0.3f;
        public float fleckEmissionRate = 0.01f;
        public bool maintenanceFallsOutsideSubstructure = true;
        public bool toggleMaintainGizmoEnabled = false;
        public bool toggleMaintainGizmoAlwaysEnabled = false;
        public string toggleMaintainGizmoIconPath = null;
        public string toggleMaintainLabelKey = "VGE_ToggleMaintainBuilding";
        public string toggleMaintainDescKey = "VGE_ToggleMaintainBuildingDesc";
        [Unsaved] protected CachedTexture toggleMaintainGizmoIcon;

        public Texture2D ToggleMaintainGizmoIcon => (toggleMaintainGizmoIcon ??= new CachedTexture(toggleMaintainGizmoIconPath)).Texture;

        public CompProperties_GravMaintainable()
        {
            compClass = typeof(CompGravMaintainable);
        }
    }
}
