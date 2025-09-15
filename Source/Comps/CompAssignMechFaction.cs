using RimWorld;
using Verse;

namespace VanillaGravshipExpanded
{
    public class CompProperties_AssignMechFaction : CompProperties
    {
        public CompProperties_AssignMechFaction()
        {
            this.compClass = typeof(CompAssignMechFaction);
        }
    }

    public class CompAssignMechFaction : ThingComp
    {
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (respawningAfterLoad is false && this.parent.Faction != Faction.OfMechanoids)
            {
                this.parent.SetFactionDirect(Faction.OfMechanoids);
            }
        }
    }
}
