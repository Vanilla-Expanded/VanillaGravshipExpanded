using HarmonyLib;
using RimWorld;
using Verse;
using System.Linq;
using RimWorld.Planet;

namespace VanillaGravshipExpanded
{
    [HotSwappable]
    [HarmonyPatch(typeof(WorldComponent_GravshipController), "LandingEnded")]
    public static class WorldComponent_GravshipController_LandingEnded_Patch
    {
        public static void Prefix(WorldComponent_GravshipController __instance)
        {
            var gravship = __instance.gravship;
            ApplyGravDataYield(gravship);
            ApplyCrashlanding(gravship);
            RegenScaffondingSections(gravship);
        }

        private static void ApplyGravDataYield(Gravship gravship)
        {
            var launchInfo = gravship.Engine.launchInfo;
            if (launchInfo == null)
            {
                Log.Error($"[Gravdata] No launch info found, skipping gravdata yield");
                return;
            }
            var landingTile = gravship.Engine.Tile;
            float quality = launchInfo.quality;

            Log.Message($"[Gravdata] Landing at tile: {landingTile}");
            Log.Message($"[Gravdata] Quality: {quality}");

            LaunchInfo_ExposeData_Patch.gravtechResearcherPawns.TryGetValue(launchInfo, out var researcherPawn);
            Log.Message($"[Gravdata] Researcher pawn: {researcherPawn?.Name}");

            var launchSourceTile = LaunchInfo_ExposeData_Patch.launchSourceTiles[launchInfo];
            var distanceTravelled = GravshipHelper.GetDistance(launchSourceTile, landingTile);
            Log.Message($"[Gravdata] Distance travelled: {distanceTravelled} - from {launchSourceTile} to {landingTile}");

            var gravdataYield = GravdataUtility.CalculateGravdataYield(distanceTravelled, quality, gravship.Engine, researcherPawn);

            Log.Message($"[Gravdata] Calculated gravdata yield: {gravdataYield}");
            if (World_ExposeData_Patch.currentGravtechProject != null)
            {
                Log.Message($"[Gravdata] Adding {gravdataYield} to project: {World_ExposeData_Patch.currentGravtechProject.defName}");
                Find.ResearchManager.AddProgress(World_ExposeData_Patch.currentGravtechProject, gravdataYield);
                if (World_ExposeData_Patch.currentGravtechProject.IsFinished)
                {
                    Log.Message($"[Gravdata] Project completed: {World_ExposeData_Patch.currentGravtechProject.defName}");
                    World_ExposeData_Patch.currentGravtechProject = null;
                }
            }
            else
            {
                Log.Message($"[Gravdata] No current project selected, gravdata lost");
            }
            LaunchInfo_ExposeData_Patch.gravtechResearcherPawns.Remove(launchInfo);
            LaunchInfo_ExposeData_Patch.launchSourceTiles.Remove(launchInfo);
        }

        private static void RegenScaffondingSections(Gravship gravship)
        {
            var map = gravship.Engine.Map;
            var foundations = gravship.Foundations.Keys;
            ulong dirtyFlags = (ulong)MapMeshFlagDefOf.Terrain;
            foreach (var cell in foundations)
            {
                var loc = cell + gravship.Engine.Position;
                if (loc.InBounds(map) && loc.GetTerrain(map) == VGEDefOf.VGE_GravshipSubscaffold)
                {
                    map.mapDrawer.MapMeshDirty(loc, dirtyFlags, regenAdjacentCells: true, regenAdjacentSections: false);
                }
            }
        }

        private static void ApplyCrashlanding(Gravship gravship)
        {
            var map = gravship.Engine.Map;
            foreach (var blocker in GravshipMapGenUtility.BlockingThings)
            {
                if (blocker.def.destroyable)
                {
                    float damageAmount = blocker.MaxHitPoints * 0.25f;
                    foreach (var cell in blocker.OccupiedRect())
                    {
                        foreach (var thing in gravship.Things.Where(t => t.Position == cell))
                        {
                            thing.TakeDamage(new DamageInfo(DamageDefOf.Blunt, damageAmount));
                        }

                        if (blocker.MaxHitPoints >= 300 && Rand.Chance(GetSubstructureDamageChance(blocker.MaxHitPoints)))
                        {
                            var terrain = map.terrainGrid.FoundationAt(cell);
                            if (terrain == TerrainDefOf.Substructure)
                            {
                                map.terrainGrid.SetFoundation(cell, VGEDefOf.VGE_DamagedSubstructure);
                                DamageWorker_ExplosionDamageTerrain_Patch.SpawnDebrisFilth(cell, map);
                            }
                            else if (terrain == VGEDefOf.VGE_DamagedSubstructure || terrain == VGEDefOf.VGE_GravshipSubscaffold)
                            {
                                map.terrainGrid.RemoveFoundation(cell, false);
                            }
                        }
                    }
                    if (blocker.Destroyed is false)
                    {
                        blocker.Destroy(DestroyMode.Vanish);
                    }
                }
                else
                {
                    foreach (var cell in blocker.OccupiedRect())
                    {
                        foreach (var thing in gravship.Things.Where(t => t.def.destroyable && t.Position == cell).ToList())
                        {
                            if (thing.Destroyed is false)
                            {
                                thing.Destroy(DestroyMode.Vanish);
                            }
                        }
                        var terrain = map.terrainGrid.FoundationAt(cell);
                        if (terrain == TerrainDefOf.Substructure || terrain == VGEDefOf.VGE_DamagedSubstructure || terrain == VGEDefOf.VGE_GravshipSubscaffold)
                        {
                            map.terrainGrid.RemoveFoundation(cell, false);
                        }
                    }
                }
            }
        }

        private static float GetSubstructureDamageChance(float hp)
        {
            if (hp < 300) return 0f;
            if (hp >= 1000) return 1f;
            if (hp < 600) return GenMath.LerpDouble(300, 600, 0.1f, 0.25f, hp);
            return GenMath.LerpDouble(600, 1000, 0.25f, 1f, hp);
        }
    }
}
