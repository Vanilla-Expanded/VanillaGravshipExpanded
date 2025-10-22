using RimWorld;
using Verse;

namespace VanillaGravshipExpanded
{
    [StaticConstructorOnStartup]
    public class Building_AutonomousTargeting : Building_TargetingTerminal
    {
        private CompPowerTrader powerComp;
        private AutonomousTargetingExtension _extension;
        private AutonomousTargetingExtension Extension => _extension ??= def.GetModExtension<AutonomousTargetingExtension>();

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            powerComp = GetComp<CompPowerTrader>();
        }

        public bool IsPowered => powerComp?.PowerOn ?? false;
        public override bool MannedByPlayer => IsPowered;
        public override float GravshipTargeting => Extension.gravshipTargeting;
    }
}
