using HarmonyLib;
using RimWorld;
using Verse;
using System.Linq;
using RimWorld.Planet;
using System.Collections.Generic;

namespace VanillaGravshipExpanded
{
    [HotSwappable]
    [HarmonyPatch(typeof(WorldComponent_GravshipController), "LandingEnded")]
    public static class WorldComponent_GravshipController_LandingEnded_Patch
    {
        public static void Prefix(WorldComponent_GravshipController __instance)
        {
            var gravship = __instance.gravship;
            var map = gravship.Engine.Map;
            var foundations = gravship.Foundations.Keys;
            
            foreach (var blocker in GravshipMapGenUtility.BlockingThings)
            {
                var cells = blocker.OccupiedRect();
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
                            }
                            else if (terrain == VGEDefOf.VGE_DamagedSubstructure)
                            {
                                map.terrainGrid.RemoveFoundation(cell);
                            }
                            // TODO: figure out what to do with VGE_GravshipSubscaffolding once its created
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
                        if (terrain == TerrainDefOf.Substructure || terrain == VGEDefOf.VGE_DamagedSubstructure)
                        {
                            map.terrainGrid.RemoveFoundation(cell);
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
