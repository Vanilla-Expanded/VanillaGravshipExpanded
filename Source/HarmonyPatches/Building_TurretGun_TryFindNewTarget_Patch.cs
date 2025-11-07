using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded
{
    [HarmonyPatch(typeof(Building_TurretGun), "TryFindNewTarget")]
    public static class Building_TurretGun_TryFindNewTarget_Patch
    {
        public static bool Prefix(Building_TurretGun __instance, ref LocalTargetInfo __result)
        {
            if (__instance is Building_GravshipTurret and not Building_EnemyMechTurret and not Building_GravshipTurretAutoAttack and not Building_AnticraftCaster)
            {
                __result = LocalTargetInfo.Invalid;
                return false;
            }
            return true;
        }
    }
}
