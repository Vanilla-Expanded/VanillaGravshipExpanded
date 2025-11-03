using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded
{
    [HarmonyPatch(typeof(Building_GravEngine), nameof(Building_GravEngine.Inspect))]
    public static class Building_GravEngine_Inspect_Patch
    {
        public static void Prefix(out bool __state)
        {
            __state = Find.ResearchManager.gravEngineInspected;
        }

        public static void Postfix(Building_GravEngine __instance, bool __state)
        {
            // Sanity check against re-inspecting a grav engine a second time
            if (__state)
                return;

            QuestUtility.SendQuestTargetSignals(__instance.Map.Parent.questTags, "Inspected", __instance.Named("SUBJECT"));
            Find.ResearchManager.FinishProject(ResearchProjectDefOf.BasicGravtech);
        }
    }
}
