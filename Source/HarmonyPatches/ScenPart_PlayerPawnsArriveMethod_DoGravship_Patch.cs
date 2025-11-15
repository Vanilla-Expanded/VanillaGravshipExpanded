using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using KCSG;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(ScenPart_PlayerPawnsArriveMethod), nameof(ScenPart_PlayerPawnsArriveMethod.DoGravship))]
public static class ScenPart_PlayerPawnsArriveMethod_DoGravship_Patch
{
    public static bool Prefix(Map map, List<Thing> startingItems)
    {
        var orGenerateVar = MapGenerator.GetOrGenerateVar<List<CellRect>>("UsedRects");
        map.regionAndRoomUpdater.Enabled = true;
        var playerStartSpot = MapGenerator.PlayerStartSpot;
        var cellRect = CellRect.CenteredOn(playerStartSpot, VGEDefOf.VGE_StartingGravjumper.Sizes.x, VGEDefOf.VGE_StartingGravjumper.Sizes.z);
        var hashSet = cellRect.Cells.ToHashSet();
        if (!MapGenerator.PlayerStartSpotValid)
        {
            GenStep_ReserveGravshipArea.SetStartSpot(map, hashSet, orGenerateVar);
            playerStartSpot = MapGenerator.PlayerStartSpot;
        }
        GravshipPlacementUtility.ClearAreaForGravship(map, playerStartSpot, hashSet);
        var list = new HashSet<Thing>();
        cellRect = CellRect.CenteredOn(playerStartSpot, cellRect.Width, cellRect.Height);
        GenOption.GetAllMineableIn(cellRect, map);
        LayoutUtils.CleanRect(VGEDefOf.VGE_StartingGravjumper, map, cellRect, true);
        VGEDefOf.VGE_StartingGravjumper.Generate(cellRect, map, list, Faction.OfPlayer);

        orGenerateVar.Add(cellRect);
        foreach (var startingAndOptionalPawn in Find.GameInitData.startingAndOptionalPawns)
        {
            if (!cellRect.TryRandomElement(c => c.Standable(map) && (c.GetTerrain(map)?.IsSubstructure ?? false), out var result))
            {
                Log.Error("Could not find a valid spawn location for pawn " + startingAndOptionalPawn.Name);
            }
            else
            {
                GenPlace.TryPlaceThing(startingAndOptionalPawn, result, map, ThingPlaceMode.Near);
            }
        }

        var allShelves = list.OfType<Building_Storage>().ToList();
        var emptyShelves = new List<Building_Storage>(allShelves);
        foreach (var startingItem in startingItems)
        {
            if (startingItem.def.CanHaveFaction)
            {
                startingItem.SetFactionDirect(Faction.OfPlayer);
            }
            var countLeft = startingItem.stackCount;
            var attempts = 99;
            while (countLeft > 0 && attempts-- > 0)
            {
                // First try to use empty shelves
                if (!emptyShelves.Where(x => x.GetParentStoreSettings().AllowedToAccept(startingItem)).TryRandomElement(out var shelf))
                {
                    // If there's none, try placing around full shelves
                    allShelves.Where(x => x.GetParentStoreSettings().AllowedToAccept(startingItem)).TryRandomElement(out shelf);
                }

                IntVec3 cell;
                // Pick a shelf cell if possible
                if (shelf != null)
                {
                    cell = shelf.OccupiedRect().RandomCell;
                }
                // Try to pick any substructure tile
                else if (!cellRect.TryFindRandomCell(out cell, x => x.GetTerrain(map) == TerrainDefOf.Substructure && x.GetFirstThing<Building_Door>(map) == null))
                {
                    // Pick any tile in the rect
                    cell = cellRect.RandomCell;
                }

                var thing = startingItem.SplitOff(Math.Min(startingItem.def.stackLimit, countLeft));
                countLeft -= thing.stackCount;
                GenPlace.TryPlaceThing(thing, cell, map, ThingPlaceMode.Near, extraValidator: x => x.GetFirstThing<Building_Door>(map) == null);

                // If shelf is full after adding to it, remove it from list of empty shelves
                if (shelf != null && shelf.SpaceRemainingFor(startingItem.def) <= 0)
                    emptyShelves.Remove(shelf);
            }
        }
        foreach (var thing in list)
        {
            if (thing.def == ThingDefOf.Door)
            {
                MapGenerator.rootsToUnfog.AddRange(GenAdj.CellsAdjacentCardinal(thing));
            }
            if (thing is Building_GravEngine building_GravEngine)
            {
                building_GravEngine.silentlyActivate = true;
            }
            // Don't refuel stuff, since that will be handled through KCSG import
        }
        foreach (var cell in cellRect)
        {
            if (cell.GetTerrain(map) == TerrainDefOf.Substructure)
            {
                map.areaManager.Home[cell] = true;
            }
        }
        return false;
    }
}
