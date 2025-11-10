using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded
{
    [HarmonyPatch(typeof(Verb_LaunchProjectile), "TryCastShot")]
    public static class Verb_LaunchProjectile_TryCastShot_Patch
    {
        private static Thing GetManningPawn(Thing manningPawn, Verb_LaunchProjectile verb, ref Thing equipmentSource)
        {
            var turret = verb.caster as Building_GravshipTurret;
            if (turret != null && turret is not Building_EnemyMechTurret)
            {
                equipmentSource = turret;
                return turret.ManningPawn;
            }
            return manningPawn;
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            var getManningPawnMethod = AccessTools.Method(typeof(Verb_LaunchProjectile_TryCastShot_Patch), "GetManningPawn");
            for (int i = 0; i < codes.Count; i++)
            {
                var instruction = codes[i];
                yield return instruction;
                if (instruction.opcode == OpCodes.Stloc_S && instruction.operand is LocalBuilder lb && lb.LocalIndex == 4)
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_3);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldloca_S, 4);
                    yield return new CodeInstruction(OpCodes.Call, getManningPawnMethod);
                    yield return new CodeInstruction(OpCodes.Stloc_3);
                }
            }
        }
    }
}
