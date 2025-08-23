using HarmonyLib;
using PipeSystem;
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

        // Divide cost by total fuel (cached before vanilla code started lowering it) to get a ratio of fuel we'll need to set each fuel tank to
        var ratio = cost / __state;
        foreach (var comp in __instance.GravshipComponents)
        {
            if (comp.Props.providesFuel && comp.CanBeActive)
            {
                var storage = comp.parent.GetComp<CompResourceStorage>();
                storage?.DrawResource(storage.AmountStored * ratio);
            }
        }

        var heatManager = __instance.GetComp<CompHeatManager>();
        heatManager.AddHeat(cost);
        
        ApplyCooldownReduction(__instance);
    }

    private static void ApplyCooldownReduction(Building_GravEngine gravEngine)
    {
        float totalReduction = GetCooldownReduction(gravEngine);
        if (totalReduction > 0f)
        {
            int originalCooldownTicks = gravEngine.cooldownCompleteTick - GenTicks.TicksGame;
            Log.Message($"Original cooldown is {originalCooldownTicks / (float)GenDate.TicksPerDay} days");
            int reducedCooldownTicks = Mathf.RoundToInt(originalCooldownTicks * (1f - totalReduction));
            gravEngine.cooldownCompleteTick = GenTicks.TicksGame + reducedCooldownTicks;
            Log.Message($"Reduced cooldown by {totalReduction * 100f}% to {reducedCooldownTicks / (float)GenDate.TicksPerDay} days");
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
                totalReduction += heatsink.Props.cooldownReductionPercent;
            }
        }
        totalReduction = Mathf.Min(totalReduction, 0.5f);
        return totalReduction;
    }
}
