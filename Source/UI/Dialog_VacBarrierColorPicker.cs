using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded;

public class Dialog_VacBarrierColorPicker : Dialog_ColorPickerBase
{
    public Building_VacBarrier_Recolorable vacBarrier;
    public Building_VacBarrier_Recolorable[] extraVacBarriers;

    public override Color DefaultColor => vacBarrier.def.colorGenerator.ExemplaryColor;
    public override bool ShowDarklight => false;
    public override List<Color> PickableColors => Dialog_GlowerColorPicker.colors;
    public override float ForcedColorValue => 1f;
    public override bool ShowColorTemperatureBar => false;

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
}