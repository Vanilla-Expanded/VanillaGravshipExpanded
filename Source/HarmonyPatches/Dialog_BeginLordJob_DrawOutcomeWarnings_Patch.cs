using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded
{
    [HotSwappable]
    [HarmonyPatch(typeof(Dialog_BeginRitual), "DrawExtraRitualOutcomeDescriptions")]
    public static class Dialog_BeginRitual_DrawExtraRitualOutcomeDescriptions_Patch
    {
        public static void Prefix(Dialog_BeginRitual __instance, ref string __state)
        {
            if (__instance is Dialog_BeginGravshipLaunch)
            {
                var outcome = __instance.outcome.extraOutcomeDescriptions.First();
                __state = outcome.description;
                var engine = __instance.target.Thing.TryGetComp<CompPilotConsole>()?.engine;
                if (engine == null)
                    return;
                var cooldownReduction = Building_GravEngine_ConsumeFuel_Patch.GetCooldownReduction(engine);
                if (cooldownReduction <= 0f)
                    return;
                var info = "VGE_LaunchHeatsinkCooldownInfo".Translate(cooldownReduction.ToStringPercent());
                outcome.description += " " + info;
            }
        }
        
        public static void Postfix(Dialog_BeginRitual __instance, string __state)
        {
            if (__state.NullOrEmpty() is false)
            {
                __instance.outcome.extraOutcomeDescriptions.First().description = __state;
            }
        }
    }
}
