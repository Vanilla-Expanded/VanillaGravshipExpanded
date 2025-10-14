using HarmonyLib;
using RimWorld;
using Verse;
using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using System.Collections.Generic;
using VanillaGravshipExpanded;

namespace VanillaGravshipExpanded
{
    [HotSwappable]
    [HarmonyPatch(typeof(WorldComponent_GravshipController), "LandingEnded")]
    public static class WorldComponent_GravshipController_LandingEnded_Patch
    {
        public static Dictionary<Building_GravEngine, bool> gravdataCorruptionOccurred = new Dictionary<Building_GravEngine, bool>();
        public static void Prefix(WorldComponent_GravshipController __instance, out (Gravship gravship, Dictionary<LandingOutcomeDef, float> outcomes) __state)
        {
            try
            {
                var gravship = __instance.gravship;
                gravdataCorruptionOccurred[gravship.Engine] = false;
                ApplyCrashlanding(gravship, __instance.map);
                RegenScaffondingSections(gravship, __instance.map);
                __state = (gravship, new Dictionary<LandingOutcomeDef, float>());
                var customOutcomes = DefDatabase<LandingOutcomeDef>.AllDefsListForReading
                    .Where(x => x.Worker is LandingOutcomeWorker_GravshipBase)
                    .ToList();

                foreach (var outcome in customOutcomes)
                {
                    __state.outcomes[outcome] = outcome.weight;
                    var worker = outcome.Worker as LandingOutcomeWorker_GravshipBase;
                    if (worker != null && worker.CanTrigger(gravship) is false)
                    {
                        outcome.weight = 0;
                    }
                }

                // Remove cooldown if there's a grav anchor
                if (__instance.map.listerThings.AnyThingWithDef(ThingDefOf.GravAnchor))
                    __instance.gravship.engine.cooldownCompleteTick = GenTicks.TicksGame;
            }
            catch (System.Exception ex)
            {
                Log.Error($"[VGE] Exception in LandingEnded Prefix: {ex}");
                __state = (null, null);
            }
        }

        public static void Postfix(WorldComponent_GravshipController __instance, (Gravship gravship, Dictionary<LandingOutcomeDef, float> outcomes) __state)
        {
            try
            {
                bool negateMaintenance = false;
                int distanceTravelled = 0;
                var gravship = __state.gravship;
                ApplyGravDataYield(gravship, out distanceTravelled);
                TryTriggerLaunchBoon(gravship, out negateMaintenance);
                if (!negateMaintenance)
                {
                    CalculateMaintenanceLoss(gravship, distanceTravelled, 1);
                }
                gravdataCorruptionOccurred[gravship.Engine] = false;
                foreach (var kvp in __state.outcomes)
                {
                    kvp.Key.weight = kvp.Value;
                }
                Log.Message(__state.outcomes.Select(kvp => $"{kvp.Key.defName}: {kvp.Key.weight}").ToStringSafeEnumerable());
            }
            catch (System.Exception ex)
            {
                Log.Error($"[VGE] Exception in LandingEnded Postfix: {ex}");
            }
        }


