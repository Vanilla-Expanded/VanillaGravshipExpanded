using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded
{
    [HarmonyPatch(typeof(DamageWorker), nameof(DamageWorker.ExplosionDamageTerrain))]
    public static class DamageWorker_ExplosionDamageTerrain_Patch
    {
        public static void Postfix(DamageWorker __instance, Explosion explosion, IntVec3 c)
        {
            var projectile = explosion.weapon?.GetModExtension<TurretExtension_SubstructureDamage>();
            if (projectile == null)
            {
                return;
            }

            var cell = c;
            var map = explosion.Map;
            var explosionCenter = explosion.Position;

            if (cell.DistanceTo(explosionCenter) > projectile.substructureDamageRadius)
            {
                return;
            }

            var terrain = cell.GetTerrain(map);
            TerrainDef newTerrain = null;
            if (terrain == TerrainDefOf.Substructure)
            {
                newTerrain = VGEDefOf.VGE_DamagedSubstructure;
            }
            else if (terrain == VGEDefOf.VGE_DamagedSubstructure || terrain == VGEDefOf.VGE_GravshipSubscaffold)
            {
                map.terrainGrid.Notify_TerrainDestroyed(cell);
            }
            if (newTerrain != null)
            {
                map.terrainGrid.SetTerrain(cell, newTerrain);
            }
        }
    }
}
