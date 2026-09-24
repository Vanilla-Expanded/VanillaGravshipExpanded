using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(Building_TurretFoam), nameof(Building_TurretFoam.TryFindNewTarget))]
public static class Building_TurretFoam_Patch
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr, ILGenerator generator)
    {
        var matcher = new CodeMatcher(instr, generator);

        CompFirefoamPack_ChanceToUse_Patch.InsertAstrofireToCondition(matcher);

        return matcher.Instructions();
    }
}