using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace VanillaGravshipExpanded;

public abstract class RitualPosition_GravshipLaunchBase : RitualPosition_Copilot
{
    protected abstract CompGravshipFacility GetRelevantComp(Thing thing);

    public override void FindCells(List<IntVec3> cells, Thing thing, CellRect rect, IntVec3 spot, Rot4 rotation)
    {
        if (thing.TryGetComp<CompPilotConsole>(out var comp) && comp.engine != null)
        {
            foreach (var facility in comp.engine.AffectedByFacilities.LinkedFacilitiesListForReading)
            {
                if (GetRelevantComp(facility) != null)
                {
                    IEnumerable<IntVec3> possibleCells;
                    if (facility.def.HasSingleOrMultipleInteractionCells)
                        possibleCells = facility.InteractionCells.InRandomOrder();
                    else
                        possibleCells = GenAdj.CellsAdjacentCardinal(facility);

                    foreach (var cell in possibleCells.InRandomOrder())
                    {
                        if (comp.engine.ValidSubstructureAt(cell))
                            cells.Add(cell);
                    }
                }
            }
        }

        // If we didn't find correct location, fallback to original behaviour
        if (!cells.Any())
            base.FindCells(cells, thing, rect, spot, rotation);
    }

    public override PawnStagePosition GetCell(IntVec3 spot, Pawn p, LordJob_Ritual ritual)
    {
        var result = base.GetCell(spot, p, ritual);

        // Make sure the pawn is facing the correct building, rather than the pilot console
        if (faceThing)
        {
            foreach (var cell in GenAdjFast.AdjacentCells8Way(result.cell))
            {
                var building = cell.GetEdificeSafe(ritual.Map);
                if (GetRelevantComp(building) != null && (!building.def.hasInteractionCell || building.InteractionCell == result.cell))
                    result.orientation = Rot4.FromAngleFlat((building.Position - result.cell).AngleFlat);
            }
        }

        return result;
    }
}