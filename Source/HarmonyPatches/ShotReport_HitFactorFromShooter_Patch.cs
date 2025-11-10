using HarmonyLib;
using Verse;

namespace VanillaGravshipExpanded
{
    [HarmonyPatch(typeof(ShotReport), "HitFactorFromShooter")]
    public static class ShotReport_HitFactorFromShooter_Patch
    {
        public static void Postfix(ref float __result, Thing caster, float distance, float? acc = null)
        {
            if (caster is Building_GravshipTurret turret)
            {
                __result = turret.GravshipTargeting;
            }
        }
    }

    [HarmonyPatch(typeof(ShotReport), "HitReportFor")]
    public static class ShotReport_HitReportFor_Patch
    {
        public static void Postfix(ref ShotReport __result, Thing caster, Verb verb, LocalTargetInfo target)
        {
            if (caster is Building_GravshipTurret turret)
            {
                __result.factorFromTargetSize = 1f;
            }
        }
    }
}
