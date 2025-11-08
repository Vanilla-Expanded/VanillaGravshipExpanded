using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace VanillaGravshipExpanded
{
    [HotSwappable]
    [HarmonyPatch(typeof(Building_TurretGun), "TryStartShootSomething")]
    public static class Building_TurretGun_TryStartShootSomething_Patch
    {
        public static bool Prefix(Building_TurretGun __instance)
        {
            if (__instance.def == VGEDefOf.VGE_PointDefenseTurret)
            {
                return false;
            }
            return true;
        }

        public static void Postfix(Building_TurretGun __instance)
        {
            if (__instance.CurrentTarget.IsValid)
            {
                if (__instance is Building_GravshipTurret building_GravshipTurret)
                {
                    var gravshipTargeting = building_GravshipTurret.GravshipTargeting;
                    float alpha = 1.2f;
                    float multiplier = Mathf.Clamp(Mathf.Pow(gravshipTargeting, -alpha), 0.1f, 2.0f);
                    var warmupTime = building_GravshipTurret.def.building.turretBurstWarmupTime * multiplier;
                    building_GravshipTurret.burstWarmupTicksLeft = warmupTime.RandomInRange.SecondsToTicks();
                    VGEDefOf.VGE_GravshipTarget_Acquired.PlayOneShot(new TargetInfo(__instance.Position, __instance.Map));
                }
            }
        }
    }
}
