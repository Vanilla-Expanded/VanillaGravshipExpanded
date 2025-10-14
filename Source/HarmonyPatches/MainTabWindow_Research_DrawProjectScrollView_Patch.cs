using HarmonyLib;
using RimWorld;
using Verse;
using UnityEngine;

namespace VanillaGravshipExpanded
{
    [HarmonyPatch(typeof(MainTabWindow_Research), nameof(MainTabWindow_Research.DrawProjectScrollView))]
    public static class MainTabWindow_Research_DrawProjectScrollView_Patch
    {
        public static void Prefix(MainTabWindow_Research __instance, out TechLevel __state)
        {
            __state = __instance.selectedProject.techLevel;
            if (__instance.selectedProject.tab == VGEDefOf.VGE_Gravtech)
            {
                __instance.selectedProject.techLevel = Faction.OfPlayer.def.techLevel;
            }
        }
        
        public static void Postfix(MainTabWindow_Research __instance, TechLevel __state)
        {
            __instance.selectedProject.techLevel = __state;
        }
    }
}
