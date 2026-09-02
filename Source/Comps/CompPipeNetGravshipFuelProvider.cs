using System;
using System.Collections.Generic;
using System.Linq;
using PipeSystem;
using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded;

public class CompPipeNetGravshipFuelProvider : CompGravshipFacility, IGravshipFuelProvider
{
    protected CompResourceStorage storage;
    protected GravshipFuelExtension extension;

    public Thing ParentThing => parent;

    public new CompProperties_PipeNetFuelProvider Props => (CompProperties_PipeNetFuelProvider)props;

    public override void PostPostMake()
    {
        base.PostPostMake();
        InitializeComps();
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        if (Scribe.mode == LoadSaveMode.LoadingVars)
            InitializeComps();
    }

    private void InitializeComps()
    {
        storage = parent.GetComps<CompResourceStorage>().FirstOrDefault(c => c.Props.pipeNet == Props.pipeNet);
        extension = Props.pipeNet.GetModExtension<GravshipFuelExtension>();
    }

    public bool IsActive(Building_GravEngine engine, List<CompGravshipThruster> activeThrusters, List<IGravshipFuelProvider> otherProviders)
    {
        if (Props.isGenericFuel)
            return true;

        if (activeThrusters.Any(t => t.parent.GetComps<CompResource>().Any(r => r.Props.pipeNet == Props.pipeNet)))
            return true;

        // Remove other providers using the same pipe net
        otherProviders?.RemoveAll(x => x is CompPipeNetGravshipFuelProvider other && Props.pipeNet == other.Props.pipeNet);
        return false;
    }

    public float CurrentFuel(Building_GravEngine engine) => storage.AmountStored;

    public float MaxFuel(Building_GravEngine engine) => storage.Props.storageCapacity;

    public float CurrentRangeProvidedByFuel(Building_GravEngine engine, List<CompGravshipThruster> activeThrusters, List<IGravshipFuelProvider> otherProviders)
    {
        if (Props.isGenericFuel)
        {
            // If specified, use the generic ratio
            if (Props.genericResourceToRangeRatio > 0)
                return storage.AmountStored / Props.genericResourceToRangeRatio;
            // If not specified, use the extension's ratio
            if (extension != null)
                return storage.AmountStored / extension.resourceToRangeRatio;
            // If extension is null, use the default chemfuel ratio of 10
            return storage.AmountStored / 10f;
        }

        var range = storage.AmountStored;
        otherProviders?.RemoveAll(x =>
        {
            if (x is not CompPipeNetGravshipFuelProvider other || Props.pipeNet != other.Props.pipeNet)
                return false;

            range += other.storage.AmountStored;
            return true;
        });

        range /= extension.resourceToRangeRatio;

        // Grab either the range provided by thrusters or max range of all thrusters
        return Mathf.Min(range, GetMaxRangeForThrusters(activeThrusters));
    }

    public float MaxRangeProvidedByFuel(Building_GravEngine engine, List<CompGravshipThruster> activeThrusters, List<IGravshipFuelProvider> otherProviders)
    {
        if (Props.isGenericFuel)
        {
            // If specified, use the generic ratio
            if (Props.genericResourceToRangeRatio > 0)
                return storage.Props.storageCapacity / Props.genericResourceToRangeRatio;
            // If not specified, use the extension's ratio
            if (extension != null)
                return storage.Props.storageCapacity / extension.resourceToRangeRatio;
            // If extension is null, use the default chemfuel ratio of 10
            return storage.Props.storageCapacity / 10f;
        }

        var maxRange = storage.Props.storageCapacity;
        otherProviders?.RemoveAll(x =>
        {
            if (x is not CompPipeNetGravshipFuelProvider other || Props.pipeNet != other.Props.pipeNet)
                return false;

            maxRange += other.storage.Props.storageCapacity;
            return true;
        });

        maxRange /= extension.resourceToRangeRatio;

        // return maxRange;
        // Grab either the range provided by thrusters or max range of all thrusters
        return Mathf.Min(maxRange, GetMaxRangeForThrusters(activeThrusters));
    }

    private float GetMaxRangeForThrusters(List<CompGravshipThruster> activeThrusters)
    {
        var maxRange = 0f;

        for (var i = 0; i < activeThrusters.Count; i++)
        {
            var thruster = activeThrusters[i];
            for (var j = 0; j < thruster.parent.AllComps.Count; j++)
            {
                if (thruster.parent.AllComps[j] is CompResource resource && Props.pipeNet == resource.Props.pipeNet)
                    maxRange += thruster.Props.statOffsets.GetStatOffsetFromList(StatDefOf.GravshipRange);
            }
        }

        return maxRange;
    }

    public float ConsumeFuelAmount(Building_GravEngine engine, float fuelAmount)
    {
        var amountStored = storage.AmountStored;
        if (amountStored >= fuelAmount)
        {
            storage.DrawResource(fuelAmount);
            return fuelAmount;
        }

        storage.Empty();
        return amountStored;
    }

