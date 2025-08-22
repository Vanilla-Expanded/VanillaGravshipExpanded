using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded
{
    [HarmonyPatch(typeof(Building_TurretGun), nameof(Building_TurretGun.ResetForcedTarget))]
    public static class Building_TurretGun_ResetForcedTarget_Patch
    {
        public static void Prefix(Building_TurretGun __instance)
        {
            if (__instance.GetComp<CompWorldArtillery>() is CompWorldArtillery comp)
            {
                comp.Reset();
                __instance.AttackVerb.Reset();
            }
        }
    }
}
