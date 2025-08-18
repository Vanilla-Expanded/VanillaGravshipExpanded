using RimWorld;
using UnityEngine;
using Verse;
using System;
using System.Collections.Generic;
using System.Linq;



namespace VanillaGravshipExpanded
{


    public class GravshipsMod_Settings : ModSettings

    {

        public const float orbitalObjectsMultiplierBase = 2;
        public static float orbitalObjectsMultiplier = orbitalObjectsMultiplierBase;

        private static Vector2 scrollPosition = Vector2.zero;



        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look<float>(ref orbitalObjectsMultiplier, "orbitalObjectsMultiplier", orbitalObjectsMultiplierBase, true);

        }

        public void DoWindowContents(Rect inRect)
        {
            Listing_Standard ls = new Listing_Standard();

            var scrollContainer = inRect.ContractedBy(10);
            scrollContainer.height -= ls.CurHeight;
            scrollContainer.y += ls.CurHeight;
            Widgets.DrawBoxSolid(scrollContainer, Color.grey);
            var innerContainer = scrollContainer.ContractedBy(1);
            Widgets.DrawBoxSolid(innerContainer, new ColorInt(42, 43, 44).ToColor);
            var frameRect = innerContainer.ContractedBy(5);
            frameRect.y += 15;
            frameRect.height -= 15;
            var contentRect = frameRect;
            contentRect.x = 0;
            contentRect.y = 0;
            contentRect.width -= 20;      
            contentRect.height = 500;

            Listing_Standard ls2 = new Listing_Standard();

            Widgets.BeginScrollView(frameRect, ref scrollPosition, contentRect, true);
            ls2.Begin(contentRect.AtZero());


            var orbitLabel = ls2.LabelPlusButton("VGE_OrbitObjectsMultiplier".Translate() + ": " + orbitalObjectsMultiplier + "x", "VGE_OrbitObjectsMultiplierDesc".Translate());
            orbitalObjectsMultiplier = (float)Math.Round(ls2.Slider(orbitalObjectsMultiplier, 0f, 10f), 2);

            if (ls2.Settings_Button("VGE_Reset".Translate(), new Rect(0f, orbitLabel.position.y + 35, 250f, 29f)))
            {
                orbitalObjectsMultiplier = orbitalObjectsMultiplierBase;
            }
           
            ls2.End();
            Widgets.EndScrollView();
            Write();
        }
    }
}