    public FuelUsageData ConsumeFuelRatio(Building_GravEngine engine, float fuelConsumedRatio, List<CompGravshipThruster> activeThrusters, List<IGravshipFuelProvider> otherProviders, bool consumeFuel)
    {
        var data = new FuelUsageData();

        if (Props.isGenericFuel)
        {
            var amountToConsume = storage.AmountStored * fuelConsumedRatio;
            if (consumeFuel)
                storage.DrawResource(amountToConsume);
            data.fuelData[this] = amountToConsume;
            data.totalAmount = amountToConsume;
            // Sorting order shouldn't matter for generic providers, but let's include in case it's ever needed
            data.sortingOrder = amountToConsume / fuelConsumedRatio;
            data.isGenericFuel = true;
            return data;
        }

        var totalFuel = CurrentFuel(engine);
        // CurrentRangeProvidedByFuel would remove all from the list if we passed the list, so we'll need to manually iterate over all thrusters
        var range = CurrentRangeProvidedByFuel(engine, activeThrusters, null);

        if (otherProviders != null)
        {
            for (var i = 0; i < otherProviders.Count; i++)
            {
                if (otherProviders[i] is CompPipeNetGravshipFuelProvider other && Props.pipeNet == other.Props.pipeNet)
                {
                    totalFuel += other.CurrentFuel(engine);
                    range += other.CurrentRangeProvidedByFuel(engine, activeThrusters, null);
                }
            }
        }

        range = Mathf.Min(range, GetMaxRangeForThrusters(activeThrusters));
        var toConsume = range * fuelConsumedRatio * extension.resourceToRangeRatio;
        var toConsumeRatio = toConsume / totalFuel;

        var amount = storage.AmountStored * toConsumeRatio;
        if (consumeFuel)
            storage.DrawResource(amount);
        data.fuelData[this] = data.totalAmount = amount;
        otherProviders?.RemoveAll(x =>
        {
            if (x is not CompPipeNetGravshipFuelProvider other || Props.pipeNet != other.Props.pipeNet)
                return false;

            var amount = other.storage.AmountStored * toConsumeRatio;
            if (consumeFuel)
                other.storage.DrawResource(amount);
            data.fuelData[x] = amount;
            data.totalAmount += amount;
            return true;
        });

        data.sortingOrder = range * fuelConsumedRatio;
        if (data.totalAmount > 0)
            data.reportString = $"{data.totalAmount.ToStringDecimalIfSmall()} {Props.pipeNet.resource.name.UncapitalizeFirst()}";

        return data;
    }

    public float AddFuelAmount(Building_GravEngine engine, float amount)
    {
        var canAccept = storage.AmountCanAccept;
        if (canAccept >= amount)
        {
            storage.AddResource(amount);
            return amount;
        }

        storage.AddResource(canAccept);
        return canAccept;
    }

    public FuelTabEntry GetFuelTabEntry(Building_GravEngine engine, List<CompGravshipThruster> activeThrusters, List<IGravshipFuelProvider> otherProviders)
    {
        if (Props.isGenericFuel)
            return null;

        var entry = new SimpleMultiLineTextEntry(engine)
        {
            title = "VGE_FuelTab_ThrustersTitle".Translate(Props.pipeNet.resource.name.UncapitalizeFirst().Named("RESOURCE")).CapitalizeFirst(),
        };
        var currentFuel = CurrentFuel(engine);
        var maxFuel = MaxFuel(engine);
        entry.fuelProviders.Add(ParentThing);

        otherProviders?.RemoveAll(x =>
        {
            if (x is not CompPipeNetGravshipFuelProvider other || Props.pipeNet != other.Props.pipeNet)
                return false;

            entry.fuelProviders.Add(other.ParentThing);
            currentFuel += other.CurrentFuel(engine);
            maxFuel += other.MaxFuel(engine);

            return true;
        });

        var range = currentFuel / extension.resourceToRangeRatio;
        var maxRange = maxFuel / extension.resourceToRangeRatio;

        for (var i = 0; i < activeThrusters.Count; i++)
        {
            var thruster = activeThrusters[i];
            if (thruster.parent.GetComp<CompResource>() is { } comp && comp.Props.pipeNet == Props.pipeNet)
                entry.thrusters.Add(thruster.parent);
        }

        var maxRangeForThrusters = GetMaxRangeForThrusters(activeThrusters);
        if (range > maxRangeForThrusters)
            range = maxRangeForThrusters;
        if (maxRange > maxRangeForThrusters)
            maxRange = maxRangeForThrusters;

        entry.text.Add($"{"VGE_FuelTab_Thrusters".Translate().CapitalizeFirst()}: {entry.thrusters.Count}");
        entry.text.Add($"{Props.pipeNet.resource.name.CapitalizeFirst()}: {currentFuel} / {maxFuel}");
        entry.text.Add($"{"VGE_FuelTab_Range".Translate().CapitalizeFirst()}: {range} / {maxRange}");
        entry.text.Add("VGE_FuelTab_UsagePerTile".Translate((extension.resourceToRangeRatio * engine.FuelUseageFactor).Named("COST"), Props.pipeNet.resource.name.UncapitalizeFirst().Named("RESOURCE")).CapitalizeFirst());

        return entry;
    }
}