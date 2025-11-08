using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded
{
    [HotSwappable]
    [HarmonyPatch(typeof(Building_TurretGun), "TryFindNewTarget")]
    public static class Building_TurretGun_TryFindNewTarget_Patch
    {
        public static bool Prefix(Building_TurretGun __instance, ref LocalTargetInfo __result)
        {
            if (__instance is Building_GravshipTurret turret && turret.CanAutoAttack is false)
            {
                __result = LocalTargetInfo.Invalid;
                return false;
            }
            return true;
        }
    }
}
