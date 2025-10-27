using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.Sound;

namespace VanillaGravshipExpanded
{
    public class Building_MechanoidResourceProcessor : Building
    {
        private int ticksToProduce;
        private int productionCycles;
        private ThingDef resource;
        private int resourceAmount;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ticksToProduce, "ticksToProduce");
            Scribe_Values.Look(ref productionCycles, "productionCycles");
            Scribe_Defs.Look(ref resource, "resource");
            Scribe_Values.Look(ref resourceAmount, "resourceAmount");
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                productionCycles = Rand.RangeInclusive(3, 6);
                SetNextProduction();
            }
        }

        public override void Tick()
        {
            base.Tick();
            if (productionCycles > 0)
            {
                ticksToProduce--;
                if (ticksToProduce <= 0)
                {
                    Produce();
                    SetNextProduction();
                }
            }
        }

        private void SetNextProduction()
        {
            ticksToProduce = 4 * GenDate.TicksPerHour;
            float value = Rand.Value;
            if (value < 0.25f)
            {
                resource = VGEDefOf.VGE_Astrofuel;
                resourceAmount = 30;
            }
            else if (value < 0.5f)
            {
                resource = ThingDefOf.GravlitePanel;
                resourceAmount = 45;
            }
            else if (value < 0.75f)
            {
                resource = ThingDefOf.ComponentSpacer;
                resourceAmount = 1;
            }
            else
            {
                resource = ThingDefOf.ComponentIndustrial;
                resourceAmount = 6;
            }
        }

        private void Produce()
        {
            Thing thing = ThingMaker.MakeThing(resource);
            thing.stackCount = resourceAmount;
            GenPlace.TryPlaceThing(thing, Position, Map, ThingPlaceMode.Near);
            SoundDefOf.CryptosleepCasket_Eject.PlayOneShot(new TargetInfo(Position, Map));
            productionCycles--;
        }

        public override string GetInspectString()
        {
            string text = base.GetInspectString();
            if (productionCycles > 0)
            {
                text += "VGE_ResourceProcessorInfo".Translate(resource.LabelCap, resourceAmount, ticksToProduce.ToStringTicksToPeriod(), productionCycles);
            }
            return text.TrimEndNewlines();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            if (DebugSettings.ShowDevGizmos)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Dev: Set production timer to 1 tick",
                    action = delegate ()
                    {
                        ticksToProduce = 1;
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "Dev: Reset production timer",
                    action = delegate ()
                    {
                        SetNextProduction();
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "Dev: Add 1 production cycle",
                    action = delegate ()
                    {
                        productionCycles++;
                    }
                };
            }
        }
    }
}
