using HarmonyLib;
using Mono.Cecil.Cil;
using RimWorld;
using RimWorld.QuestGen;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using RimWorld.Planet;
using Verse.AI;



namespace VanillaGravshipExpanded
{
    [HarmonyPatch(typeof(MapDrawLayer_OrbitalDebris), "Regenerate")]

    public static class VanillaGravshipExpanded_MapDrawLayer_OrbitalDebris_Regenerate_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var codes = codeInstructions.ToList();
            var multiply = AccessTools.Method(typeof(VanillaGravshipExpanded_MapDrawLayer_OrbitalDebris_Regenerate_Patch), "GetMultiplier");

            for (var i = 0; i < codes.Count; i++)
            {

                if (codes[i].opcode == OpCodes.Stloc_S && codes[i].operand is LocalBuilder local && local.LocalIndex == 7)
                {
                    yield return new CodeInstruction(OpCodes.Stloc_S,7);
                    yield return new CodeInstruction(OpCodes.Ldloc_S,7);
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Call, multiply);
                    yield return new CodeInstruction(OpCodes.Stloc_S, 7);

                }
                else yield return codes[i];
            }
        }


        public static int GetMultiplier(int num, OrbitalDebrisDef debris)
        {
            OrbitalDebrisExtension extension = debris.GetModExtension<OrbitalDebrisExtension>();
            if (extension != null)
            {
                return (int)(num * extension.orbitalDebrisClusterMultiplier);
            }
            else return num;
                
        }





    }



}