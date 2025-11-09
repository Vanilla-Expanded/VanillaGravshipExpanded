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
            var modExtension = explosion.weapon?.GetModExtension<TurretExtension_SubstructureDamage>();
            if (modExtension == null)
            {
                return;
            }

            var cell = c;
            var map = explosion.Map;
            var explosionCenter = explosion.Position;

            if (cell.DistanceTo(explosionCenter) > modExtension.substructureDamageRadius)
            {
                return;
            }

            var terrain = cell.GetTerrain(map);
            if (terrain == TerrainDefOf.Substructure)
            {
                map.terrainGrid.SetTerrain(cell, VGEDefOf.VGE_DamagedSubstructure);
                SpawnDebrisFilth(cell, map);
                ThingUtility.CheckAutoRebuildTerrainOnDestroyed(TerrainDefOf.Substructure, c, map);
            }
            else if (terrain == VGEDefOf.VGE_DamagedSubstructure || terrain == VGEDefOf.VGE_GravshipSubscaffold)
            {
                map.terrainGrid.RemoveFoundation(cell, false);
            }
        }

        public static void SpawnDebrisFilth(IntVec3 cell, Map map)
        {
            CellRect area = CellRect.FromCell(cell).ExpandedBy(1);
            int count = 0;
            foreach (IntVec3 filthCell in area.Cells.InRandomOrder())
            {
                if (filthCell.InBounds(map) && FilthMaker.TryMakeFilth(filthCell, map, VGEDefOf.VGE_Filth_DamagedSubstructure))
                {
                    count++;
                    if (count >= 3)
                    {
                        break;
                    }
                }
            }
        }
    }
}
