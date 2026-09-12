using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
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

        Log.Error("[VGE] Error replacing Chemfuel cost with precise cost report - could not find one of the lambdas to CompPilotConsole:StartChoosingDestination.");
        return false;
    }

    private static MethodBase TargetMethod() => typeof(CompPilotConsole).FindIncludingInnerTypes<MethodBase>(t => t.FirstMethod(m => m.Name == $"<{nameof(CompPilotConsole.StartChoosingDestination_NewTemp)}>b__2"));

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr, MethodBase baseMethod)
    {
        var matcher = new CodeMatcher(instr);

        // Match one of the relevant strings (like "FuelAmount" or "Cost")
        matcher.MatchStartForward(new CodeMatch(OpCodes.Ldstr, "FuelAmount"));
        // Go to the string.Format call
        matcher.MatchEndForward(
            CodeMatch.Calls(typeof(string).DeclaredMethod(nameof(string.Format), [typeof(string), typeof(object), typeof(object)]))
        );
        matcher.Advance();
        // Log.Error($"Type: {baseMethod.DeclaringType.FullDescription()}, method: {baseMethod.FullDescription()}, field: {baseMethod.DeclaringType.DeclaredField("<>4__this")}, other field: {baseMethod.DeclaringType.DeclaredField("curTile")}");
        // Insert our wrapper around existing text
        matcher.Insert(
            // Load the console field
            CodeInstruction.LoadArgument(0),
            CodeInstruction.LoadField(baseMethod.DeclaringType, "<>4__this"),
            // Load the fuel cost field
            CodeInstruction.LoadLocal(8),
            // Load the current tile field
            // CodeInstruction.LoadArgument(0),
            // CodeInstruction.LoadField(baseMethod.DeclaringType, "curTile"),
            // Load the target tile variable
            // CodeInstruction.LoadLocal(2),
            // Call our wrapper method
            CodeInstruction.Call(() => GetCostsReplacement)
        );

        var replacements = 0;
        while (true)
        {
            // We want to replace the 3 errors that happen after the code we replaced.
            // We don't want to replace the single occurrence beforehand
            matcher.MatchStartForward(new CodeMatch(OpCodes.Ldstr, " ({0})"));
            if (matcher.IsInvalid)
                break;

            matcher.Operand = "\n({0})";
            if (++replacements >= 10)
            {
                Log.Error("Too many attempt at patching CompPilotConsole:StartChoosingDestination lambda");
                break;
            }
        }

        const int expectedPatches = 3;

        if (replacements != expectedPatches)
            Log.Error($"Patching CompPilotConsole:StartChoosingDestination lambda - unexpected amount of patches. Expected patches: {expectedPatches}, actual patch amount: {replacements}. Formatting may be incorrect while targeting on world map.");

        return matcher.Instructions();
    }

    private static string GetCostsReplacement(string current, CompPilotConsole console, float cost)
    {
        return GravshipFuelProviderUtility.GetFuelConsumptionReport(console.engine, cost / console.engine.TotalFuel, startingText: $"{"Cost".Translate().CapitalizeFirst()}:");
    }
}