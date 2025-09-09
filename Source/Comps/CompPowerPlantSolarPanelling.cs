
using RimWorld;
using UnityEngine;
using Verse;
namespace VanillaGravshipExpanded
{
    [StaticConstructorOnStartup]
    public class CompPowerPlantSolarPanelling : CompPowerPlantSolar
    {
    

        private static readonly Vector2 NewBarSize = new Vector2(0.9f, 0.14f);

       // private static readonly Material PowerPlantSolarBarFilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.5f, 0.475f, 0.1f));

       // private static readonly Material PowerPlantSolarBarUnfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.15f, 0.15f, 0.15f));

       

        public override void PostDraw()
        {
          
            GenDraw.FillableBarRequest r = default(GenDraw.FillableBarRequest);
            r.center = parent.DrawPos + Vector3.up * 0.1f;
            r.size = NewBarSize;
            r.fillPercent = base.PowerOutput / (0f - base.Props.PowerConsumption);
            r.filledMat = PowerPlantSolarBarFilledMat;
            r.unfilledMat = PowerPlantSolarBarUnfilledMat;
            r.margin = 0.15f;
            Rot4 rotation = parent.Rotation;
           // rotation.Rotate(RotationDirection.Clockwise);
            r.rotation = rotation;
            GenDraw.DrawFillableBar(r);
        }
    }
}