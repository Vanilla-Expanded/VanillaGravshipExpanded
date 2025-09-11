using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(Building_GravEngine), nameof(Building_GravEngine.DrawAt))]
public static class Building_GravEngine_DrawAt_Patch
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
    {
        var orbField = typeof(Building_GravEngine).DeclaredField(nameof(Building_GravEngine.OrbMat));
        var ourMethod = typeof(Building_GravEngine_DrawAt_Patch).DeclaredMethod(nameof(GetCorrectMaterial));

        var orbReplacements = 0;
        var orbDrawingFixes = 0;

        foreach (var ci in instr)
        {
            yield return ci;

            if (ci.LoadsField(orbField))
            {
                // Load "this"
                yield return CodeInstruction.LoadArgument(0);
                // Call our method to possibly replace the grav engine graphic
                yield return new CodeInstruction(OpCodes.Call, ourMethod);

                orbReplacements++;
            }
            else if (ci.opcode == OpCodes.Ldc_R4 && ci.operand is float f and 0.03658537f)
            {
                // If there were grav engines with bigger graphics this would need to be even bigger to fix the graphic overlap bug.
                // I suspect the orb overlaps with the building due to the building having addTopAltitudeBias set to true.
                ci.operand = f + 0.045f;

                orbDrawingFixes++;
            }
        }

        const int expectedOrbReplacementPatches = 1;
        const int expectedOrbDrawingFixes = 1;

        if (orbReplacements != expectedOrbReplacementPatches)
            Log.Error($"Patching GravEngine:DrawAt - unexpected amount of patches. Expected patches: {expectedOrbReplacementPatches}, actual patch amount: {orbReplacements}. Cooldown graphic for grav engines orbs may not work.");
        if (orbDrawingFixes != expectedOrbDrawingFixes)
            Log.Error($"Patching GravEngine:DrawAt - unexpected amount of patches. Expected patches: {expectedOrbDrawingFixes}, actual patch amount: {orbDrawingFixes}. Either grav engine orb overlapping the engine was fixed by Ludeon, or this fix broke.");
    }

    private static CachedMaterial GetCorrectMaterial(CachedMaterial current, Building_GravEngine instance)
    {
        if (instance.Graphic is not IGravEngineGraphic graphic)
            return current;

        return Find.TickManager.TicksGame >= instance.cooldownCompleteTick ? graphic.OrbNormalMat : graphic.OrbCooldownMat;
    }
}