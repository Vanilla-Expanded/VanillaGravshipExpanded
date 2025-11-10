using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
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
            if (__instance.mapHasGravAnchor || __instance.map?.info?.parent == null || !__instance.map.Parent.CanEverKeepThisMap())
            {
                Log.Message($"[VGE] Map has grav anchor: {__instance.map}, parent into null: {__instance.map?.info?.parent == null}, can ever keep: {__instance.map?.Parent.CanEverKeepThisMap()}.");
                return;
            }

            var map = __instance.map;
            var mapParent = map.Parent;

            if (mapParent.ShouldAlwaysKeepThisMap())
            {
                // The map has something preventing us from abandoning it. This could be
                // a colonist, a building, or perhaps transporters travelling to this map.
                __instance.mapHasGravAnchor = true;
                // If the map can be settled and there's no hostiles, give the player
                // an option to either settle or keep the map (without settling).
                if (map.Parent.CanBeSettled && !AnyHostilesOnMap(map))
                {
                    Find.WindowStack.Add(new Dialog_MessageBox(
                        "VGE_MapDecisionSettleText".Translate(),
                        // Do nothing when ignoring the map
                        "VGE_DontSettle".Translate(),
                        null,
                        // Settle the map
                        "VGE_SettleMap".Translate(),
                        SettleTile,
                        // We don't need to turn the abandon button red, you can settle it yourself if you decide to ignore it
                        buttonADestructive: false
                    ));
                }
            }
            else if (mapParent.ShouldHaveKeepMapUI())
            {
                // The map doesn't have anything left that would keep it around,
                // but we're allowed to settle it ourselves. Display a message box.
                __instance.mapHasGravAnchor = true;
                Find.WindowStack.Add(new Dialog_MessageBox(
                    "VGE_MapDecisionText".Translate(),
                    // Abandon and destroy the tile
                    "VGE_DiscardMap".Translate(),
                    AbandonTile,
                    // Settle the map
                    "VGE_KeepMap".Translate(),
                    SettleTile,
                    // We turn the abandon button red, since if you press the button the tile will disappear
                    buttonADestructive: true
                ));
            }
            else
            {
                // If the map cannot be settled, just abandon it. Gracefully if needed.
                Log.Message("[VGE] Map cannot be kept.");
                __instance.mapHasGravAnchor = true;
                AbandonTile();
            }

            void SettleTile()
            {
                if (map.Parent.CanBeSettled)
                    SettleInExistingMapUtility.Settle(map);
            }

            void AbandonTile()
            {
                if (map.Parent is Settlement settlement && settlement.Faction == Faction.OfPlayer)
                {
                    // In case of player settlements, just abandon them to turn them into abandoned settlement.
                    map.Parent.Abandon(wasGravshipLaunch: false);
                }
                else
                {
                    // For any other sort of site/settlement, just gracefully.
                    // Basically the same code as MapParent:CheckRemoveMapNow,
                    // except that we always remove the map.
                    map.Parent.ShouldRemoveMapNow(out var removeWorldObject);
                    Current.Game.DeinitAndRemoveMap(map, false);
                    if (!mapParent.Destroyed && (removeWorldObject || mapParent.forceRemoveWorldObjectWhenMapRemoved))
                        mapParent.Destroy();
                    // Perhaps add something along the line of:
                    // if (mapParent.Destroyed && tile.HasNoSiteOnIt()) SpawnAbandonedSettlementOrLaunchSite()?
                }
            }
        }

        public static bool CanEverKeepThisMap(this MapParent mapParent)
            => mapParent.Map is not { IsStartingMap: true, IsPlayerHome: true } || Find.Scenario.AllParts.OfType<ScenPart_ForcedMap>().All(x => x.mapGenerator != MapGeneratorDefOf.OrbitalRelay);

        public static bool ShouldAlwaysKeepThisMap(this MapParent mapParent)
        {
            // The rest works similar to MapParent.ShouldRemoveMapNow (with some changes).
            // We don't want to abandon the tile if there's colonists there, in its pocket maps,
            // there's building that prevent map removal, or we have incoming transporters.

            var map = mapParent.Map;

            if (map.mapPawns.AnyPawnBlockingMapRemoval || map.AnyBuildingBlockingMapRemoval || TransporterUtility.IncomingTransporterPreventingMapRemoval(map))
            {
                Log.Message("[VGE] ShouldAlwaysKeepThisMap map has pawns, buildings, or incoming drop pods blocking removal.");
                return true;
            }

            // Don't ask why the ToList call. Site:ShouldRemoveMapNow does this, so perhaps there's
            // a possible issue with the collection being modified while we iterate over it?
            // Also, calling AnyPawnBlockingMapRemoval checks all child pocket maps, but since
            // vanilla code checks for it again right after, perhaps there's a reason for it?
            foreach (var pocketMap in Find.World.pocketMaps.ToList())
            {
                // Also check for any buildings blocking map removal, as courtesy for mods that may want that.
                if (pocketMap.sourceMap == map && (pocketMap.Map.mapPawns.AnyPawnBlockingMapRemoval || pocketMap.Map.AnyBuildingBlockingMapRemoval))
                {
                    Log.Message("[VGE] ShouldAlwaysKeepThisMap pocket map has pawns or buildings blocking removal.");
                    return true;
                }
            }

            return false;
        }

        public static bool ShouldHaveKeepMapUI(this MapParent mapParent)
        {
            var map = mapParent.Map;

            // Check for abandon comp and make sure that the tile is actually abandonable (owned by player)
            var hasAbandonComp = mapParent.GetComponent<AbandonComp>() != null && mapParent.HasMap && mapParent.Faction == Faction.OfPlayer;
            var canBeSettled = mapParent.CanBeSettled;
            var ownedByPlayer = mapParent.Faction == Faction.OfPlayer;
            Log.Message($"[VGE] ShouldHaveKeepMapUI check for {mapParent.LabelCap}: canBeSettled={canBeSettled.Reason} - {canBeSettled == true}, hasAbandonComp={hasAbandonComp}, owner faction: {mapParent.Faction}, owned by player: {ownedByPlayer}, mapParent.def {mapParent.def}, mapParent.GetType() {mapParent.GetType()}");
            // Display the UI for maps that can either be settled or has an active abadon comp, but only if either there's no hostiles or the map is a player settlement.
            return (canBeSettled || hasAbandonComp) && (ownedByPlayer || !AnyHostilesOnMap(map));
        }

        private static bool AnyHostilesOnMap(Map map)
            => map.attackTargetsCache.TargetsHostileToColony.Any(item => GenHostility.IsActiveThreatToPlayer(item));
    }
}