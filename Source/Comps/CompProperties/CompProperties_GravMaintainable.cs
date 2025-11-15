using Verse;

namespace VanillaGravshipExpanded
{
    public class CompProperties_GravMaintainable : CompProperties
    {

        public float minMaintenanceForAlert = 0.3f;
        public float fleckEmissionRate = 0.01f;
        public bool maintenanceFallsOutsideSubstructure = true;

        public CompProperties_GravMaintainable()
        {
            compClass = typeof(CompGravMaintainable);
        }
    }
}
