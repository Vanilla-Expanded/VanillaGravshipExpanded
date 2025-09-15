using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
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
                var gravshipState = Dialog_BeginRitual_ShowRitualBeginWindow_Patch.state;
                var cooldownReduction = Building_GravEngine_ConsumeFuel_Patch.GetCooldownReduction(engine);
                if (cooldownReduction > 0f)
                {
                    var info = "VGE_LaunchHeatsinkCooldownInfo".Translate(cooldownReduction.ToStringPercent());
                    outcome.description += " " + info;
                }
                if (World_ExposeData_Patch.currentGravtechProject == null && engine.GravshipComponents.Any(x => x.parent is Building_GravshipBlackBox) is false)
                {
                    var warningPart = "Warning".Translate().ToString().ToUpper();
                    var messagePart = "VGE_NoGravtechProjectSelected".Translate();
                    var coloredWarning = $"<color=red>{warningPart}:</color> {messagePart}";
                    outcome.description += "\n\n" + coloredWarning + "\n";
                }
                else if (gravshipState != null)
                {
                    Pawn researcherPawn = __instance.assignments.AssignedPawns("gravtechResearcher").FirstOrDefault();
                    float distanceTravelled = GravshipHelper.GetDistance(engine.Map.Tile, gravshipState.targetTile);
                    List<QualityFactor> list = __instance.PopulateQualityFactors(out var qualityRange);
                    var quality = __instance.PredictedQuality(list).min;
                    int gravdataYield = GravdataUtility.CalculateGravdataYield(distanceTravelled, quality, engine, researcherPawn);
                    var gravdataInfo = "VGE_GravdataYieldInfo".Translate(gravdataYield);
                    outcome.description += " " + gravdataInfo;
                    
                    if (GravshipUtility.TryGetPathFuelCost(engine.Map.Tile, gravshipState.targetTile, out var cost, out _))
                    {
                        outcome.description += "\n\n" + "VGE_LaunchHeatUnitsInfo".Translate(cost);
                    }
                    
                    outcome.description += "\n\n" + "DEV: " + $"Distance: {distanceTravelled}, Quality: {quality}, Researcher: {researcherPawn?.Name}, ResearchStat: {researcherPawn?.GetStatValue(VGEDefOf.VGE_GravshipResearch)}, YieldMultiplier: {GravdataUtility.CalculateYieldMultiplier(engine)}, Cost: {cost}\n";
                    
                    float boonChance = GravshipHelper.LaunchBoonChanceFromQuality(quality);
                    var boonInfo = "VGE_LaunchBoonChanceInfo".Translate((boonChance * 100).ToString("F1"));
                    outcome.description += "\n\n" + boonInfo;
                }
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
