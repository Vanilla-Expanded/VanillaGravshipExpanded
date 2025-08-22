using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded
{
    [HarmonyPatch(typeof(Verb_LaunchProjectile), "TryCastShot")]
    public static class Verb_LaunchProjectile_TryCastShot_Patch
    {
        public struct VerbState
        {
            public float warmupTime;
            public float forcedMissRadius;
        }

        public static void Prefix(Verb_LaunchProjectile __instance, out VerbState __state)
        {
            __state = new VerbState
            {
                warmupTime = __instance.verbProps.warmupTime,
                forcedMissRadius = __instance.verbProps.forcedMissRadius
            };
            
            if (__instance.caster is Building_GravshipTurret building_GravshipTurret && building_GravshipTurret.MannedByPlayer)
            {
                var gravshipTargeting = building_GravshipTurret.ManningPawn.GetStatValue(VGEDefOf.VGE_GravshipTargeting);
                float alpha = 1.2f;
                float multiplier = Mathf.Clamp(Mathf.Pow(gravshipTargeting, -alpha), 0.1f, 2.0f);

                __instance.verbProps.warmupTime *= multiplier;
            }
        }

        public static void Postfix(Verb_LaunchProjectile __instance, VerbState __state)
        {
            __instance.verbProps.warmupTime = __state.warmupTime;
            __instance.verbProps.forcedMissRadius = __state.forcedMissRadius;
        }
    }
}
