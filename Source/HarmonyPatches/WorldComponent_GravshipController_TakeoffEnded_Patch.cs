using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Linq;
using Verse;

namespace VanillaGravshipExpanded
{
    [HotSwappable]
    [HarmonyPatch(typeof(WorldComponent_GravshipController), "TakeoffEnded")]
    public static class WorldComponent_GravshipController_TakeoffEnded_Patch
    {
        public static void Prefix(WorldComponent_GravshipController __instance)
        {
            if (__instance.mapHasGravAnchor is false && __instance.map.Parent.ShouldHaveKeepMapUI())
            {
                __instance.mapHasGravAnchor = true;
                var map = __instance.map;
                Find.WindowStack.Add(new Dialog_MessageBox("VGE_MapDecisionText".Translate(), "VGE_KeepMap".Translate(), delegate
                {
                    Log.Message($"[VGE] Keeping map: " + map.Parent.Faction?.def + " - is player home: " + map.IsPlayerHome);
                }, "VGE_DiscardMap".Translate(), delegate
                {
                    map.Parent.Abandon(wasGravshipLaunch: true);
                }, buttonADestructive: false));
            }
        }

        public static bool ShouldHaveKeepMapUI(this MapParent mapParent)
        {
            var hasAbandonComp = mapParent.GetComponent<AbandonComp>() != null;
            var shouldRemoveMap = mapParent.ShouldRemoveMapNow(out _);
            var isStartingMap = mapParent.Map.IsStartingMap && mapParent.Map.IsPlayerHome && Find.Scenario.AllParts.OfType<ScenPart_ForcedMap>().Any(x => x.mapGenerator == MapGeneratorDefOf.OrbitalRelay);
            return hasAbandonComp && shouldRemoveMap is false && isStartingMap is false;
        }
    }
}
