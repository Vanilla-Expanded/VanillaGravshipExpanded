using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded
{
    [HotSwappable]
    public class Projectile_Asteroid : Projectile_Space
    {
        private bool noLoot;
        private static readonly List<ThingDef> resourceDefs = new List<ThingDef>
        {
            ThingDefOf.Steel,
            ThingDefOf.Uranium,
            ThingDefOf.Gold,
            ThingDefOf.Plasteel
        };

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref noLoot, "noLoot", false);
        }

        public override void TickInterval(int delta)
        {
            lifetime -= delta;
            ticksToImpact -= delta;
            if (!ExactPosition.InBounds(base.Map))
            {
                noLoot = true;
            }
            lifetime += delta;
            ticksToImpact += delta;
            base.TickInterval(delta);
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (noLoot is false && this.Map != null)
            {
                Thing vacstone = ThingMaker.MakeThing(ThingDefOf.ChunkVacstone);
                GenPlace.TryPlaceThing(vacstone, this.Position, this.Map, ThingPlaceMode.Near);
                float rand = Rand.Value;
                bool shouldDropResources = false;
                if (this.def == VGEDefOf.VGE_MediumAsteroid && rand < 0.10f)
                {
                    shouldDropResources = true;
                }
                else if (this.def == VGEDefOf.VGE_LargeAsteroid && rand < 0.25f)
                {
                    shouldDropResources = true;
                }

                if (shouldDropResources)
                {
                    Thing resourceLoot = GetRandomResourceLoot();
                    GenPlace.TryPlaceThing(resourceLoot, this.Position, this.Map, ThingPlaceMode.Near);
                    resourceLoot.SetForbidden(true, false);
                }
            }
            
            base.Destroy(mode);
        }
        
        private Thing GetRandomResourceLoot()
        {
            ThingDef selectedDef = resourceDefs.RandomElement();
            
            int count;
            if (selectedDef == ThingDefOf.Steel || selectedDef == ThingDefOf.Uranium)
            {
                count = Rand.Range(5, 15);
            }
            else if (selectedDef == ThingDefOf.Gold)
            {
                count = Rand.Range(10, 30);
            }
            else
            {
                count = Rand.Range(1, 5);
            }

            Thing newLoot = ThingMaker.MakeThing(selectedDef);
            newLoot.stackCount = count;
            return newLoot;
        }
    }
}
