using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(JobDriver_PilotConsole), nameof(JobDriver_PilotConsole.MakeNewToils))]
public static class JobDriver_PilotConsole_MakeNewToils_Patch
{
    private static IEnumerable<Toil> Postfix(IEnumerable<Toil> toils, JobDriver_PilotConsole __instance)
    {
        var replacedToils = 0;

        foreach (var toil in toils)
        {
            if (toil.debugName == nameof(JobDriver_PilotConsole.MakeNewToils) && toil.initAction != null)
            {
                var originalAction = toil.initAction;
                toil.initAction = () =>
                {
                    var thing = __instance.job.GetTarget(JobDriver_PilotConsole.ConsoleInd).Thing;
                    if (thing == null)
                        return;

                    if (thing.def == VGEDefOf.VGE_PilotCockpit)
                        ((Precept_Ritual)__instance.pawn.Ideo.GetPrecept(VGEDefOf.VGE_GravjumperLaunch)).ShowRitualBeginWindow(thing, selectedPawn: __instance.pawn);
                    else if (thing.def == VGEDefOf.VGE_PilotBridge)
                        ((Precept_Ritual)__instance.pawn.Ideo.GetPrecept(VGEDefOf.VGE_GravhulkLaunch)).ShowRitualBeginWindow(thing, selectedPawn: __instance.pawn);
                    else
                        originalAction();
                };

                replacedToils++;
                if (replacedToils == 2)
                    Log.Error("[VGE] Replacing target ritual in JobDriver_PilotConsole:MakeNewToils failed, replaced too many toils. Possible mod conflict? Using the right-click float menu to start the ritual may be broken, please use the gizmos instead.");
            }

            yield return toil;
        }
    }
}