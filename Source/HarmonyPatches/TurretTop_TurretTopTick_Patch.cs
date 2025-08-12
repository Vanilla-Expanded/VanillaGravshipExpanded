using HarmonyLib;
using RimWorld;

namespace VanillaGravshipExpanded
{
    [HarmonyPatch(typeof(TurretTop), nameof(TurretTop.TurretTopTick))]
    public static class TurretTop_TurretTopTick_Patch
    {
        public static bool Prefix(TurretTop __instance)
        {
            if (__instance.parentTurret is Building_GravshipTurret gravshipTurret)
            {
                if (gravshipTurret.linkedTerminal is null || !gravshipTurret.linkedTerminal.MannableComp.MannedNow)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
