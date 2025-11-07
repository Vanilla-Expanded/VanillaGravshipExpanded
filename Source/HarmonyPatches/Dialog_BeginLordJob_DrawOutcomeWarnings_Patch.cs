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
        public static void Postfix(Dialog_BeginRitual __instance, Rect viewRect, float totalQuality, ref float curY, ref float totalInfoHeight)
        {
            if (__instance is Dialog_BeginGravshipLaunch)
            {
                var engine = __instance.target.Thing.TryGetComp<CompPilotConsole>()?.engine;
                var gravshipState = Dialog_BeginRitual_ShowRitualBeginWindow_Patch.state;

                // Grav Anchor / Heatsink info
                var map = gravshipState != null ? Current.Game.FindMap(gravshipState.targetTile) : null;
                if (map != null && map.listerThings.AnyThingWithDef(ThingDefOf.GravAnchor))
                {
                    DrawInfoLine(viewRect, ref curY, ref totalInfoHeight, "VGE_LaunchGravAnchorCooldownInfo".Translate());
                }
                else
                {
                    var cooldownReduction = Building_GravEngine_ConsumeFuel_Patch.GetCooldownReduction(engine);
                    if (cooldownReduction > 0f)
                    {
                        var info = "VGE_LaunchHeatsinkCooldownInfo".Translate(cooldownReduction.ToStringPercent());
                        DrawInfoLine(viewRect, ref curY, ref totalInfoHeight, info);
                    }
                }

                // Warning about no gravtech project
                bool noGravdataSource = World_ExposeData_Patch.currentGravtechProject == null && !engine.GravshipComponents.Any(x => x.parent is Building_GravshipBlackBox);
                bool anyGravTechAvailable = DefDatabase<ResearchProjectDef>.AllDefs.Any(x => x.tab == VGEDefOf.VGE_Gravtech && x.CanStartNow);
                if (noGravdataSource && anyGravTechAvailable)
                {
                    string warningPart = "Warning".Translate().ToString().ToUpper() + ": ";
                    string messagePart = "VGE_NoGravtechProjectSelected".Translate();

                    GUI.color = ColorLibrary.RedReadable;
                    DrawInfoLine(viewRect, ref curY, ref totalInfoHeight, warningPart + messagePart);
                    GUI.color = Color.white;
                }

                // Gravdata and launch info
                if (gravshipState != null)
                {
                    Pawn researcherPawn = GravdataUtility.GetResearcher(__instance.assignments);
                    float distanceTravelled = GravshipHelper.GetDistance(engine.Map.Tile, gravshipState.targetTile);
                    List<QualityFactor> list = __instance.PopulateQualityFactors(out var qualityRange);
                    var quality = __instance.PredictedQuality(list).min;

                    if (!noGravdataSource)
                    {
                        int gravdataYield = GravdataUtility.CalculateGravdataYield(distanceTravelled, quality, engine, researcherPawn);
                        var gravdataInfo = "VGE_GravdataYieldInfo".Translate(gravdataYield);
                        DrawInfoLine(viewRect, ref curY, ref totalInfoHeight, gravdataInfo);
                    }

                    if (GravshipUtility.TryGetPathFuelCost(engine.Map.Tile, gravshipState.targetTile, out var cost, out _, fuelFactor: engine.FuelUseageFactor))
                    {
                        DrawInfoLine(viewRect, ref curY, ref totalInfoHeight, "VGE_LaunchFuelAndHeatUnitsInfo".Translate(cost));
                    }

                    float boonChance = GravshipHelper.LaunchBoonChanceFromQuality(quality);
                    var boonInfo = "VGE_LaunchBoonChanceInfo".Translate((boonChance * 100).ToString("F1"));
                    DrawInfoLine(viewRect, ref curY, ref totalInfoHeight, boonInfo);
                }
            }
        }

        private static void DrawInfoLine(Rect viewRect, ref float curY, ref float totalInfoHeight, string text)
        {
            float height = Mathf.Max(Text.CalcHeight(text, viewRect.width), Text.LineHeight);
            Widgets.Label(new Rect(viewRect.x, curY, viewRect.width, height), text);
            curY += height;
            totalInfoHeight += height;
        }
    }
}
