using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using LudeonTK;

namespace VanillaGravshipExpanded
{
    public class CompProperties_HeatManager : CompProperties
    {
        public CompProperties_HeatManager()
        {
            compClass = typeof(CompHeatManager);
        }
    }

    [HotSwappable]
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

        public const float BaseHeatMultiplier = 100f;
        public const float BaseHeatsinkCapacityMultiplier = 5f;

        [TweakValue("0GravshipHeatMultiplier", 1f, 500f)]
        public static float HeatMultiplier = BaseHeatMultiplier;
        [TweakValue("0GravshipHeatsinkCapacityMultiplier", 1f, 100f)]
        public static float HeatsinkCapacityMultiplier = BaseHeatsinkCapacityMultiplier;

        // Keeping the old method for mod compatibility purposes, may remove in the future
        public void AddHeat(float amount) => AddHeat(amount, true);

        public float AddHeat(float amount, bool applyHeatMultiplier = true, bool applyToShip = true, bool storeExcess = true)
        {
            if (applyHeatMultiplier)
                amount *= HeatMultiplier;
            amount = DistributeHeat(amount, applyToShip);
            // If we don't store the excess in this comp, remove it from the heat units so whatever tried to add the heat will handle it itself
            if (!storeExcess)
            {
                heatUnits -= amount;
                if (heatUnits <= 0f)
                    shouldApplyHeat = false;
            }

            return amount;
        }

        private float DistributeHeat(float amount, bool applyToShip)
        {
            // Temporarily store old heat amount
            var oldStoredHeat = heatUnits;
            // Add the amount we're supposed to add to heat units
            heatUnits += amount;

            var heatsinks = Engine.GravshipComponents
                .Select(comp => comp.parent.GetComp<CompHeatsink>()).Where(h => h != null)
                .ToList();

            if (heatsinks.Count > 0 && heatUnits > 0)
            {
                var sortedHeatsinks = heatsinks.OrderBy(h => h.EffectiveMaxHeat - h.StoredHeat).ToList();

                while (heatUnits > 0 && sortedHeatsinks.Any(h => h.StoredHeat < h.EffectiveMaxHeat))
                {
                    float remainingHeat = heatUnits;
                    int activeHeatsinks = sortedHeatsinks.Count(h => h.StoredHeat < h.EffectiveMaxHeat);

                    if (activeHeatsinks == 0)
                    {
                        break;
                    }

                    float heatPerActiveHeatsink = remainingHeat / activeHeatsinks;
                    float totalTransferredThisRound = 0;

                    foreach (var heatsink in sortedHeatsinks)
                    {
                        float spaceInHeatsink = heatsink.EffectiveMaxHeat - heatsink.StoredHeat;
                        if (spaceInHeatsink > 0)
                        {
                            float heatToTransfer = Mathf.Min(heatPerActiveHeatsink, spaceInHeatsink);
                            heatsink.AddHeat(heatToTransfer);
                            heatUnits -= heatToTransfer;
                            totalTransferredThisRound += heatToTransfer;
                        }
                    }

                    if (totalTransferredThisRound == 0)
                    {
                        break;
                    }
                }
            }

            if (heatUnits > 0 && applyToShip)
            {
                bool result = TryApplyHeatToShip(heatUnits, true);
                if (result is false)
                {
                    shouldApplyHeat = true;
                }
            }

            // If we have less heat than when we started with, return 0 (no excess)
            if (heatUnits <= oldStoredHeat)
                return 0f;
            // If we have more heat units that when we started with, return the difference so we know how much heat we weren't able to store
            return heatUnits - oldStoredHeat;
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

        public bool TryApplyHeatToShip(float heatAmount) => TryApplyHeatToShip(heatAmount, false);

        private bool TryApplyHeatToShip(float heatAmount, bool removeFromHeatUnits)
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
            if (removeFromHeatUnits)
                heatUnits -= heatAmount;
            foreach (var room in cachedShipRooms)
            {
                float roomHeat = heatPerCell * room.CellCount;
                room.PushHeat(roomHeat);
            }
            return true;
        }

        private List<Room> GetShipRooms()
        {
            var shipRooms = new HashSet<Room>();
            // foreach (var pos in Engine.ValidSubstructure)
            // {
            //     if (pos.UsesOutdoorTemperature(parent.Map))
            //         continue;
            //
            //     var room = pos.GetRoom(parent.Map);
            //     if (room != null)
            //     {
            //         shipRooms.Add(room);
            //     }
            // }

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
