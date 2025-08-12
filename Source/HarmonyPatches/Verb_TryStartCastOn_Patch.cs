using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded
{
    [HarmonyPatch(typeof(Verb), "TryStartCastOn", new[] { typeof(LocalTargetInfo), typeof(LocalTargetInfo), typeof(bool), typeof(bool), typeof(bool), typeof(bool) })]
    public static class Verb_TryStartCastOn_Patch
    {
        public static void Prefix(Verb __instance, ref float __state)
        {
            __state = __instance.verbProps.warmupTime;
            if (__instance.caster is Building_GravshipTurret building_GravshipTurret && building_GravshipTurret.MannedByPlayer)
            {
                var gravshipTargeting = building_GravshipTurret.ManningPawn.GetStatValue(VGEDefOf.VGE_GravshipTargeting);
                float alpha = 1.2f;
                float multiplier = Mathf.Clamp(Mathf.Pow(gravshipTargeting, -alpha), 0.1f, 2.0f);
                __instance.verbProps.warmupTime *= multiplier;
            }
        }

        public static void Postfix(Verb __instance, float __state)
        {
            __instance.verbProps.warmupTime = __state;
        }
    }
}
