using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;

namespace VanillaGravshipExpanded
{
    [StaticConstructorOnStartup]
    [HotSwappable]
    public static class DubsMintMenus_Compat_Patches
    {
        static DubsMintMenus_Compat_Patches()
        {
            if (ModsConfig.IsActive("Dubwise.DubsMintMenus"))
            {
                var harmony = new Harmony("VanillaGravshipExpanded.DubsMintMenusCompat");
                ApplyDubsMintMenusPatches(harmony);
            }
        }

        private static void ApplyDubsMintMenusPatches(Harmony harmony)
        {
            var mainTabWindowType = AccessTools.TypeByName("DubsMintMenus.MainTabWindow_MintResearch");
            if (mainTabWindowType != null)
            {
                var nextResearchProjectMethod = AccessTools.Method(mainTabWindowType, "NextResearchProject");
                if (nextResearchProjectMethod != null)
                {
                    var prefix = new HarmonyMethod(typeof(DubsMintMenus_Compat_Patches), nameof(NextResearchProject_Prefix));
                    harmony.Patch(nextResearchProjectMethod, prefix: prefix);
                }
                else Log.Error("[VGE] DubsMintMenus.MainTabWindow_MintResearch.NextResearchProject method is not found.");
                var lockedBoxMethod = AccessTools.Method(mainTabWindowType, "LockedBox");
                if (lockedBoxMethod != null)
                {
                    var postfix = new HarmonyMethod(typeof(DubsMintMenus_Compat_Patches), nameof(LockedBox_Postfix));
                    harmony.Patch(lockedBoxMethod, postfix: postfix);
                }
                else Log.Error("[VGE] DubsMintMenus.MainTabWindow_MintResearch.LockedBox method is not found.");

                var drawPanelMethod = AccessTools.Method(mainTabWindowType, "DrawPanel");
                if (drawPanelMethod != null)
                {
                    var prefix = new HarmonyMethod(typeof(DubsMintMenus_Compat_Patches), nameof(DrawPanel_Prefix));
                    var postfix = new HarmonyMethod(typeof(DubsMintMenus_Compat_Patches), nameof(DrawPanel_Postfix));
                    harmony.Patch(drawPanelMethod, prefix: prefix, postfix: postfix);
                }
                else Log.Error("[VGE] DubsMintMenus.MainTabWindow_MintResearch.DrawPanel method is not found.");

                var mysterBoxType = AccessTools.Inner(mainTabWindowType, "MysterBox");
                if (mysterBoxType != null)
                {
                    var pushToQueueMethod = AccessTools.Method(mysterBoxType, "PushToQueue");
                    if (pushToQueueMethod != null)
                    {
                        var prefix = new HarmonyMethod(typeof(DubsMintMenus_Compat_Patches), nameof(Queue_Prefix));
                        harmony.Patch(pushToQueueMethod, prefix: prefix);
                    }
                    else Log.Error("[VGE] DubsMintMenus.MainTabWindow_MintResearch.MysterBox.PushToQueue method is not found.");

                    var insertQueueMethod = AccessTools.Method(mysterBoxType, "InsertQueue");
                    if (insertQueueMethod != null)
                    {
                        var prefix = new HarmonyMethod(typeof(DubsMintMenus_Compat_Patches), nameof(Queue_Prefix));
                        harmony.Patch(insertQueueMethod, prefix: prefix);
                    }
                    else Log.Error("[VGE] DubsMintMenus.MainTabWindow_MintResearch.MysterBox.InsertQueue method is not found.");
                }
                else Log.Error("[VGE] DubsMintMenus.MainTabWindow_MintResearch.MysterBox type is not found.");
            }
            else Log.Error("[VGE] DubsMintMenus.MainTabWindow_MintResearch type is not found.");
        }

        public static bool NextResearchProject_Prefix(ResearchProjectDef SelectedResearch)
        {
            return !SelectedResearch.SetGravshipResearch(false);
        }

        public static void LockedBox_Postfix(ResearchProjectDef selectedProject, ref string __result)
        {
            if (selectedProject.IsGravshipResearch() && World_ExposeData_Patch.currentGravtechProject == selectedProject)
            {
                __result = "InProgress".Translate();
            }
        }
        public static void DrawPanel_Prefix(out ResearchProjectDef __state, ResearchProjectDef ___SelectedStep)
        {
            __state = Find.ResearchManager.currentProj;
            if (___SelectedStep != null && ___SelectedStep.IsGravshipResearch() && World_ExposeData_Patch.currentGravtechProject == ___SelectedStep)
            {
                Find.ResearchManager.currentProj = ___SelectedStep;
            }
        }
        public static void DrawPanel_Postfix(ResearchProjectDef __state, ResearchProjectDef ___SelectedStep)
        {
            bool prefixMadeChange = ___SelectedStep != null
                                     && ___SelectedStep.IsGravshipResearch()
                                     && World_ExposeData_Patch.currentGravtechProject == ___SelectedStep;
            if (prefixMadeChange && Find.ResearchManager.currentProj == ___SelectedStep)
            {
                Find.ResearchManager.currentProj = __state;
            }
        }

        public static bool Queue_Prefix(ResearchProjectDef proj)
        {
            if (proj.IsGravshipResearch())
            {
                return !proj.SetGravshipResearch();
            }
            return true;
        }
    }
}
