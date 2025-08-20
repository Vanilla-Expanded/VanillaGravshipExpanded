using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace VanillaGravshipExpanded
{
    [HarmonyPatch]
    public static class CastSourceReplacer
    {
        [HarmonyTargetMethods]
        public static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(Verb_LaunchProjectile), "TryCastShot");
            yield return AccessTools.Method(typeof(GenDraw), nameof(GenDraw.DrawAimPie));
        }
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return instructions.MethodReplacer(
                AccessTools.PropertyGetter(typeof(Thing), nameof(Thing.DrawPos)),
                AccessTools.Method(typeof(Building_GravshipTurret), nameof(Building_GravshipTurret.GetCastSource))
            );
        }
    }
}
