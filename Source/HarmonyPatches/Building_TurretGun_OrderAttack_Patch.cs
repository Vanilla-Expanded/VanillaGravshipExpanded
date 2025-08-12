using HarmonyLib;
using RimWorld;

namespace VanillaGravshipExpanded
{
    [HarmonyPatch(typeof(Building_TurretGun), nameof(Building_TurretGun.OrderAttack))]
    public static class Building_TurretGun_OrderAttack_Patch
    {
        public static void Prefix(Building_TurretGun __instance)
        {
            if (__instance.GetComp<CompWorldArtillery>() is CompWorldArtillery comp)
            {
                comp.Reset();
            }
        }
    }
}
