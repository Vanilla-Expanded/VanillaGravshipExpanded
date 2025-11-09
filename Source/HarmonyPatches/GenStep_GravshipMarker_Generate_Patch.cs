using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace VanillaGravshipExpanded
{
    [HarmonyPatch(typeof(GenStep_GravshipMarker), "Generate")]
    public static class GenStep_GravshipMarker_Generate_Patch
    {
        public static bool Prefix(Map map, GenStepParams parms)
        {
            if (ModsConfig.OdysseyActive)
            {
                Gravship gravship = parms.gravship;
                if (gravship != null)
                {
                    IntVec3 playerStartSpot = MapGenerator.PlayerStartSpot;
                    GravshipLandingMarker obj = ThingMaker.MakeThing(ThingDefOf.GravshipLandingMarker) as GravshipLandingMarker;
                    obj.gravship = gravship;
                    GenSpawn.Spawn(obj, playerStartSpot, map);
                }
            }
            return false;
        }
    }
}
