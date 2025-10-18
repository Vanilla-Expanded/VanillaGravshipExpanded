using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace VanillaGravshipExpanded;

public class Gizmo_OxygenProvider(CompApparelOxygenProvider oxygenProvider) : Gizmo_Slider
{
    private static bool draggingBar;

    protected CompApparelOxygenProvider oxygenProvider = oxygenProvider;

    public override bool IsDraggable => oxygenProvider.Wearer == null || oxygenProvider.Wearer.IsColonistPlayerControlled || oxygenProvider.Wearer.IsPrisonerOfColony;

    public override float Target
    {
        get => oxygenProvider.rechargeAtCharges / oxygenProvider.MaxCharges;
        set => oxygenProvider.SetRechargeValuePct(value);
    }

    public override float ValuePercent => oxygenProvider.ValuePercent;

    public override string Title => oxygenProvider.parent.LabelCapNoCount;

    public override bool DraggingBar
    {
        get => draggingBar;
        set => draggingBar = value;
    }

    public override FloatRange DragRange => new(0, 0.9f);

    public override Color BarColor => new(202f / 255f, 164f / 255f, 79f / 255f);

    public override Color BarHighlightColor => new(242f / 255f, 204f / 255f, 109f / 255f);

    public override string GetTooltip()
    {
        var text = $"{"VGE_OxygenUnits".Translate().CapitalizeFirst().Colorize(ColoredText.TipSectionTitleColor)}: {oxygenProvider.RemainingChargesExactString} / {oxygenProvider.MaxCharges}";

        if (oxygenProvider.Wearer == null || oxygenProvider.Wearer.IsColonistPlayerControlled || oxygenProvider.Wearer.IsPrisonerOfColony)
        {
            if (oxygenProvider.AutomaticRechargeEnabled)
                text += $"\n{"VGE_OxygenReplenishAt".Translate(oxygenProvider.rechargeAtCharges)}";
            else
                text += $"\n{"VGE_OxygenReplenishDisabled".Translate()}";
        }

        text += $"\n\n{"VGE_OxygenDrainIfActive".Translate(oxygenProvider.Props.consumptionPerTick * GenDate.TicksPerDay)}";

        return text;
    }

    public override void DrawHeader(Rect headerRect, ref bool mouseOverElement)
    {
        if (IsDraggable)
        {
            headerRect.xMax -= 24f;

            var iconRect = new Rect(headerRect.xMax, headerRect.y, 24f, 24f);
            Widgets.DefIcon(iconRect, VGEDefOf.VGE_OxygenCanister);
            GUI.DrawTexture(new Rect(iconRect.center.x, iconRect.y, iconRect.width / 2f, iconRect.height / 2f), oxygenProvider.AutomaticRechargeEnabled ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex);
            if (Widgets.ButtonInvisible(iconRect))
            {
                oxygenProvider.AutomaticRechargeEnabled = !oxygenProvider.AutomaticRechargeEnabled;
                if (oxygenProvider.AutomaticRechargeEnabled)
                    SoundDefOf.Tick_High.PlayOneShotOnCamera();
                else
                    SoundDefOf.Tick_Low.PlayOneShotOnCamera();
            }

            if (Mouse.IsOver(iconRect))
            {
                Widgets.DrawHighlight(iconRect);
                var onOff = (oxygenProvider.AutomaticRechargeEnabled ? "On" : "Off").Translate().ToString().UncapitalizeFirst();
                if (oxygenProvider.Wearer != null)
                    TooltipHandler.TipRegion(iconRect, () => "VGE_OxygenReplenishOnOffDesc".Translate(oxygenProvider.Wearer.Named("PAWN"), oxygenProvider.rechargeAtCharges.Named("MIN"), onOff.Named("ONOFF")), -1032044680);
                else
                    TooltipHandler.TipRegion(iconRect, () => "VGE_OxygenReplenishOnOffNoWearerDesc".Translate(oxygenProvider.rechargeAtCharges.Named("MIN"), onOff.Named("ONOFF")), 2146139727);
                mouseOverElement = true;
            }
        }
        
        base.DrawHeader(headerRect, ref mouseOverElement);
    }

    public override IEnumerable<float> GetBarThresholds()
    {
        for (var mult = 0.2f; mult < 0.9f; mult += 0.2f)
            yield return mult;
    }
}