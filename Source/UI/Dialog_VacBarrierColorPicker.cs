using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace VanillaGravshipExpanded;

[HotSwappable]
[StaticConstructorOnStartup]
public class Dialog_VacBarrierColorPicker : Dialog_ColorPickerBase
{
    public static readonly Texture2D ColorValuePickerExp;

    public Building_VacBarrier_Recolorable vacBarrier;
    public Building_VacBarrier_Recolorable[] extraVacBarriers;

    private bool colorValueBarDragging = false;

    public override Color DefaultColor => vacBarrier.def.colorGenerator.ExemplaryColor;
    public override bool ShowDarklight => false;
    public override List<Color> PickableColors => Dialog_GlowerColorPicker.colors;
    public override bool ShowColorTemperatureBar => false;
    public virtual bool ShowColorValueBar => true;

    public override float ForcedColorValue
    {
        get
        {
            Color.RGBToHSV(color, out _, out _, out var v);
            return v;
        }
    }

    static Dialog_VacBarrierColorPicker()
    {
        ColorValuePickerExp = new Texture2D(Widgets.ColorTemperatureExp.width, Widgets.ColorTemperatureExp.height);

        for (var x = 0; x < ColorValuePickerExp.width; x++)
        {
            var val = (float)x / ColorValuePickerExp.width;
            var color = new Color(val, val, val);

            for (var y = 0; y < ColorValuePickerExp.height; y++)
                ColorValuePickerExp.SetPixel(x, y, color);
        }

        ColorValuePickerExp.Apply(false);
    }

    public Dialog_VacBarrierColorPicker(Building_VacBarrier_Recolorable vacBarrier, List<Building_VacBarrier_Recolorable> extraVacBarriers, Widgets.ColorComponents visibleTextfields, Widgets.ColorComponents editableTextfields) : base(visibleTextfields, editableTextfields)
    {
        this.vacBarrier = vacBarrier;
        this.extraVacBarriers = extraVacBarriers.ToArray();

        color = vacBarrier.barrierColor;
        oldColor = vacBarrier.barrierColor;
    }

    public override void SaveColor(Color color)
    {
        foreach (var extraVacBarrier in extraVacBarriers)
        {
            extraVacBarrier.barrierColor = color;
            extraVacBarrier.Notify_ColorChanged();
        }
    }

    public override void DoWindowContents(Rect inRect)
    {
        base.DoWindowContents(inRect);

        if (ShowColorValueBar)
        {
            using (TextBlock.Default())
            {
                var rectDivider = new RectDivider(inRect, 195906069);

                rectDivider.NewRow(245f);
                if (ShowColorTemperatureBar)
                    rectDivider.NewRow(30f);

                var rect = rectDivider.NewRow(34f);
                ColorValueBar(rect, ref color, ref colorValueBarDragging);
            }
        }
    }

    public static void ColorValueBar(Rect rect, ref Color color, ref bool dragging)
    {
        Color.RGBToHSV(color, out var h, out var s, out var v);

        var rectDivider = new RectDivider(rect, 661493905, new Vector2(17f, 0f));
        using (new TextBlock(TextAnchor.MiddleLeft))
        {
            string text2 = "VGE_ColorPickerValue".Translate().CapitalizeFirst();
            Widgets.Label(rectDivider.NewCol(Text.CalcSize(text2).x), text2);
            Widgets.Label(rectDivider.NewCol(Text.CalcSize("XXX%").x), v.ToStringPercent());
        }

        if (!dragging)
        {
            TooltipHandler.TipRegion(rect, "VGE_ColorPickerValueTooltip".Translate());
            MouseoverSounds.DoRegion(rect);
        }

        if (Event.current.button == 0)
        {
            if (dragging && Event.current.type == EventType.MouseUp)
            {
                dragging = false;
            }
            else if (Widgets.ClickedInsideRect(rectDivider) || (dragging && UnityGUIBugsFixer.MouseDrag()))
            {
                dragging = true;
                if (Event.current.type == EventType.MouseDrag)
                {
                    Event.current.Use();
                }

                var value = Mathf.Clamp01((Event.current.mousePosition.x - rectDivider.Rect.xMin) / rectDivider.Rect.width);
                color = Color.HSVToRGB(h, s, value);
            }
        }

        rectDivider.NewRow(6f);
        rectDivider.NewRow(6f, VerticalJustification.Bottom);
        GUI.DrawTexture(rectDivider, ColorValuePickerExp, ScaleMode.StretchToFill, true, 1f, Color.HSVToRGB(h, s, 1f), 0f, 0f);

        var width = rectDivider.Rect.width * v;
        var topArrowRect = new Rect(rectDivider.Rect.x + width - 6f, rectDivider.Rect.y - 6f, 12f, 12f);
        var bottomArrowRect = new Rect(rectDivider.Rect.x + width - 6f, rectDivider.Rect.yMax - 6f, 12f, 12f);
        GUI.DrawTextureWithTexCoords(topArrowRect, Widgets.SelectionArrow, new Rect(0f, 1f, 1f, -1f), true);
        GUI.DrawTextureWithTexCoords(bottomArrowRect, Widgets.SelectionArrow, new Rect(0f, 0f, 1f, 1f), true);
    }
}