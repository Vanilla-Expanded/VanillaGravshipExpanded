using RimWorld;
using Verse;
using System.Linq;

namespace VanillaGravshipExpanded
{
    public abstract class CompFacilityConnected : ThingComp
    {
        protected CompGravshipFacility facility;
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            InitializeComps();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            if (Scribe.mode == LoadSaveMode.LoadingVars)
                InitializeComps();
        }

        protected virtual void InitializeComps()
        {
            facility = parent.GetComp<CompGravshipFacility>();
        }

        public bool CanBeOn(out Building_GravEngine engine)
        {
            engine = null;
            if (!facility.LinkedBuildings.NullOrEmpty() && facility.CanBeActive)
            {
                var linkedEngine = facility.LinkedBuildings.OfType<Building_GravEngine>().FirstOrDefault();
                if (linkedEngine != null)
                {
                    engine = linkedEngine;
                    return true;
                }
            }
            return false;
        }
    }
}
