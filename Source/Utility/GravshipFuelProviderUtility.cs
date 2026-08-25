using System;
using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded;

public static class GravshipFuelProviderUtility
{
    private static readonly List<CompGravshipThruster> TmpThrustersList = [];
    private static readonly List<IGravshipFuelProvider> TmpProvidersList = [];

    private static readonly GravshipFuelConsumptionReportCache CachedFuelConsumptionReport = new();
    private static readonly GravshipFuelTabEntryCache CachedFuelTabEntry = new();

    public static void ConsumeFuelRatioForAllProviders(Building_GravEngine engine, float ratio, FuelSpentData fuelSpentData = null)
    {
        ForEachActiveProvider(engine, ConsumeFuel);

        void ConsumeFuel(IGravshipFuelProvider provider, Building_GravEngine gravEngine, List<CompGravshipThruster> thrusters, List<IGravshipFuelProvider> otherProviders)
        {
            var consumptionList = provider.ConsumeFuelRatio(gravEngine, ratio, thrusters, otherProviders, true);
            if (fuelSpentData?.fuelData != null)
            {
                foreach (var (p, amount) in consumptionList.fuelData)
                {
                    if (p.ParentThing is { } thing)
                        fuelSpentData.fuelData[thing] = amount;
                }
            }
        }
    }

    public static void RefundFuelForAllProviders(Building_GravEngine engine, float refundRatio, FuelSpentData fuelSpentData)
    {
        if (fuelSpentData == null || fuelSpentData.fuelData.NullOrEmpty())
            return;

        ForEachActiveProvider(engine, RefundFuel);

        void RefundFuel(IGravshipFuelProvider provider, Building_GravEngine gravEngine, List<CompGravshipThruster> thrusters, List<IGravshipFuelProvider> otherProviders)
        {
            if (provider.ParentThing is { } thing && fuelSpentData.fuelData.TryGetValue(thing, out var amount))
                provider.AddFuelAmount(gravEngine, amount * refundRatio);
        }
    }

    public static float CurrentRangeForAllProviders(Building_GravEngine engine)
    {
        var range = 0f;

        ForEachActiveProvider(engine, SumMaxRanges);

        return range;

        void SumMaxRanges(IGravshipFuelProvider provider, Building_GravEngine gravEngine, List<CompGravshipThruster> thrusters, List<IGravshipFuelProvider> otherProviders)
        {
            range += provider.CurrentRangeProvidedByFuel(gravEngine, thrusters, otherProviders);
        }
    }

    public static float MaxRangeForAllProviders(Building_GravEngine engine)
    {
        var range = 0f;

        ForEachActiveProvider(engine, SumMaxRanges);

        return range;

        void SumMaxRanges(IGravshipFuelProvider provider, Building_GravEngine gravEngine, List<CompGravshipThruster> thrusters, List<IGravshipFuelProvider> otherProviders)
        {
            range += provider.MaxRangeProvidedByFuel(gravEngine, thrusters, otherProviders);
        }
    }

    public static StringBuilder GetFuelConsumptionReport(Building_GravEngine engine, float fuelCostRatio, int maxEntries = int.MaxValue, string startingText = null, string extraTextAtMaxEntries = null)
    {
        var builder = CachedFuelConsumptionReport.GetCachedIfValid(engine, fuelCostRatio, maxEntries, startingText, extraTextAtMaxEntries, 0);
        if (builder != null)
            return builder;

        var list = new List<FuelUsageData>();
    
        builder = new StringBuilder(startingText);
        var entries = 0;
        var otherFuel = 0f;

        ForEachActiveProvider(engine, GetFuelConsumptionReports, HandleRefuelables);

        list.SortByDescending(x => x);

        foreach (var data in list)
        {
            if (entries >= maxEntries)
            {
                if (!extraTextAtMaxEntries.NullOrEmpty())
                    builder.AppendInNewLine(extraTextAtMaxEntries);
                break;
            }

            builder.AppendInNewLine(data.reportString);
            if (DebugActions.EnableFuelUsageOrder)
                builder.Append($" (order: {data.sortingOrder})");
            entries++;
        }

        if (otherFuel > 0f && entries <= maxEntries)
            builder.AppendInNewLine($"{Mathf.RoundToInt(otherFuel)} {"VGE_OtherFuel".Translate()}");

        CachedFuelConsumptionReport.CacheValues(engine, fuelCostRatio, maxEntries, startingText, extraTextAtMaxEntries, builder);

        return builder;

        void GetFuelConsumptionReports(IGravshipFuelProvider provider, Building_GravEngine gravEngine, List<CompGravshipThruster> thrusters, List<IGravshipFuelProvider> otherProviders)
        {
            var entry = provider.ConsumeFuelRatio(gravEngine, fuelCostRatio, thrusters, otherProviders, false);
            if (entry != null)
            {
                if (entry.isGenericFuel)
                    otherFuel += entry.totalAmount;
                else if (!entry.reportString.NullOrEmpty())
                    list.Add(entry);
            }
        }

        void HandleRefuelables(CompRefuelable refuelable, CompGravshipFacility facility)
        {
            if (facility.CanBeActive)
                otherFuel += refuelable.Fuel;
        }
    }

