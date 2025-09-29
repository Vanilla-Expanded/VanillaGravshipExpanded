using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(GravshipUtility), nameof(GravshipUtility.TryGetPathFuelCost))]
public static class GravshipUtility_TryGetPathFuelCost_Patch
{
    private static void Prefix(ref float fuelPerTile)
    {
        // We're changing the cost from 10 chemfuel to 5 astrofuel (which, by default, costs 10 chemfuel).
        // The method takes 10 as the default value, and never specifies any other cost. So the simplest
        // way to handle this is just cut all the costs in half. We don't bother considering the fuel
        // savings factor in this, since that is a separate argument for this method (as opposed to
        // Building_GravEngine:FuelPerTile getter, which includes the savings already).
        fuelPerTile /= 2f;
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
    {
        var replacements = 0;

        foreach (var ci in instr)
        {
            // Drop the minimum cost of the gravship launch from 50 to 25, since we're cutting everything else in half.
            if (ci.opcode == OpCodes.Ldc_R4 && ci.operand is 50f)
            {
                ci.operand = 25f;
                replacements++;
            }

            yield return ci;
        }

        const int expectedGravshipRangeCalls = 1;

        if (replacements != expectedGravshipRangeCalls)
            Log.Error($"Patching GravshipUtility:TryGetPathFuelCost - unexpected amount of patches. Expected patches: {expectedGravshipRangeCalls}, actual patch amount: {replacements}. Gravship launch cost may be incorrect/broken.");
    }
}