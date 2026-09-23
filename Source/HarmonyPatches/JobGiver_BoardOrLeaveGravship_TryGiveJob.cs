using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(JobGiver_BoardOrLeaveGravship), nameof(JobGiver_BoardOrLeaveGravship.TryGiveJob))]
public class JobGiver_BoardOrLeaveGravship_TryGiveJob_Patch
{
    private static readonly List<Thing> TmpList = [];

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr, MethodBase baseMethod)
        => ListAllGravEngines_Patch.ReplaceListerThings(instr, baseMethod, typeof(JobGiver_BoardOrLeaveGravship_TryGiveJob_Patch).DeclaredMethod(nameof(ReturnSingleGravEngine)), 1);

    private static List<Thing> ReturnSingleGravEngine(ListerThings lister, ThingDef def)
    {
        if (def != ThingDefOf.GravEngine)
            return lister.ThingsOfDef(def);

        TmpList.Clear();
        var engine = GravEngineTracker.GetPlayerGravEngine();
        if (engine?.Map?.listerThings == lister)
            TmpList.Add(engine);
        return TmpList;
    }
}