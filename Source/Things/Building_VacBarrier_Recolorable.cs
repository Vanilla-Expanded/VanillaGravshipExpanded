using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded;

public class Building_VacBarrier_Recolorable : Building_VacBarrier
{
    private const Widgets.ColorComponents EditableRgb = Widgets.ColorComponents.Red | Widgets.ColorComponents.Green | Widgets.ColorComponents.Blue;
    private static readonly List<Building_VacBarrier_Recolorable> TmpExtraBarriers = new(64);
    private static int LastBarrierGizmoUpdateFrameCount = 0;

    public Color barrierColor;

    public override Graphic Graphic
    {
        get
        {
            var style = StyleDef;
            if (style?.Graphic != null)
                return styleGraphicInt ??= style.graphicData != null ? GraphicColored(style.graphicData) : style.Graphic;

            if (graphicInt == null)
            {
                if (def.graphicData == null)
                    return BaseContent.BadGraphic;
                graphicInt = GraphicColored(def.graphicData);
            }

            return graphicInt;
        }
    }

    public override void PostMake()
    {
        base.PostMake();

        if (def.colorGenerator != null)
            barrierColor = def.colorGenerator.NewRandomizedColor();
    }

    public override void ExposeData()
    {
        base.ExposeData();

        Scribe_Values.Look(ref barrierColor, "color", Color.white);
    }

    protected Graphic GraphicColored(GraphicData graphicData)
    {
        // If color indistinguishable from original, use original
        if (barrierColor.IndistinguishableFrom(graphicData.Graphic.Color) && DrawColorTwo.IndistinguishableFrom(graphicData.Graphic.ColorTwo))
            return graphicData.Graphic;
        // Use colored version
        return graphicData.Graphic.GetColoredVersion(graphicData.Graphic.Shader, barrierColor, DrawColorTwo);
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (var gizmo in base.GetGizmos())
            yield return gizmo;

        // Since there can be multiple selected vac barriers, don't run this code more than once per frame.
        // It would result in wasted processing power, and it already caused some unintended issues.
        if (Time.frameCount == LastBarrierGizmoUpdateFrameCount)
            yield break;
        LastBarrierGizmoUpdateFrameCount = Time.frameCount;

        Color32? color = barrierColor;
        var extraBarriers = ExtraSelectedBarriers();

        foreach (var extraBarrier in extraBarriers)
        {
            if (extraBarrier.barrierColor != barrierColor)
            {
                color = null;
                break;
            }
        }

        yield return new Command_ColorIcon
        {
            defaultLabel = "GlowerChangeColor".Translate(),
            defaultDesc = "GlowerChangeColorDescription".Translate(),
            icon = ContentFinder<Texture2D>.Get("UI/Commands/ChangeColor"),
            color = color,
            action = () => Find.WindowStack.Add(new Dialog_VacBarrierColorPicker(this, extraBarriers, EditableRgb, EditableRgb)),
        };
    }

    private static List<Building_VacBarrier_Recolorable> ExtraSelectedBarriers()
    {
        TmpExtraBarriers.Clear();

        foreach (var obj in Find.Selector.SelectedObjectsListForReading)
        {
            if (obj is Building_VacBarrier_Recolorable vacBarrier)
                TmpExtraBarriers.Add(vacBarrier);
        }

        LastBarrierGizmoUpdateFrameCount = Time.frameCount;
        return TmpExtraBarriers;
    }
}