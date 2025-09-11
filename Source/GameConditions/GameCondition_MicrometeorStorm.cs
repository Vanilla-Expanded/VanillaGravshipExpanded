using System.Collections.Generic;
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
        [HotSwappable]
        public class WeatherOverlay_MicrometeorStorm : WeatherOverlayDualPanner
        {
            public WeatherOverlay_MicrometeorStorm()
            {
                Init();
            }

            public void Init()
            {
                LongEventHandler.ExecuteWhenFinished(delegate
                {
                    worldOverlayPanSpeed1 = 0.03f;
                    worldPanDir1 = new Vector2(-0.25f, -1f);
                    worldPanDir1.Normalize();
                    worldOverlayPanSpeed2 = 0.04f;
                    worldPanDir2 = new Vector2(-0.24f, -1f);
                    worldPanDir2.Normalize();

                    worldOverlayMat = new Material(MaterialPool.MatFrom("Weather/MicrometeorStormWorldOverlay"));
                    var mat = MatLoader.LoadMat("Weather/SnowOverlayWorld");
                    worldOverlayMat.CopyPropertiesFromMaterial(mat);
                    worldOverlayMat.shader = mat.shader;
                    Texture2D texture = ContentFinder<Texture2D>.Get("Weather/MicrometeorStormWorldOverlay");
                    worldOverlayMat.SetTexture("_MainTex", texture);
                    worldOverlayMat.SetTexture("_MainTex2", texture);
                    worldOverlayMat.SetColor("_TuningColor", new Color(0.272f, 0.272f, 0.272f, 0.6f));
                });
            }
        }

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
            foreach (var building in map.listerBuildings.allBuildingsColonist)
            {
                if (building.IsHashIntervalTick(tickInterval, delta) && !building.Position.Roofed(map) && Rand.Chance(0.5f))
                {
                    building.TakeDamage(new DamageInfo(DamageDefOf.Deterioration, building.MaxHitPoints * 0.01f));
                    EffecterDef effecterDef = EffecterDefOf.Deflect_Metal;
                    TargetInfo targetInfo = new TargetInfo(building.Position, map);
                    effecterDef.Spawn().Trigger(targetInfo, targetInfo);
                }
            }

            foreach (var pawn in map.mapPawns.AllPawnsSpawned)
            {
                if (pawn.IsHashIntervalTick(tickInterval, delta) && !pawn.Position.Roofed(map) && Rand.Chance(0.5f))
                {
                    var bodyPart = pawn.health.hediffSet.GetNotMissingParts().RandomElementWithFallback();
                    if (bodyPart != null)
                    {
                        var dinfo = new DamageInfo(DamageDefOf.Cut, 12f, 99f, -1f, null, bodyPart);
                        pawn.TakeDamage(dinfo);
                    }
                }
            }
        }
    }
}
