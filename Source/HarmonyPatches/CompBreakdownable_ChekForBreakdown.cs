using HarmonyLib;
using RimWorld;
using Verse.AI;

namespace VanillaGravshipExpanded
{
    [HarmonyPatch(typeof(CompBreakdownable), "ChekForBreakdown")]
    public static class VanillaGravshipExpanded_CompBreakdownable_ChekForBreakdown_Patch
    {
        public static bool Prefix(CompBreakdownable __instance)
        {
            if ( StaticCollections.gravMaintainables.Contains(__instance.parent.def) ==true)
            {
                return false;
            }
            return true;
        }
    }
}
