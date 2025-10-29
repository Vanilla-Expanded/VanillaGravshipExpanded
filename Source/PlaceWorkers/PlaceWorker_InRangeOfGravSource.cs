using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded
{
    public class PlaceWorker_InRangeOfGravSource : PlaceWorker
    {
        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            Map currentMap = Find.CurrentMap;
            CompProperties_GravshipFacility compProperties = def.GetCompProperties<CompProperties_GravshipFacility>();
            foreach (Thing item in currentMap.listerThings.ThingsOfDef(ThingDefOf.GravEngine))
            {
                GenDraw.DrawLineBetween(center.ToVector3Shifted(), item.TrueCenter(), center.InHorDistOf(item.Position, compProperties.maxDistance) ? SimpleColor.Green : SimpleColor.Red);
            }
            foreach (Thing item2 in currentMap.listerThings.ThingsOfDef(VGEDefOf.VGE_GravFieldAmplifier))
            {
                GenDraw.DrawLineBetween(center.ToVector3Shifted(), item2.TrueCenter(), center.InHorDistOf(item2.Position, compProperties.maxDistance) ? SimpleColor.Green : SimpleColor.Red);
            }
        }

        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            if (!(checkingDef is ThingDef thingDef))
            {
                return AcceptanceReport.WasRejected;
            }
            CompProperties_GravshipFacility compProperties = thingDef.GetCompProperties<CompProperties_GravshipFacility>();
            foreach (Thing item in map.listerThings.ThingsOfDef(ThingDefOf.GravEngine))
            {
                if (loc.InHorDistOf(item.Position, compProperties.maxDistance))
                {
                    return AcceptanceReport.WasAccepted;
                }
            }
            foreach (Thing item2 in map.listerThings.ThingsOfDef(VGEDefOf.VGE_GravFieldAmplifier))
            {
                if (loc.InHorDistOf(item2.Position, compProperties.maxDistance))
                {
                    return AcceptanceReport.WasAccepted;
                }
            }
            return "VGE_MustBePlacedInRangeOfGravSource".Translate();
        }
    }
}