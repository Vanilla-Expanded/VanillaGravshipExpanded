using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using UnityEngine;

namespace VanillaGravshipExpanded
{
    public class MaintenanceAndDeterioration_MapComponent : MapComponent
    {
        private HashSet<Thing> thingsInSpace = new HashSet<Thing>();
        
        private Dictionary<Thing, int> timeInSpace = new Dictionary<Thing, int>();
        
        private int tickCounter = 0;
        
        private const int CHECK_INTERVAL = 60;
        
        private const float DAMAGE_PER_TICK = 0.01f / 60f;

        public HashSet<Thing> maintainables_InMap = new HashSet<Thing>();

        public MaintenanceAndDeterioration_MapComponent(Map map) : base(map)
        {
        }
        
        public override void MapComponentTick()
        {            
            if (map.Tile.LayerDef.isSpace)
            {
                tickCounter++;
                if (tickCounter >= CHECK_INTERVAL)
                {
                    tickCounter = 0;
                    ProcessSpaceDeterioration();
                }
            }
        }
        
        private void ProcessSpaceDeterioration()
        {
            var allThings = map.listerThings.AllThings;
            
            var currentlyInSpace = new HashSet<Thing>();
            
            for (int i = 0; i < allThings.Count; i++)
            {
                Thing thing = allThings[i];
                
                if (!ShouldDeteriorateInSpace(thing))
                {
                    continue;
                }
                
                if (IsOnSpaceTerrain(thing))
                {
                    currentlyInSpace.Add(thing);
                    
                    if (!thingsInSpace.Contains(thing))
                    {
                        thingsInSpace.Add(thing);
                        timeInSpace[thing] = 0;
                    }
                    
                    timeInSpace[thing] = timeInSpace[thing] + CHECK_INTERVAL;
                    
                    ApplySpaceDamage(thing);
                }
                else
                {
                    if (thingsInSpace.Contains(thing))
                    {
                        thingsInSpace.Remove(thing);
                        timeInSpace.Remove(thing);
                    }
                }
            }
            
            var toRemove = new List<Thing>();
            foreach (Thing thing in thingsInSpace)
            {
                if (!allThings.Contains(thing))
                {
                    toRemove.Add(thing);
                }
            }
            
            foreach (Thing thing in toRemove)
            {
                thingsInSpace.Remove(thing);
                timeInSpace.Remove(thing);
            }
        }
        
        private bool IsOnSpaceTerrain(Thing thing)
        {
            var position = thing.Position;
            if (!position.InBounds(map))
            {
                return false;
            }
            
            var terrain = position.GetTerrain(map);
            return terrain == TerrainDefOf.Space;
        }
        
        private bool ShouldDeteriorateInSpace(Thing thing)
        {
            if (thing is Pawn || thing is Building)
            {
                return false;
            }
            
            if (thing.def.destroyable == false || thing.def.useHitPoints == false)
            {
                return false;
            }
            
            return true;
        }
        
        private void ApplySpaceDamage(Thing thing)
        {
            var maxHitPoints = thing.MaxHitPoints;
            var damage = Mathf.Max(1, Mathf.RoundToInt(maxHitPoints * DAMAGE_PER_TICK * CHECK_INTERVAL));
            
            thing.TakeDamage(new DamageInfo(DamageDefOf.Deterioration, damage, 0f, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown));
            
            if (!thing.Destroyed && thing.HitPoints <= 0)
            {
                thing.Destroy();
            }
        }
        
        public int GetTimeInSpace(Thing thing)
        {
            if (timeInSpace.ContainsKey(thing))
            {
                return timeInSpace[thing];
            }
            return 0;
        }
        
        public bool IsThingInSpace(Thing thing)
        {
            return thingsInSpace.Contains(thing) && IsOnSpaceTerrain(thing);
        }

        public void AddMaintainableToMap(Thing thing)
        {
            if (!maintainables_InMap.Contains(thing))
            {
                maintainables_InMap.Add(thing);
            }
        }

        public void RemoveMaintainableFromMap(Thing thing)
        {
            if (maintainables_InMap.Contains(thing))
            {
                maintainables_InMap.Remove(thing);
            }
        }

        public float AverageMaintenanceInMap()
        {
            var totalMaintenance = 0f;
            var totalBuildings = 0;

            foreach (Thing thing in maintainables_InMap)
            {
                // Only player buildings
                if (thing.Faction != Faction.OfPlayer)
                    continue;

                var comp = thing.TryGetComp<CompGravMaintainable>();
                // Not null and maintenance is falling
                if (comp is not { maintenanceFalls: true })
                    continue;

                // TODO: Add a check for grav engine connection
                
                totalMaintenance += thing.TryGetComp<CompGravMaintainable>().maintenance;
                totalBuildings++;
            }

            if (totalBuildings > 0)
                return totalMaintenance / totalBuildings;
            return 1;
        }

        public void ChangeGlobalMaintenance(float amount, float chance)
        {
            if (maintainables_InMap.Count > 0)
            {
                foreach (Thing thing in maintainables_InMap)
                {
                    if (Rand.Chance(chance))
                    {
                        CompGravMaintainable comp = thing.TryGetComp<CompGravMaintainable>();
                        comp.maintenance += amount * thing.GetStatValue(VGEDefOf.VGE_MaintenanceSensitivity);
                    }

                }

            }

        }
    }
}
