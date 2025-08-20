using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace VanillaGravshipExpanded
{
    [HotSwappable]
    public static class GravshipMapGenUtility
    {
        public static IEnumerable<CellRect> ClampOccupiedRectsToRadius(IEnumerable<CellRect> originalRects, Gravship gravship)
        {
            IntVec3 enginePosition = gravship.Engine.Position;
            List<CellRect> clampedRects = new List<CellRect>();

            foreach (CellRect rect in originalRects)
            {
                CellRect clamped = new CellRect();
                bool hasCellsInRadius = false;

                foreach (IntVec3 cell in rect)
                {
                    if (cell.InHorDistOf(enginePosition, 30f))
                    {
                        if (!hasCellsInRadius)
                        {
                            clamped = new CellRect(cell.x, cell.z, 1, 1);
                            hasCellsInRadius = true;
                        }
                        else
                        {
                            clamped.Encapsulate(cell);
                        }
                    }
                }

                if (hasCellsInRadius)
                {
                    clampedRects.Add(clamped);
                }
            }

            return clampedRects;
        }

        public static HashSet<IntVec3> ClampCellsToRadius(HashSet<IntVec3> cells, Gravship gravship)
        {
            IntVec3 enginePosition = gravship.Engine.Position;
            cells.RemoveWhere(cell => cell.DistanceTo(enginePosition) > 30f);
            return cells;
        }
        
        public static HashSet<Thing> BlockingThings = new HashSet<Thing>();
        public static HashSet<Thing> GetBlockingThings(IEnumerable<IntVec3> cells, Map map)
        {
            var blockingThings = new HashSet<Thing>();
            foreach (var cell in cells)
            {
                blockingThings.AddRange(GetBlockingThingsInCell(cell, map));
            }
            BlockingThings = blockingThings;
            return blockingThings;
        }

        public static IEnumerable<Thing> GetBlockingThingsInCell(IntVec3 cell, Map map)
        {
            foreach (var thing in cell.GetThingList(map))
            {
                if (!thing.def.preventGravshipLandingOn)
                {
                    var building = thing.def.building;
                    if (building == null || building.canLandGravshipOn)
                    {
                        continue;
                    }
                }
                yield return thing;
            }
        }
    }
}
