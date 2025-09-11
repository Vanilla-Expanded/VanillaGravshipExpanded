using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
namespace VanillaGravshipExpanded
{
    public class GameCondition_Comet : GameCondition
    {
        private int curColorIndex = -1;

        private int prevColorIndex = -1;

        private float curColorTransition;

        public const float MaxSunGlow = 0.5f;

        private const float Glow = 0.5f;

        private const float SkyColorStrength = 0.15f;

        private const float OverlayColorStrength = 0.055f;

        private const float BaseBrightness = 0.73f;

        private const int TransitionDurationTicks_NotPermanent = 280;

        private const int TransitionDurationTicks_Permanent = 3750;

        private static readonly Color[] Colors = new Color[8]
        {
        new Color(0f, 1f, 0f),
        new Color(0.3f, 1f, 0f),
        new Color(0f, 1f, 0.7f),
        new Color(0.3f, 1f, 0.7f),
        new Color(0f, 0.5f, 1f),
        new Color(0f, 0f, 1f),
        new Color(0.87f, 0f, 1f),
        new Color(0.75f, 0f, 1f)
        };

        public Color CurrentColor => Color.Lerp(Colors[prevColorIndex], Colors[curColorIndex], curColorTransition);

        private int TransitionDurationTicks
        {
            get
            {
                if (!base.Permanent)
                {
                    return TransitionDurationTicks_NotPermanent;
                }
                return TransitionDurationTicks_Permanent;
            }
        }

        private bool BrightInAllMaps
        {
            get
            {
                List<Map> maps = Find.Maps;
                for (int i = 0; i < maps.Count; i++)
                {
                    if (GenCelestial.CurCelestialSunGlow(maps[i]) <= MaxSunGlow)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public bool AlwaysDarkInAllMaps
        {
            get
            {
                List<Map> maps = Find.Maps;
                for (int i = 0; i < maps.Count; i++)
                {
                    if (!maps[i].GameConditionManager.IsAlwaysDarkOutside)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public override int TransitionTicks => 200;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref curColorIndex, "curColorIndex", 0);
            Scribe_Values.Look(ref prevColorIndex, "prevColorIndex", 0);
            Scribe_Values.Look(ref curColorTransition, "curColorTransition", 0f);
        }

        public override void Init()
        {
            base.Init();
            curColorIndex = Rand.Range(0, Colors.Length);
            prevColorIndex = curColorIndex;
            curColorTransition = 1f;
        }

        public override float SkyGazeChanceFactor(Map map)
        {
            if (map.GameConditionManager.IsAlwaysDarkOutside)
            {
                return 0f;
            }
            return 8f;
        }

        public override float SkyGazeJoyGainFactor(Map map)
        {
            if (map.GameConditionManager.IsAlwaysDarkOutside)
            {
                return 0f;
            }
            return 5f;
        }

        public override float SkyTargetLerpFactor(Map map)
        {
            if (map.GameConditionManager.IsAlwaysDarkOutside)
            {
                return 0f;
            }
            return GameConditionUtility.LerpInOutValue(this, TransitionTicks);
        }

        public override SkyTarget? SkyTarget(Map map)
        {
            if (map.GameConditionManager.IsAlwaysDarkOutside)
            {
                return null;
            }
            Color currentColor = CurrentColor;
            return new SkyTarget(colorSet: new SkyColorSet(Color.Lerp(Color.white, currentColor, SkyColorStrength) * Brightness(map), new Color(0.92f, 0.92f, 0.92f), Color.Lerp(Color.white, currentColor, OverlayColorStrength) * Brightness(map), 1f), glow: Mathf.Max(GenCelestial.CurCelestialSunGlow(map), Glow), lightsourceShineSize: 1f, lightsourceShineIntensity: 1f);
        }

        private float Brightness(Map map)
        {
            return Mathf.Max(BaseBrightness, GenCelestial.CurCelestialSunGlow(map));
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
            if (!base.Permanent && base.TicksLeft > TransitionTicks)
            {
                if (BrightInAllMaps)
                {
                    base.TicksLeft = TransitionTicks;
                }
                if (AlwaysDarkInAllMaps)
                {
                    base.TicksLeft = TransitionTicks;
                }
            }
        }

        private int GetNewColorIndex()
        {
            return (from x in Enumerable.Range(0, Colors.Length)
                    where x != curColorIndex
                    select x).RandomElement();
        }
    }
}