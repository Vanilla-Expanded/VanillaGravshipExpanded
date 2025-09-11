using System.Collections.Generic;
using System.Linq;
using LudeonTK;
using RimWorld;
using UnityEngine;
using Verse;
namespace VanillaGravshipExpanded
{
    public class GameCondition_GravitationalAnomaly : GameCondition
    {
        [StaticConstructorOnStartup]
        private class GravAnomalyOverlay : SkyOverlay
        {
            private static readonly Material GravAnomalyOverlayWorld = MatLoader.LoadMat("Weather/GlowSporeOverlayWorld");

            private static readonly ComplexCurve speedCurve = new ComplexCurve(new UnityEngine.Keyframe(0f, 0f), new UnityEngine.Keyframe(1f, 1f), new UnityEngine.Keyframe(2f, 1.1f), new UnityEngine.Keyframe(3f, 1.2f));

            private TexturePannerSpeedCurve panner0 = new TexturePannerSpeedCurve(GravAnomalyOverlayWorld, "_MainTex", speedCurve, new Vector2(-1f, -0.2f), 0.0001f);

            private TexturePannerSpeedCurve panner1 = new TexturePannerSpeedCurve(GravAnomalyOverlayWorld, "_MainTex2", speedCurve, new Vector2(0.35f, -1f), 5E-05f);

            public override void SetOverlayColor(Color color)
            {
                GravAnomalyOverlayWorld.color = color;
            }

            public override void DrawOverlay(Map map)
            {
                SkyOverlay.DrawWorldOverlay(map, GravAnomalyOverlayWorld);
            }

            public override void TickOverlay(Map map, float lerpFactor)
            {
                panner0.Tick();
                panner1.Tick();
            }
        }

        private int curColorIndex = -1;

        private int prevColorIndex = -1;

        private float curColorTransition;

        private const float SkyColorStrength = 0.075f;

        private const float OverlayColorStrength = 0.025f;

        private const int TransitionDurationTicks_NotPermanent = 280;

        private int longTickCounter = 0;

        private static readonly GravAnomalyOverlay gravAnomalyOverlay = new GravAnomalyOverlay();

        private static readonly List<SkyOverlay> overlays = new List<SkyOverlay> { gravAnomalyOverlay };

        private static readonly Color[] Colors = new Color[8]
        {
            new Color(0f, 0f, 1f),
            new Color(0.3f, 0.3f, 1f),
            new Color(0f, 0.7f, 1f),
            new Color(0.3f, 0.7f, 1f),
            new Color(0f, 0.5f, 1f),
            new Color(0.2f, 0.2f, 1f),
            new Color(0.5f, 0f, 1f),
            new Color(0.35f, 0f, 1f)
        };

        public Color CurrentColor => Color.Lerp(Colors[prevColorIndex], Colors[curColorIndex], curColorTransition);

        private int TransitionDurationTicks => TransitionDurationTicks_NotPermanent;

        public override int TransitionTicks => 200;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref curColorIndex, "curColorIndex", 0);
            Scribe_Values.Look(ref prevColorIndex, "prevColorIndex", 0);
            Scribe_Values.Look(ref curColorTransition, "curColorTransition", 0f);
            Scribe_Values.Look(ref longTickCounter, "longTickCounter");
        }

        public override void Init()
        {
            base.Init();
            curColorIndex = Rand.Range(0, Colors.Length);
            prevColorIndex = curColorIndex;
            curColorTransition = 1f;
        }


        public override float SkyTargetLerpFactor(Map map)
        {
            return GameConditionUtility.LerpInOutValue(this, TransitionTicks);
        }

        public override SkyTarget? SkyTarget(Map map)
        {
            Color currentColor = CurrentColor;
            SkyColorSet colorSet = new SkyColorSet(Color.Lerp(Color.white, currentColor, SkyColorStrength), new Color(0.92f, 0.92f, 0.92f), Color.Lerp(Color.white, currentColor, OverlayColorStrength), 1f);
            return new SkyTarget(0f, colorSet, 1f, 1f);
        }



        public override void GameConditionDraw(Map map)
        {
            if (!HiddenByOtherCondition(map))
            {
                gravAnomalyOverlay.DrawOverlay(map);
            }
        }

        public override void GameConditionTick()
        {
            curColorTransition += 1f / (float)TransitionDurationTicks;
            if (curColorTransition >= 1f)
            {
                prevColorIndex = curColorIndex;
                curColorIndex = GetNewColorIndex();
                curColorTransition = 0f;
            }
            foreach (Map affectedMap in base.AffectedMaps)
            {
                if (!HiddenByOtherCondition(affectedMap))
                {
                    gravAnomalyOverlay.TickOverlay(affectedMap, 1f);
                }
            }
            longTickCounter++;
            if (longTickCounter >= 2000)
            {
                if (World_ExposeData_Patch.currentGravtechProject != null)
                {
                    Find.ResearchManager.AddProgress(World_ExposeData_Patch.currentGravtechProject, 1);
                }
                longTickCounter = 0;
            }
            
        }

        private int GetNewColorIndex()
        {
            return (from x in Enumerable.Range(0, Colors.Length)
                    where x != curColorIndex
                    select x).RandomElement();
        }

        public override List<SkyOverlay> SkyOverlays(Map map)
        {
            return overlays;
        }
    }
}