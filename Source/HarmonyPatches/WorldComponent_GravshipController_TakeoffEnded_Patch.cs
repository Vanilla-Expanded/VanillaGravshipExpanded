using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Linq;
using Unity.Collections;
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
                    if (map.Parent is Settlement settlement && settlement.Faction == Faction.OfPlayer)
                    {
                        return;
                    }
                    SettleInExistingMapUtility.Settle(map);
                }, "VGE_DiscardMap".Translate(), delegate
                {
                    map.Parent.Abandon(wasGravshipLaunch: true);
                }, buttonADestructive: false));
            }
        }

        public static bool ShouldHaveKeepMapUI(this MapParent mapParent)
        {
            var map = mapParent.Map;
            var hasAbandonComp = mapParent.GetComponent<AbandonComp>() != null;
            var shouldRemoveMap = mapParent.ShouldRemoveMapNow(out _);
            var isStartingMap = map.IsStartingMap && map.IsPlayerHome && Find.Scenario.AllParts.OfType<ScenPart_ForcedMap>().Any(x => x.mapGenerator == MapGeneratorDefOf.OrbitalRelay);
            var canBeSettled = mapParent.CanBeSettled;
            var hasEnemies = map.attackTargetsCache.TargetsHostileToColony.Any(item => GenHostility.IsActiveThreatToPlayer(item));
            Log.Message($"[VGE] ShouldHaveKeepMapUI check for {mapParent.LabelCap}: canBeSettled={canBeSettled.Reason} - {canBeSettled == true}, hasAbandonComp={hasAbandonComp}, hasEnemies={hasEnemies}, shouldRemoveMap={shouldRemoveMap}, isStartingMap={isStartingMap}, mapParent.def {mapParent.def}, mapParent.GetType() {mapParent.GetType()}");
            return (canBeSettled || hasAbandonComp || shouldRemoveMap) && hasEnemies is false && isStartingMap is false || mapParent is Settlement settlement && settlement.Faction == Faction.OfPlayer;
        }
    }
}