        private static void ApplyGravDataYield(Gravship gravship, out int distanceTravelled)
        {
            var launchInfo = gravship.Engine?.launchInfo;
            if (launchInfo == null || LaunchInfo_ExposeData_Patch.launchSourceTiles.TryGetValue(launchInfo, out var launchSourceTile) is false)
            {
                Log.Error($"[VGE] No launch info found, skipping gravdata yield");
                distanceTravelled = 0;
                return;
            }
            var landingTile = gravship.Engine.Tile;
            float quality = launchInfo.quality;
            int gravdataYield;

            Log.Message($"[VGE] Landing at tile: {landingTile}");
            Log.Message($"[VGE] Quality: {quality}");

            LaunchInfo_ExposeData_Patch.gravtechResearcherPawns.TryGetValue(launchInfo, out var researcherPawn);
            Log.Message($"[VGE] Researcher pawn: {researcherPawn?.Name}");

            distanceTravelled = GravshipHelper.GetDistance(launchSourceTile, landingTile);
            Log.Message($"[VGE] Distance travelled: {distanceTravelled} - from {launchSourceTile} to {landingTile}");

            var blackBox = gravship.Engine.GravshipComponents.Select(comp => comp.parent).OfType<Building_GravshipBlackBox>().FirstOrDefault();

            if (gravdataCorruptionOccurred.TryGetValue(gravship.Engine, out bool corruptionOccurred) && corruptionOccurred)
            {
                gravdataYield = 0;
                Log.Message($"[VGE] Gravdata corruption occurred, yield set to 0");
                if (blackBox != null)
                {
                    Log.Message($"[VGE] Clearing black box due to gravdata corruption");
                    blackBox.TakeGravdata(blackBox.StoredGravdata);
                }
            }
            else
            {
                gravdataYield = GravdataUtility.CalculateGravdataYield(distanceTravelled, quality, gravship.Engine, researcherPawn);
                Log.Message($"[VGE] Calculated gravdata yield: {gravdataYield}");
            }

            int remainingGravdata = gravdataYield;
            if (blackBox != null)
            {
                var toAdd = blackBox.TakeGravdata(blackBox.StoredGravdata);
                Log.Message($"[VGE] Fetching {toAdd} gravdata from black box");
                remainingGravdata += toAdd;
            }
            if (World_ExposeData_Patch.currentGravtechProject != null)
            {
                Log.Message($"[VGE] Adding {remainingGravdata} to project: {World_ExposeData_Patch.currentGravtechProject.defName}");
                float progressNeeded = World_ExposeData_Patch.currentGravtechProject.Cost - Find.ResearchManager.GetProgress(World_ExposeData_Patch.currentGravtechProject);
                int progressToAdd = Mathf.Min(remainingGravdata, (int)progressNeeded);

                Find.ResearchManager.AddProgress(World_ExposeData_Patch.currentGravtechProject, progressToAdd);
                remainingGravdata -= progressToAdd;

                if (World_ExposeData_Patch.currentGravtechProject.IsFinished)
                {
                    Log.Message($"[VGE] Project completed: {World_ExposeData_Patch.currentGravtechProject.defName}");
                    World_ExposeData_Patch.currentGravtechProject = null;
                }
            }
            if (blackBox != null && remainingGravdata > 0)
            {
                Log.Message($"[VGE] Storing {remainingGravdata} gravdata in black box");
                blackBox.AddGravdata(remainingGravdata);
            }
            else if (remainingGravdata > 0)
            {
                Log.Message($"[VGE] No black box, {remainingGravdata} gravdata lost");
            }

            LaunchInfo_ExposeData_Patch.gravtechResearcherPawns.Remove(launchInfo);
            LaunchInfo_ExposeData_Patch.launchSourceTiles.Remove(launchInfo);
        }

        public static void CalculateMaintenanceLoss(Gravship gravship, int distanceTravelled, float chance)
        {
            if (gravship.Engine.Map is null)
            {
                Log.Error("[VGE] gravship engine has no map, skipping maintenance loss.");
                return;
            }
            GravMaintainables_MapComponent comp = gravship.Engine.Map.GetComponent<GravMaintainables_MapComponent>();

            if (comp != null)
            {
                gravship.Engine.Map.GetComponent<GravMaintainables_MapComponent>().ChangeGlobalMaintenance(-0.001f * distanceTravelled, chance);

            }
            else
            {
                Log.Error($"[VGE] Map lacked the GravMaintainables_MapComponent, can't handle maintenance loss");
            }


        }

        private static void TryTriggerLaunchBoon(Gravship gravship, out bool negateMaintenance)
        {
            negateMaintenance = false;
            var launchInfo = gravship.Engine?.launchInfo;
            if (launchInfo == null)
            {
                return;
            }

            float quality = launchInfo.quality;
            float boonChance = GravshipHelper.LaunchBoonChanceFromQuality(quality);
            if (Rand.Chance(boonChance))
            {
                var launchBoons = DefDatabase<LaunchBoonDef>.AllDefsListForReading.Where(x => x.Worker.CanTrigger(gravship)).ToList();
                if (launchBoons.TryRandomElementByWeight(x => x.weight, out LaunchBoonDef selectedBoon))
                {
                    selectedBoon.Worker.ApplyBoon(gravship);
                    if (selectedBoon.negateMaintenance)
                    {
                        negateMaintenance = true;
                    }
                }
            }
        }

        private static void RegenScaffondingSections(Gravship gravship, Map map)
        {
            var foundations = gravship.Foundations.Keys;
            foreach (var cell in foundations)
            {
                var loc = cell + gravship.Engine.Position;
                if (loc.InBounds(map) && loc.GetTerrain(map) == VGEDefOf.VGE_GravshipSubscaffold)
                {
                    map.mapDrawer.MapMeshDirty(loc, MapMeshFlagDefOf.Terrain, regenAdjacentCells: true, regenAdjacentSections: false);
                }
            }
        }

        private static void ApplyCrashlanding(Gravship gravship, Map map)
        {
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
                    var comp = blocker.TryGetComp<CompExplosive>();
                    if (comp != null)
                    {
                        comp.Detonate(map, ignoreUnspawned: true);
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
