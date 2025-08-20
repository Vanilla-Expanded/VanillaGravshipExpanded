using HarmonyLib;
using RimWorld;
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
                if (__instance is Building_GravshipTurret)
                {
                    VGEDefOf.VGE_GravshipTarget_Acquired.PlayOneShot(new TargetInfo(__instance.Position, __instance.Map));
                }
            }
        }
    }
}
