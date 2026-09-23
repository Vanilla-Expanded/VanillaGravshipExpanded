using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch]
public static class ListAllGravEngines_Patch
{
    private static readonly List<Thing> TmpList = [];

    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return typeof(SubstructureGrid).DeclaredMethod(nameof(SubstructureGrid.DrawSubstructureCountOnGUI));
        yield return typeof(SubstructureGrid).DeclaredMethod(nameof(SubstructureGrid.DrawSubstructureFootprint));

        var method = typeof(FormCaravanComp).FindIncludingInnerTypes<MethodBase>(t => t.FirstMethod(m => m.Name == "<GetGizmos>b__0"));
        if (method != null)
            yield return method;
        else
            Log.Error("[VGE] Error adding confirmation to leaving gravjumper/gravhulk engines behind - could not find one of the lambdas to FormCaravanComp:GetGizmos.");
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr, MethodBase baseMethod)
        => ReplaceListerThings(instr, baseMethod, typeof(ListAllGravEngines_Patch).DeclaredMethod(nameof(ReturnAllGravEngines)), 1);

    public static IEnumerable<CodeInstruction> ReplaceListerThings(IEnumerable<CodeInstruction> instr, MethodBase baseMethod, MethodBase listerThingsMethodReplacement, int expectedPatches)
    {
        var gravEngineField = typeof(ThingDefOf).DeclaredField(nameof(ThingDefOf.GravEngine));
        var listerThingsMethodTarget = typeof(ListerThings).DeclaredMethod(nameof(ListerThings.ThingsOfDef));

        var isGravEngineField = false;
        var replacedThingsOfDefCalls = 0;

        foreach (var ci in instr)
        {
            if (ci.LoadsField(gravEngineField))
            {
                isGravEngineField = true;
            }
            else if (isGravEngineField)
            {
                isGravEngineField = false;

                if (ci.Calls(listerThingsMethodTarget))
                {
                    // Replace the vanilla method call with our own
                    ci.opcode = OpCodes.Call;
                    ci.operand = listerThingsMethodReplacement;

                    replacedThingsOfDefCalls++;
                }
            }

            yield return ci;
        }

        if (replacedThingsOfDefCalls != expectedPatches)
            Log.Error($"Patching {baseMethod.DeclaringType?.Name}:{baseMethod.Name} - unexpected amount of patches. Expected patches: {expectedPatches}, actual patch amount: {replacedThingsOfDefCalls}. Game may fail to find custom VE grav engines.");
    }

    private static List<Thing> ReturnAllGravEngines(ListerThings lister, ThingDef def)
    {
        if (def != ThingDefOf.GravEngine)
            return lister.ThingsOfDef(def);

        TmpList.Clear();
        for (var i = 0; i < GravshipHelper.GravEngineDefs.Length; i++)
            TmpList.AddRange(lister.ThingsOfDef(GravshipHelper.GravEngineDefs[i]));

        return TmpList;
    }
}