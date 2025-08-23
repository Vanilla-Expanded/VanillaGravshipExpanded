using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VanillaGravshipExpanded
{
    public class CompProperties_HeatManager : CompProperties
    {
        public CompProperties_HeatManager()
        {
            compClass = typeof(CompHeatManager);
        }
    }

    public class CompHeatManager : ThingComp
    {
        public CompProperties_HeatManager Props => props as CompProperties_HeatManager;

        private float heatUnits;
        private List<Room> cachedShipRooms;
        private int roomCacheTick;
        private bool shouldApplyHeat;
        public float HeatUnits => heatUnits;
        public Building_GravEngine Engine => parent as Building_GravEngine;
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref heatUnits, "heatUnits");
            Scribe_Values.Look(ref shouldApplyHeat, "shouldApplyHeat");
        }

        public void AddHeat(float amount)
        {
            heatUnits += amount;
            if (heatUnits > 0)
            {
                DistributeHeat();
            }
        }

        private void DistributeHeat()
        {
            var heatsinks = Engine.GravshipComponents
                .Select(comp => comp.parent.GetComp<CompHeatsink>()).Where(h => h != null)
                .ToList();

            if (heatsinks.Count > 0 && heatUnits > 0)
            {
                var sortedHeatsinks = heatsinks.OrderBy(h => h.Props.maxHeat - h.StoredHeat).ToList();

                while (heatUnits > 0 && sortedHeatsinks.Any(h => h.Props.maxHeat > h.StoredHeat))
                {
                    float remainingHeat = heatUnits;
                    int activeHeatsinks = sortedHeatsinks.Count(h => h.Props.maxHeat > h.StoredHeat);

                    if (activeHeatsinks == 0)
                        break;

                    float heatPerActiveHeatsink = remainingHeat / activeHeatsinks;
                    float totalTransferredThisRound = 0;

                    foreach (var heatsink in sortedHeatsinks)
                    {
                        float spaceInHeatsink = heatsink.Props.maxHeat - heatsink.StoredHeat;
                        if (spaceInHeatsink > 0)
                        {
                            float heatToTransfer = Mathf.Min(heatPerActiveHeatsink, spaceInHeatsink);
                            heatsink.AddHeat(heatToTransfer);
                            heatUnits -= heatToTransfer;
                            totalTransferredThisRound += heatToTransfer;
                        }
                    }

                    if (totalTransferredThisRound == 0)
                        break;
                }
            }

            if (heatUnits > 0 && TryApplyHeatToShip(heatUnits) is false)
            {
                shouldApplyHeat = true;
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            if (heatUnits > 0 && shouldApplyHeat)
            {
                if (TryApplyHeatToShip(heatUnits))
                {
                    shouldApplyHeat = false;
                }
            }
        }

        private bool TryApplyHeatToShip(float heatAmount)
        {
            var map = parent.Map;
            if (map == null)
                return false;
            if (Find.TickManager.TicksGame - roomCacheTick > 60)
            {
                cachedShipRooms = GetShipRooms();
                roomCacheTick = Find.TickManager.TicksGame;
            }

            if (cachedShipRooms == null || cachedShipRooms.Count == 0)
                return false;
            int totalCells = cachedShipRooms.Sum(room => room.CellCount);
            if (totalCells == 0)
                return false;
                
            float heatPerCell = heatAmount / totalCells;
            heatUnits -= heatAmount;
            foreach (var room in cachedShipRooms)
            {
                float roomHeat = heatPerCell * room.CellCount;
                room.PushHeat(roomHeat);
            }
            Log.Message("Pushed heat: " + heatAmount);
            return true;
        }

        private List<Room> GetShipRooms()
        {
            var shipRooms = new HashSet<Room>();
            foreach (var comp in Engine.GravshipComponents)
            {
                if (comp.parent.Position.UsesOutdoorTemperature(parent.Map))
                    continue;
                    
                var room = comp.parent.Position.GetRoom(parent.Map);
                if (room != null)
                {
                    shipRooms.Add(room);
                }
            }
            return shipRooms.Where(room => room.PsychologicallyOutdoors == false).ToList();
        }

        public void ClearGravEngineHeat()
        {
            heatUnits = 0;
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }

            if (DebugSettings.ShowDevGizmos)
            {
                yield return new Command_Action
                {
                    defaultLabel = "DEV: Add heat",
                    defaultDesc = "Add 1 heat unit to grav engine",
                    action = () => AddHeat(1f)
                };

                yield return new Command_Action
                {
                    defaultLabel = "DEV: Add 10 heat",
                    defaultDesc = "Add 10 heat units to grav engine",
                    action = () => AddHeat(10f)
                };

                yield return new Command_Action
                {
                    defaultLabel = "DEV: Clear heat",
                    defaultDesc = "Remove all heat from grav engine",
                    action = ClearGravEngineHeat
                };

                yield return new Command_Action
                {
                    defaultLabel = "DEV: Set heat to 100",
                    defaultDesc = "Set heat to 100 units",
                    action = () => heatUnits = 100f
                };
            }
        }
    }
}
