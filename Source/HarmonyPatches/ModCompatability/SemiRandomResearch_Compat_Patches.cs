using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace VanillaGravshipExpanded
{
    [StaticConstructorOnStartup]
    [HotSwappable]
    public static class SemiRandomResearch_Compat_Patches
    {
        static SemiRandomResearch_Compat_Patches()
        {
            if (ModsConfig.IsActive("CaptainMuscles.SemiRandomResearch.unofficial") || ModsConfig.IsActive("arodoid.semirandomprogression"))
            {
                var harmony = new Harmony("VanillaGravshipExpanded.SemiRandomResearchCompat");
                ApplyPatches(harmony);
            }
        }

        private static void ApplyPatches(Harmony harmony)
        {
            var compatibilityType = AccessTools.TypeByName("CM_Semi_Random_Research.Compatibility");
            if (compatibilityType != null)
            {
                var isAnomalyContentMethod = AccessTools.Method(compatibilityType, "IsAnomalyContent");
                if (isAnomalyContentMethod != null)
                {
                    var postfix = new HarmonyMethod(typeof(SemiRandomResearch_Compat_Patches), nameof(IsAnomalyContent_Postfix));
                    harmony.Patch(isAnomalyContentMethod, postfix: postfix);
                }
                else
                {
                    Log.Error("[VGE] CM_Semi_Random_Research.Compatibility.IsAnomalyContent method not found.");
                }
            }
            else
            {
                Log.Error("[VGE] CM_Semi_Random_Research.Compatibility type not found.");
            }

            var patchContainer = AccessTools.TypeByName("CM_Semi_Random_Research.MainTabWindow_Research_Patches+MainTabWindow_Research_DrawStartButton");
            if (patchContainer != null)
            {
                var targetMethod = AccessTools.Method(patchContainer, "Prefix");
                if (targetMethod != null)
                {
                    var prefix = new HarmonyMethod(typeof(SemiRandomResearch_Compat_Patches), nameof(DrawStartButton_Prefix_Prefix));
                    harmony.Patch(targetMethod, prefix: prefix);
                }
                else
                {
                    Log.Error("[VGE] Could not find Prefix method in CM_Semi_Random_Research.MainTabWindow_Research_Patches+MainTabWindow_Research_DrawStartButton to patch.");
                }
            }
            else
            {
                Log.Error("[VGE] Could not find CM_Semi_Random_Research.MainTabWindow_Research_Patches+MainTabWindow_Research_DrawStartButton to patch.");
            }

            var utilityType = AccessTools.TypeByName("CM_Semi_Random_Research.SemiRandomResearchUtility");
            if (utilityType != null)
            {
                var canSelectNormalResearchNowMethod = AccessTools.Method(utilityType, "CanSelectNormalResearchNow");
                if (canSelectNormalResearchNowMethod != null)
                {
                    var postfix = new HarmonyMethod(typeof(SemiRandomResearch_Compat_Patches), nameof(CanSelectNormalResearchNow_Postfix));
                    harmony.Patch(canSelectNormalResearchNowMethod, postfix: postfix);
                }
                else
                {
                    Log.Error("[VGE] CM_Semi_Random_Research.SemiRandomResearchUtility.CanSelectNormalResearchNow method not found.");
                }
            }
            else
            {
                Log.Error("[VGE] CM_Semi_Random_Research.SemiRandomResearchUtility type not found.");
            }
        }

        public static void IsAnomalyContent_Postfix(ResearchProjectDef rpd, ref bool __result)
        {
            if (rpd != null && rpd.IsGravshipResearch())
            {
                __result = true;
            }
        }

        public static bool DrawStartButton_Prefix_Prefix(ResearchTabDef __1, List<string> __0)
        {
            if (__1 != null && __1.IsGravshipResearchTab())
            {
                __0.Clear();
                return false;
            }
            return true;
        }

        public static void CanSelectNormalResearchNow_Postfix(ResearchProjectDef rpd, ref bool __result)
        {
            if (rpd != null && rpd.IsGravshipResearch())
            {
                __result = true;
            }
        }
    }
}
