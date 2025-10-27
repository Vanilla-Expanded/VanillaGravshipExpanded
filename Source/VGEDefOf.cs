using PipeSystem;
using RimWorld;
using VEF.Graphics;
using Verse;

namespace VanillaGravshipExpanded
{
    [DefOf]
    public static class VGEDefOf
    {
        public static StatDef VGE_GravshipTargeting, VGE_AccuracyGlobal, VGE_MaintenanceSensitivity, VGE_GravshipMaintenance, VGE_GravshipResearch, VGE_GravPowerAdjacency, VGE_GlobalMaintenanceSensitivity;
        public static WorldObjectDef VGE_ArtilleryProjectile, AsteroidMiningSite, VGE_GravshipGenerationSite;
        public static ThingDef Gun_MiniTurret, VGE_PointDefenseTurret, VGE_GaussSmoke, VGE_SealantGoop, VGE_Filth_DamagedSubstructure, VGE_Astrofire, VGE_AstrofireSpark, VGE_GravtechConsole, VGE_EngineeringConsole, VGE_PilotCockpit, VGE_PilotBridge, VGE_Compressed_Vacstone, VGE_OxygenCanister, VGE_MaintenanceHub, VGE_CapacitorHarmonizer, VGE_DamagedEscapePod, VGE_EscapePodSkyfaller, VGE_GravhulkEngine, VGE_GravjumperEngine, VGE_SolarPanelling;
        public static ThingDef VGE_AstrofuelPipe, VGE_Filth_Astrofuel, VGE_MechanoidPodLauncher, VGE_MechanoidTempShield;
        public static SoundDef VGE_GravshipTarget_Acquired, VGE_MicrometeorStorm;
        public static TerrainDef VGE_GravshipSubscaffold, VGE_DamagedSubstructure, VGE_AsteroidIce, VGE_Compressed_Vacstone_Floor, VGE_MechanoidSubstructure;
        public static OrbitalDebrisDef VGE_IceAsteroid, VGE_AsteroidCluster, VGE_MixedDebris;
        public static DamageDef VGE_ExtinguishAstrofire, VGE_AstrofireDamage;
        public static JobDef VGE_BeatAstrofire, VGE_ExtinguishAstrofiresNearby, VGE_ExtinguishSelfAstrofire, VGE_MaintainGrav, VGE_CollectGravdata, VGE_ReplenishOxygenPack;
        public static FleckDef VGE_MaintenanceSmoke;
        public static ResearchTabDef VGE_Gravtech;
        public static PreceptDef GravshipLaunch, VGE_GravjumperLaunch, VGE_GravhulkLaunch;
        public static GameConditionDef VGE_SpaceSolarFlare, VGE_GravitationalAnomaly, VGE_DustCloud;
        public static ThingDef VGE_SmallDebris, VGE_MediumDebris, VGE_LargeDebris, VGE_SmallAsteroid, VGE_MediumAsteroid, VGE_LargeAsteroid;
        public static WeatherDef VGE_ToxicDustCloud;
        public static PipeNetDef VGE_OxygenNet;
        public static PawnKindDef Rat, VGE_Astropede;
        public static ThoughtDef VGE_CrewEuphoria;
        public static ThingDef VGE_GiantThruster, VGE_GiantAstrofuelTank, LargeChemfuelTank;
        public static ThingDef VGE_Astrofuel;
        public static ThingDef DropPodLeavingMechanoid;
        public static CustomOverlayDef VGE_NoLinkedTerminalOverlay, VGE_NoLinkedTurretOverlay, VGE_MultipleGravEnginesOverlay;

        public static KCSG.StructureLayoutDef VGE_MechOrbitalDestroyer_Alpha, VGE_MechOrbitalDestroyer_Beta, VGE_MechOrbitalDestroyer_Gamma, VGE_StartingGravjumperDamaged, VGE_StartingGravjumper;
        public static DesignationCategoryDef Odyssey;
        public static MapGeneratorDef VGE_GravshipGeneration;
        public static TerrainDef VGE_FakeTerrain;
        public static ThingDef VGE_LandingStructure;

    }
}
