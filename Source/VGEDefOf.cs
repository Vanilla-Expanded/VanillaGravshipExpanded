using RimWorld;
using Verse;

namespace VanillaGravshipExpanded
{
    [DefOf]
    public static class VGEDefOf
    {
        public static StatDef VGE_GravshipTargeting, VGE_AccuracyGlobal, VGE_MaintenanceSensitivity, VGE_GravshipMaintenance, VGE_GravshipResearch, VGE_GravPowerAdjacency;
        public static WorldObjectDef VGE_ArtilleryProjectile;
        public static SoundDef VGE_GravshipTarget_Acquired;
        public static ThingDef Gun_MiniTurret, VGE_PointDefenseTurret, VGE_GaussSmoke, VGE_SealantGoop, VGE_Filth_DamagedSubstructure, VGE_Astrofire, VGE_AstrofireSpark, VGE_GravtechConsole, VGE_EngineeringConsole, VGE_PilotCockpit, VGE_PilotBridge, VGE_Compressed_Vacstone, VGE_OxygenCanister;
        public static TerrainDef VGE_GravshipSubscaffold, VGE_DamagedSubstructure, VGE_AsteroidIce, VGE_Compressed_Vacstone_Floor;
        public static OrbitalDebrisDef VGE_IceAsteroid, VGE_AsteroidCluster, VGE_MixedDebris;
        public static DamageDef VGE_ExtinguishAstrofire, VGE_AstrofireDamage;
        public static JobDef VGE_BeatAstrofire, VGE_ExtinguishAstrofiresNearby, VGE_ExtinguishSelfAstrofire, VGE_MaintainGrav;
        public static FleckDef VGE_MaintenanceSmoke;
        public static ResearchTabDef VGE_Gravtech;
        public static PreceptDef GravshipLaunch, VGE_GravjumperLaunch, VGE_GravhulkLaunch;
        public static GameConditionDef VGE_SpaceSolarFlare, VGE_GravitationalAnomaly;
    }
}
