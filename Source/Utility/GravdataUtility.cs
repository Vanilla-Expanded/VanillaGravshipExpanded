using System;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace VanillaGravshipExpanded
{
    [HotSwappable]
    public static class GravdataUtility
    {
        public static float CalculateYieldMultiplier(Building_GravEngine engine)
        {
            float yieldMultiplier = 1f;
            foreach (var facility in engine.GravshipComponents)
            {
                var comp = facility.parent.GetComp<CompGravdataYield>();
                if (comp != null)
                {
                    yieldMultiplier *= comp.Multiplier;
                }
            }
            return yieldMultiplier;
        }

        public static int CalculateGravdataYield(float distanceTravelled, float gravshipResearchStat, float launchRitualQuality, float gravdataYieldMultiplier)
        {
            float gravdataYield = ((distanceTravelled * gravshipResearchStat) * launchRitualQuality) * gravdataYieldMultiplier;
            return (int)Math.Ceiling(gravdataYield);
        }

        public static int CalculateGravdataYield(float distanceTravelled, float launchRitualQuality, Building_GravEngine engine, Pawn researcherPawn)
        {
            float gravshipResearchStat = 0f;
            if (researcherPawn != null)
            {
                gravshipResearchStat = researcherPawn.GetStatValue(VGEDefOf.VGE_GravshipResearch);
            }
            float yieldMultiplier = CalculateYieldMultiplier(engine);
            var gravdataYield = CalculateGravdataYield(distanceTravelled, gravshipResearchStat, launchRitualQuality, yieldMultiplier);
            //Log.Message($"[Gravdata] CalculateGravdataYield called with Distance: {distanceTravelled}, Quality: {launchRitualQuality}, Researcher: {researcherPawn?.Name}, ResearchStat: {gravshipResearchStat}, YieldMultiplier: {yieldMultiplier} - Result: {gravdataYield}");
            Log.ResetMessageCount();
            return gravdataYield;
        }
    }
}
