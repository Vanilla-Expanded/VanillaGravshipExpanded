using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded
{
    [HarmonyPatch(typeof(Thing), "GetInspectString")]
    public static class Thing_GetInspectString_Patch
    {
        public static void Postfix(Thing __instance, ref string __result)
        {
            if (!__instance.def.useHitPoints)
            {
                return;
            }

            if (__instance.Map == null)
            {
                return;
            }

            var spaceComp = __instance.Map.GetComponent<MaintenanceAndDeterioration_MapComponent>();
            if (spaceComp.IsThingInSpace(__instance))
            {
                var message = "VGE_RapidlyDeterioratingInSpace".Translate();
                if (string.IsNullOrEmpty(__result))
                {
                    __result = message;
                }
                else
                {
                    __result += "\n" + message;
                }
            }
        }
    }
}
