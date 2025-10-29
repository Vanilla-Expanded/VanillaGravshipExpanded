using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded
{
    [HarmonyPatch(typeof(Building_GravEngine), nameof(Building_GravEngine.Inspect))]
    public static class Building_GravEngine_Inspect_Patch
    {
        public static void Postfix(Building_GravEngine __instance)
        {
            QuestUtility.SendQuestTargetSignals(__instance.Map.Parent.questTags, "Inspected", __instance.Named("SUBJECT"));
        }
    }
}
