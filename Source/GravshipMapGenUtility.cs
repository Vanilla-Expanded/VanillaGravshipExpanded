using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace VanillaGravshipExpanded
{
    public static class GravshipMapGenUtility
    {
        public static List<Thing> BlockingThings = new List<Thing>();
        public static List<IntVec3> RoofedCells = new List<IntVec3>();
        public static void Reset()
        {
            BlockingThings.Clear();
            RoofedCells.Clear();
        }
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
    }
}
