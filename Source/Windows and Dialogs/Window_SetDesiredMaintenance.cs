using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace VanillaGravshipExpanded
{

    public class Window_SetDesiredMaintenance : Window
    {


        public override Vector2 InitialSize => new Vector2(500f, 180f);
        private Vector2 scrollPosition = new Vector2(0, 0);

     
        private static readonly Color borderColor = new Color(0.13f, 0.13f, 0.13f);
        private static readonly Color fillColor = new Color(0, 0, 0, 0.1f);

        public Window_SetDesiredMaintenance(Thing building)
        {
            draggable = false;
            resizeable = false;
            preventCameraMotion = false;
           
        }


        public override void DoWindowContents(Rect inRect)
        {

            var outRect = new Rect(inRect);
            outRect.yMin += 40f;
            outRect.yMax -= 40f;
            outRect.width -= 16f;

            Text.Font = GameFont.Medium;
            var IntroLabel = new Rect(0, 0, 300, 32f);
            Widgets.Label(IntroLabel, "VGE_SetDesiredMaintenance".Translate().CapitalizeFirst());
            Text.Font = GameFont.Small;
            var IntroLabel2 = new Rect(0, 40, 450, 72f);
            Widgets.Label(IntroLabel2, "VGE_SetDesiredMaintenanceDesc".Translate()+ ": "+ World_ExposeData_Patch.maintenanceThreshold.ToStringPercent());
            if (Widgets.ButtonImage(new Rect(outRect.xMax - 18f - 4f, 2f, 18f, 18f), TexButton.CloseXSmall))
            {
                Close();
            }
            var SliderContainer1 = new Rect(0, 120, 450, 32f);
            SettingsHelper.HorizontalSliderLabeled(SliderContainer1, ref World_ExposeData_Patch.maintenanceThreshold, new FloatRange(0, 1), "0%", "100%");

        }
    }
}
