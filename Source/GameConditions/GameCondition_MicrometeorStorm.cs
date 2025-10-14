using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using UnityEngine.Rendering;
using Verse;
using Verse.Sound;

namespace VanillaGravshipExpanded
{
    [HotSwappable]
    public class GameCondition_MicrometeorStorm : GameCondition
    {
        private static readonly WeatherOverlay_MicrometeorStorm micrometeorStormOverlay = new WeatherOverlay_MicrometeorStorm();

        private static readonly List<SkyOverlay> overlays = new List<SkyOverlay> { micrometeorStormOverlay };


        private Sustainer sustainer;

        public override List<SkyOverlay> SkyOverlays(Map map)
        {
            return overlays;
        }

        public override int TransitionTicks => 2000;

        public override float SkyTargetLerpFactor(Map map)
        {
            return GameConditionUtility.LerpInOutValue(this, TransitionTicks);
        }
        public override void GameConditionDraw(Map map)
        {
            micrometeorStormOverlay.DrawOverlay(map);
        }

        public override void GameConditionTick()
        {
            var map = SingleMap;
            if (sustainer == null || sustainer.Ended)
            {
                sustainer = VGEDefOf.VGE_MicrometeorStorm.TrySpawnSustainer(SoundInfo.OnCamera(MaintenanceType.PerTick));
            }
            else
            {
                sustainer.Maintain();
            }
            micrometeorStormOverlay.TickOverlay(map, 1f);
            if (Find.TickManager.TicksGame % 30 == 0)
            {
                ApplyMicrometeorDamage(map, 2000, 30);
            }
        }

        private void ApplyMicrometeorDamage(Map map, int tickInterval, int delta)
        {
            foreach (var building in map.listerBuildings.allBuildingsColonist.Concat(map.listerBuildings.allBuildingsNonColonist).ToList())
            {
                if (building.IsHashIntervalTick(tickInterval, delta) && !building.Position.Roofed(map) && Rand.Chance(0.5f))
                {
                    building.TakeDamage(new DamageInfo(DamageDefOf.Deterioration, building.MaxHitPoints * 0.01f));
                    EffecterDef effecterDef = EffecterDefOf.Deflect_Metal;
                    TargetInfo targetInfo = new TargetInfo(building.Position, map);
                    effecterDef.Spawn().Trigger(targetInfo, targetInfo);
                }
            }

            foreach (var pawn in map.mapPawns.AllPawnsSpawned.ToList())
            {
                if (pawn.IsHashIntervalTick(tickInterval, delta) && !pawn.Position.Roofed(map) && Rand.Chance(0.5f))
                {
                    var bodyPart = pawn.health.hediffSet.GetRandomNotMissingPart(DamageDefOf.Cut, depth: BodyPartDepth.Outside);
                    if (bodyPart != null)
                    {
                        var dinfo = new DamageInfo(DamageDefOf.Cut, 12f, 0f, -1f, null, bodyPart);
                        pawn.TakeDamage(dinfo);
                    }
                }
            }
        }
    }
}
