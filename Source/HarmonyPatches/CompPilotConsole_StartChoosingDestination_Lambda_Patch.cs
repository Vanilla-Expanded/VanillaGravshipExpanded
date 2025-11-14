using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch]
public static class CompPilotConsole_StartChoosingDestination_Lambda_Patch
{
    private static bool Prepare(MethodBase method)
    {
        if (method != null)
            return true;
        if (TargetMethod() != null)
            return true;

        Log.Error("[VGE] Error replacing Chemfuel with Astrofuel text - could not find one of the lambdas to CompPilotConsole:StartChoosingDestination.");
        return false;
    }

    private static MethodBase TargetMethod() => typeof(CompPilotConsole).FindIncludingInnerTypes<MethodBase>(t => t.FirstMethod(m => m.Name == $"<{nameof(CompPilotConsole.StartChoosingDestination_NewTemp)}>b__2"));

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
    {
        var targetField = typeof(ThingDefOf).DeclaredField(nameof(ThingDefOf.Chemfuel));
        var replacementField = typeof(VGEDefOf).DeclaredField(nameof(VGEDefOf.VGE_Astrofuel));

        var replacements = 0;

        foreach (var ci in instr)
        {
            if (ci.LoadsField(targetField))
            {
                ci.operand = replacementField;
                replacements++;
            }

            yield return ci;
        }

        const int expectedReplacements = 1;
        if (replacements != expectedReplacements)
            Log.Error($"[VGE] Patching CompPilotConsole:StartChoosingDestination failed - unexpected amount of patches. Expected patches: {expectedReplacements}, actual patch amount: {replacements}. Selecting gravship destination may incorrectly refer to astrofuel as chemfuel.");
    }
}