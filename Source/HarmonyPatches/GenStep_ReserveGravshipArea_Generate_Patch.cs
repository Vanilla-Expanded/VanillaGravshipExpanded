using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection.Emit;
using Verse;

namespace VanillaGravshipExpanded
{
    [HarmonyPatch(typeof(GenStep_ReserveGravshipArea), "Generate")]
    public static class GenStep_ReserveGravshipArea_Generate_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var codes = new List<CodeInstruction>(instructions);
            var targetMethod = AccessTools.Method(typeof(GravshipPlacementUtility), nameof(GravshipPlacementUtility.GetCellsAdjacentToSubstructure));
            var clampMethod = AccessTools.Method(typeof(GravshipMapGenUtility), nameof(GravshipMapGenUtility.ClampCellsToRadius));
            var gravshipField = AccessTools.Field(typeof(GenStepParams), nameof(GenStepParams.gravship));

            for (int i = 0; i < codes.Count; i++)
            {
                yield return codes[i];
                if (codes[i].Calls(targetMethod))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_2);
                    yield return new CodeInstruction(OpCodes.Ldfld, gravshipField);
                    yield return new CodeInstruction(OpCodes.Call, clampMethod);
                }
            }
        }
    }
}