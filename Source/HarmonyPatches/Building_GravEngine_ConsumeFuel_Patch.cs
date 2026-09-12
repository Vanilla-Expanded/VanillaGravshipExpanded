using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded;

[HotSwappable]
[HarmonyPatch(typeof(Building_GravEngine), nameof(Building_GravEngine.ConsumeFuel))]
public static class Building_GravEngine_ConsumeFuel_Patch
{
    private static void Prefix(Building_GravEngine __instance, ref float __state) => __state = __instance.TotalFuel;

    private static void Postfix(Building_GravEngine __instance, PlanetTile tile, float __state)
    {
        // Grab cached values
        if (!GravshipUtility.TryGetPathFuelCost(__instance.Map.Tile, tile, out var cost, out _))
            return;

        OverrideFuelCostIfGravlift(__instance, ref cost);

        var extendedInfo = __instance.launchInfo.ExtendedInfo(true);
        // Divide cost by total fuel (cached before vanilla code started lowering it) to get a ratio of fuel we'll need to set each fuel tank to
        extendedInfo.lastCost = cost;
        var ratio = cost / __state;

        // Create the fuel spent data directly
        extendedInfo.fuelSpentPerTank ??= new FuelSpentData();
        GravshipFuelProviderUtility.ConsumeFuelRatioForAllProviders(__instance, ratio, extendedInfo.fuelSpentPerTank);

        var heatManager = __instance.GetComp<CompHeatManager>();
        heatManager.AddHeat(heatManager.HeatGeneratedFromFuel(cost));
        ApplyCooldownReduction(__instance);
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
    {
        var matcher = new CodeMatcher(instr);

        // Find the first return statement and insert after (let it return early if it can't find path)
        matcher.MatchEndForward(new CodeMatch(OpCodes.Ret));
        if (matcher.IsInvalid)
            Log.Error("[VGE] Failed patching Building_GravEngine.ConsumeFuel - gravlift launch may consume too much fuel when using Odyssey-style fuel tanks.");

        // Advance forward 1 so we can operate on the first instruction after return
        matcher.Advance();
        matcher.Insert(
            // Load "this" (Building_GravEngine)
            CodeInstruction.LoadArgument(0).MoveLabelsFrom(matcher.Instruction),
            // Load the first local (fuel cost) by address
            CodeInstruction.LoadLocal(0, true),
            // Call our method to override the fuel if it's gravlift launch
            CodeInstruction.Call(() => OverrideFuelCostIfGravlift)
        );

        return matcher.InstructionEnumeration();
    }

    private static void ApplyCooldownReduction(Building_GravEngine gravEngine)
    {
        float totalReduction = GetCooldownReduction(gravEngine);
        if (totalReduction > 0f)
        {
            int originalCooldownTicks = gravEngine.cooldownCompleteTick - GenTicks.TicksGame;
            int reducedCooldownTicks = Mathf.RoundToInt(originalCooldownTicks * (1f - totalReduction));
            gravEngine.cooldownCompleteTick = GenTicks.TicksGame + reducedCooldownTicks;
        }
    }

    public static float GetCooldownReduction(Building_GravEngine gravEngine)
    {
        float totalReduction = 0f;
        foreach (var comp in gravEngine.GravshipComponents)
        {
            var heatsink = comp.parent.GetComp<CompHeatsink>();
            if (heatsink != null && heatsink.IsActive)
            {
                totalReduction += heatsink.CachedStats.cooldownReductionPercent;
            }
        }
        totalReduction = Mathf.Min(totalReduction, 0.5f);
        return totalReduction;
    }

    private static void OverrideFuelCostIfGravlift(Building_GravEngine engine, ref float fuel)
    {
        if (engine.launchInfo.ExtendedInfo(false)?.isGravliftLaunch == true)
            fuel = 20f;
    }
}