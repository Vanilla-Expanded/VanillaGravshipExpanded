using HarmonyLib;
using Mono.Cecil.Cil;
using RimWorld;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using RimWorld.Planet;
using Verse.AI;



namespace VanillaGravshipExpanded
{
    [HarmonyPatch(typeof(WorldComponent_LocationGenerator), "GenerateUntilTarget")]
    [HarmonyPatch(new Type[] { typeof(PlanetLayer) })]
    public static class VanillaGravshipExpanded_WorldComponent_LocationGenerator_GenerateUntilTarget_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var codes = codeInstructions.ToList();
            var multiply = AccessTools.Method(typeof(VanillaGravshipExpanded_WorldComponent_LocationGenerator_GenerateUntilTarget_Patch), "GetMultiplier");

            for (var i = 0; i < codes.Count; i++)
            {

                if (codes[i].opcode == OpCodes.Mul)
                {

                    yield return new CodeInstruction(OpCodes.Call, multiply);

                }
                else yield return codes[i];
            }
        }


        public static float GetMultiplier(float generatedLocationFactor, float worldLocationsTarget)
        {

            return generatedLocationFactor * worldLocationsTarget * GravshipsMod_Settings.orbitalObjectsMultiplier;
        }





    }



}