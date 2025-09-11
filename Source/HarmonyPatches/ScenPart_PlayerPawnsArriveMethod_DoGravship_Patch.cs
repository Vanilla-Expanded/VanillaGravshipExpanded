using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using PipeSystem;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(ScenPart_PlayerPawnsArriveMethod), nameof(ScenPart_PlayerPawnsArriveMethod.DoGravship))]
public static class ScenPart_PlayerPawnsArriveMethod_DoGravship_Patch
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var targetLoc = -1;
        var patchCount = 0;

        foreach (var instr in instructions)
        {
            // Will catch the local index changing, but not if it's 0 through 3 (as those don't have an operand)
            if (instr.IsStloc() && instr.operand is LocalBuilder builder && builder.LocalType == typeof(List<Thing>))
                targetLoc = builder.LocalIndex;
            // Insert before the end
            else if (instr.opcode == OpCodes.Ret)
            {
                if (targetLoc < 0)
                    throw new Exception("Patching gravship start failed - could not find list of spawned buildings. Gravships will start with empty fuel tanks.");

                // Load the list of generated gravship buildings. Also move labels from return instruction, so we don't skip over our inserted code.
                yield return CodeInstruction.LoadLocal(targetLoc).MoveLabelsFrom(instr);
                // Call our method with the list of generated buildings.
                yield return CodeInstruction.Call(typeof(ScenPart_PlayerPawnsArriveMethod_DoGravship_Patch), nameof(RefillStorageTanks));
                patchCount++;
            }

            yield return instr;
        }

        const int expectedPatches = 1;
        if (patchCount != expectedPatches)
            Log.Error($"Patching gravship start failed - unexpected amount of patches. Expected patches: {expectedPatches}, actual patch amount: {patchCount}. There may be some issues.");
    }

    private static void RefillStorageTanks(List<Thing> things)
    {
        foreach (var thing in things)
        {
            var comp = thing.TryGetComp<CompResourceStorage>();
            comp?.AddResource(comp.Props.storageCapacity);
        }
    }
}