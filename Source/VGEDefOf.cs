using RimWorld;
using Verse;

namespace VanillaGravshipExpanded
{
    [DefOf]
    public static class VGEDefOf
    {
        public static StatDef VGE_GravshipTargeting, VGE_AccuracyGlobal, VGE_MaintenanceSensitivity, VGE_GravshipMaintenance;
        public static WorldObjectDef VGE_ArtilleryProjectile;
        public static SoundDef VGE_GravshipTarget_Acquired;
        public static ThingDef Gun_MiniTurret, VGE_PointDefenseTurret, VGE_GaussSmoke, VGE_SealantGoop, VGE_Filth_DamagedSubstructure, VGE_Astrofire, VGE_AstrofireSpark;
        public static TerrainDef VGE_GravshipSubscaffold, VGE_DamagedSubstructure, VGE_AsteroidIce;
        public static OrbitalDebrisDef VGE_IceAsteroid, VGE_AsteroidCluster, VGE_MixedDebris;
        public static DamageDef VGE_ExtinguishAstrofire, VGE_AstrofireDamage;
        public static JobDef VGE_BeatAstrofire, VGE_ExtinguishAstrofiresNearby, VGE_ExtinguishSelfAstrofire, VGE_MaintainGrav;
        public static FleckDef VGE_MaintenanceSmoke;
    }
}