    public static List<FuelTabEntry> GetFuelTabEntriesForAllProviders(Building_GravEngine engine)
    {
        var list = CachedFuelTabEntry.GetCachedIfValid(engine, 0);
        if (list != null)
            return list;

        list = [];
        var genericEntry = new GenericFuelTabEntry(engine)
        {
            title = "VGE_FuelTab_OtherFuelTitle".Translate().CapitalizeFirst()
        };

        ForEachActiveProvider(engine, GetAllEntries, HandleRefuelables, false, false);

        if (genericEntry.maxFuel > 0f)
            list.Add(genericEntry);

        CachedFuelTabEntry.CacheValues(engine, list);

        return list;

        void GetAllEntries(IGravshipFuelProvider provider, Building_GravEngine gravEngine, List<CompGravshipThruster> thrusters, List<IGravshipFuelProvider> otherProviders)
        {
            var entry = provider.GetFuelTabEntry(gravEngine, thrusters, otherProviders);
            if (entry != null)
            {
                if (entry.Active)
                    list.Add(entry);
            }
            else
            {
                genericEntry.fuelProviders.Add(provider.ParentThing);
                genericEntry.currentFuel += provider.CurrentFuel(gravEngine);
                genericEntry.maxFuel += provider.MaxFuel(gravEngine);
            }
        }

        void HandleRefuelables(CompRefuelable refuelable, CompGravshipFacility facility)
        {
            genericEntry.fuelProviders.Add(refuelable.parent);
            genericEntry.currentFuel += refuelable.fuel;
            genericEntry.maxFuel += refuelable.Props.fuelCapacity;
        }
    }

    public static void ForEachActiveProvider(Building_GravEngine engine, Action<IGravshipFuelProvider, Building_GravEngine, List<CompGravshipThruster>, List<IGravshipFuelProvider>> action, Action<CompRefuelable, CompGravshipFacility> refuelableAction = null, bool includeCompActiveCheck = true, bool includeProviderActiveCheck = true)
    {
        if (engine == null || action == null)
            return;

        TmpThrustersList.Clear();
        TmpProvidersList.Clear();

        // Preparation
        for (var i = 0; i < engine.GravshipComponents.Count; i++)
        {
            var comp = engine.GravshipComponents[i];
            if (comp.parent.Spawned)
            {
                if (comp is CompGravshipThruster thruster && comp.CanBeActive)
                    TmpThrustersList.Add(thruster);
                // We don't change vanilla handling of CompRefuelable
                if (comp.Props.providesFuel && (!includeCompActiveCheck || comp.CanBeActive) && !comp.parent.HasComp<CompRefuelable>())
                {
                    var refuelable = comp.parent.GetComp<CompRefuelable>();

                    if (refuelable != null)
                        refuelableAction?.Invoke(refuelable, comp);
                    else if (comp.parent is IGravshipFuelProvider thingProvider)
                        TmpProvidersList.Add(thingProvider);
                    else if (comp is IGravshipFuelProvider compProvider)
                        TmpProvidersList.Add(compProvider);
                }
            }
        }

        while (TmpProvidersList.Count > 0)
        {
            // Remove the last element
            var current = TmpProvidersList[^1];
            TmpProvidersList.RemoveAt(TmpProvidersList.Count - 1);

            if (!includeProviderActiveCheck || current.IsActive(engine, TmpThrustersList, TmpProvidersList))
                action(current, engine, TmpThrustersList, TmpProvidersList);
        }

        TmpThrustersList.Clear();
        TmpProvidersList.Clear();
    }

    private sealed class GravshipFuelConsumptionReportCache
    {
        private int tick;
        private Building_GravEngine cachedEngine;
        private int links;
        private float cachedFuelCostRatio;
        private int cachedMaxEntries;
        private string cachedStartingText;
        private string cachedExtraTextAtMaxEntries;

        private StringBuilder cachedValue;

        internal StringBuilder GetCachedIfValid(Building_GravEngine engine, float fuelCostRatio, int maxEntries, string startingText, string extraTextAtMaxEntries, int cacheValidFor)
        {
            if (Find.TickManager.TicksGame > tick + cacheValidFor ||
                cachedEngine != engine ||
                engine == null ||
                links != engine.AffectedByFacilities.LinkedFacilitiesListForReading.Count ||
                cachedFuelCostRatio != fuelCostRatio ||
                cachedMaxEntries != maxEntries ||
                cachedStartingText != startingText ||
                cachedExtraTextAtMaxEntries != extraTextAtMaxEntries)
            {
                cachedValue = null;
                return null;
            }

            return cachedValue;
        }

        internal void CacheValues(Building_GravEngine engine, float fuelCostRatio, int maxEntries, string startingText, string extraTextAtMaxEntries, StringBuilder value)
        {
            tick = Find.TickManager.TicksGame;
            cachedEngine = engine;
            links = engine.AffectedByFacilities.LinkedFacilitiesListForReading.Count;
            cachedFuelCostRatio = fuelCostRatio;
            cachedMaxEntries = maxEntries;
            cachedStartingText = startingText;
            cachedExtraTextAtMaxEntries = extraTextAtMaxEntries;
            cachedValue = value;
        }
    }

    private sealed class GravshipFuelTabEntryCache
    {
        private int tick;
        private Building_GravEngine cachedEngine;
        private int links;

        private List<FuelTabEntry> cachedValue;

        internal List<FuelTabEntry> GetCachedIfValid(Building_GravEngine engine, int cacheValidFor)
        {
            if (Find.TickManager.TicksGame > tick + cacheValidFor || cachedEngine != engine || engine == null || links != engine.AffectedByFacilities.LinkedFacilitiesListForReading.Count)
            {
                cachedValue = null;
                return null;
            }

            return cachedValue;
        }

        internal void CacheValues(Building_GravEngine engine, List<FuelTabEntry> entries)
        {
            tick = Find.TickManager.TicksGame;
            cachedEngine = engine;
            links = engine.AffectedByFacilities.LinkedFacilitiesListForReading.Count;
            cachedValue = entries;
        }
    }
}