using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(ResearchManager), nameof(ResearchManager.ExposeData))]
public static class ResearchManager_ExposeData_Patch
{
    private static void Postfix()
    {
        if (Scribe.mode == LoadSaveMode.PostLoadInit && Find.ResearchManager.gravEngineInspected && !ResearchProjectDefOf.BasicGravtech.IsFinished)
            Find.ResearchManager.FinishProject(ResearchProjectDefOf.BasicGravtech, doCompletionLetter: false);
    }
}