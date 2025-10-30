using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded;

[StaticConstructorOnStartup]
public class Building_Agrocell : Building_PlantGrower, IThingGlower
{
    protected static readonly Texture2D SunLampTexture = ContentFinder<Texture2D>.Get("Things/Building/Production/LampSun");

    protected bool growingPeriodOn = true;
    protected bool sunLampOn = true;
    protected CompGlower compGlower;

    public bool GrowingPeriodOn
    {
        get => growingPeriodOn;
        set
        {
            if (growingPeriodOn == value)
                return;

            growingPeriodOn = value;
            UpdatePowerUsage();
            compGlower.RefreshGlower();
        }
    }

    public bool SunLampOn
    {
        get => sunLampOn;
        set
        {
            if (sunLampOn == value)
                return;

            sunLampOn = value;
            UpdatePowerUsage();
            compGlower.RefreshGlower();
        }
    }

    public bool ShouldBeLitNow() => GrowingPeriodOn && SunLampOn;

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);

        RecalculateAllowed();
        UpdatePowerUsage();
    }

    public override void PostMake()
    {
        base.PostMake();

        InitializeComps();
    }

    public override void TickRare()
    {
        // In case of mods allowing for agrocells to be minified
        if (Spawned)
        {
            RecalculateAllowed();

            base.TickRare();
        }
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (var gizmo in base.GetGizmos())
            yield return gizmo;

        yield return new Command_Toggle
        {
            defaultLabel = SunLampOn ? "VGE_AgrocellSunLampDisable".Translate() : "VGE_AgrocellSunLampEnable".Translate(),
            defaultDesc = "VGE_AgrocellSunLampDesc".Translate(),
            toggleAction = () => SunLampOn = !SunLampOn,
            isActive = () => SunLampOn,
            icon = SunLampTexture,
        };
    }

    public override void ExposeData()
    {
        base.ExposeData();

        Scribe_Values.Look(ref sunLampOn, nameof(sunLampOn));

        if (Scribe.mode == LoadSaveMode.LoadingVars)
            InitializeComps();
    }

    public void RecalculateAllowed()
    {
        // Unhardcode it at some point, maybe?
        const float startTime = 0.25f;
        const float endTime = 0.8f;

        GrowingPeriodOn = GenLocalDate.DayPercent(this) is > startTime and < endTime;
    }

    protected void UpdatePowerUsage() => compPower.PowerOutput = ShouldBeLitNow() ? -compPower.Props.PowerConsumption : -compPower.Props.idlePowerDraw;

    private new void InitializeComps()
    {
        compGlower = GetComp<CompGlower>();
        // Base class doesn't handle it in a minifying-safe manner
        compPower = GetComp<CompPowerTrader>();
    }
}