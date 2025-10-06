using System;
using System.Linq;
using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(WorldComponent_GravshipController), nameof(WorldComponent_GravshipController.InitiateLanding))]
public static class GravshipPlacementUtility_PlaceGravshipInMap_Patch
{
    private static void Prefix(Gravship gravship, Map map, IntVec3 landingPos)
    {
        // We DON'T want to ever error on this, as this will break vanilla landing otherwise.
        // Wrap in try/catch for extra safety to prevent any exception from slipping through.
        try
        {
            const int maxSearchRadius = 10;

            var positions = gravship.Terrains
                .Select(x => x.Key + landingPos)
                .Concat(gravship.Foundations.Select(x => x.Key + landingPos))
                .ToHashSet();
            var pawns = positions
                .SelectMany(x => x.GetThingList(map))
                .OfType<Pawn>()
                .ToList();

            foreach (var pawn in pawns)
            {
                if (!pawn.IsAnimal && Rand.Bool)
                {
                    var cell = CellFinder.StandableCellNear(pawn.Position, map, maxSearchRadius, x => !positions.Contains(x));
                    if (cell.IsValid)
                        pawn.Position = cell;
                }
            }
        }
        catch (Exception e)
        {
            Log.Error($"[VGE] Exception caught trying to move pawns out of gravship landing area. Exception:\n{e}");
        }
    }
}