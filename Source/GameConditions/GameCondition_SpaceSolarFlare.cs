using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded
{
    public class GameCondition_SpaceSolarFlare : GameCondition
    {
        public override bool ElectricityDisabled => false;



        public override int TransitionTicks => 1000;

      

        private const float MaxSkyLerpFactor = 0.5f;

        private const float SkyGlow = 0.85f;

        private SkyColorSet OrangeColors = new SkyColorSet(new ColorInt(255, 170, 50).ToColor, new ColorInt(200, 120, 80).ToColor, new Color(0.9f, 0.5f, 0.2f), SkyGlow);

        public override float SkyTargetLerpFactor(Map map)
        {
            return GameConditionUtility.LerpInOutValue(this, TransitionTicks, MaxSkyLerpFactor);
        }

        public override SkyTarget? SkyTarget(Map map)
        {
            return new SkyTarget(0.85f, OrangeColors, 1f, 1f);
        }

        public override void DoCellSteadyEffects(IntVec3 c, Map map)
        {
            base.DoCellSteadyEffects(c, map);
            if (Rand.Chance(0.1f))
            {
                FleckMaker.ThrowHeatGlow(c, map, 2.3f);
            }
        }

    }
}