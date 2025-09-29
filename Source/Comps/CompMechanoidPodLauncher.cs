using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace VanillaGravshipExpanded
{
    public class CompProperties_MechanoidPodLauncher : CompProperties
    {
        public int spawnInterval = 60000;
        public int spawnCount = 4;
        public List<PawnKindDef> pawnKinds;
        public int maxRange = 50;

        public CompProperties_MechanoidPodLauncher()
        {
            compClass = typeof(CompMechanoidPodLauncher);
        }
    }
    public class CompMechanoidPodLauncher : ThingComp
    {
        private int ticksToSpawn;
        private int podsLeft;

        public CompProperties_MechanoidPodLauncher Props => (CompProperties_MechanoidPodLauncher)props;

        private PawnKindDef pawnKind;

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            pawnKind = Props.pawnKinds.RandomElement();
            ResetTimer();
            podsLeft = Props.spawnCount;
        }

        private void ResetTimer()
        {
            ticksToSpawn = Props.spawnInterval;
        }

        public override void CompTick()
        {
            base.CompTick();
            if (parent.Map != null)
            {
                ticksToSpawn--;
                if (ticksToSpawn <= 0)
                {
                    if (podsLeft > 0)
                    {
                        SpawnPod();
                        podsLeft--;
                    }
                    ResetTimer();
                }
            }
        }

        private void SpawnPod()
        {
            var pawns = new List<Pawn>();
            Pawn pawn = PawnGenerator.GeneratePawn(pawnKind, Faction.OfMechanoids);
            pawns.Add(pawn);
            ActiveTransporter activeTransporter = (ActiveTransporter)ThingMaker.MakeThing(ThingDefOf.ActiveDropPod);
            activeTransporter.Contents = new ActiveTransporterInfo();
            activeTransporter.Contents.innerContainer.TryAddRangeOrTransfer(pawns);
            activeTransporter.Contents.sentTransporterDef = parent.def;
            Map destinationMap = GetDestinationMap();
            FlyShipLeaving flyShip = (FlyShipLeaving)SkyfallerMaker.MakeSkyfaller(VGEDefOf.DropPodLeavingMechanoid, activeTransporter);
            flyShip.groupID = Find.UniqueIDsManager.GetNextTransporterGroupID();
            flyShip.destinationTile = destinationMap.Tile;
            IntVec3 landingSpot = GetLandingCell(destinationMap);
            var arrivalAction = new TransportersArrivalAction_LandInSpecificCell_Mechanoid(destinationMap.Parent, landingSpot);
            flyShip.arrivalAction = arrivalAction;
            GenSpawn.Spawn(flyShip, parent.Position, parent.Map, Rot4.North);
        }

        private Map GetDestinationMap()
        {
            Map destinationMap = null;
            foreach (Map map in Find.Maps.Where(x => x.IsPlayerHome))
            {
                int distance = GravshipHelper.GetDistance(parent.Map.Tile, map.Tile);
                if (distance <= Props.maxRange)
                {
                    destinationMap = map;
                    break;
                }
            }
            if (destinationMap == null)
            {
                destinationMap = parent.Map;
            }
            return destinationMap;
        }

        private IntVec3 GetLandingCell(Map map)
        {
            var homeArea = map.areaManager.Home;
            if (homeArea != null && homeArea.ActiveCells.Where(x => x.Walkable(map)).TryRandomElement(out var cell))
            {
                return cell;
            }
            return DropCellFinder.FindRaidDropCenterDistant(map);
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref ticksToSpawn, "ticksToSpawn");
            Scribe_Values.Look(ref podsLeft, "podsLeft");
            Scribe_Defs.Look(ref pawnKind, "pawnKind");
        }

        public override string CompInspectStringExtra()
        {
            return "VGE_MechanoidPodGestation".Translate(pawnKind.LabelCap, ticksToSpawn.ToStringTicksToPeriod()) + "\n" + "VGE_MechanoidPodsLeft".Translate(podsLeft);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (DebugSettings.ShowDevGizmos)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Dev: Set pod timer to 1 tick",
                    action = delegate ()
                    {
                        ticksToSpawn = 1;
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "Dev: Reset pod timer",
                    action = delegate ()
                    {
                        ResetTimer();
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "Dev: Add 1 pod left",
                    action = delegate ()
                    {
                        podsLeft++;
                    }
                };
            }
        }
    }

    public class TransportersArrivalAction_LandInSpecificCell_Mechanoid : TransportersArrivalAction_LandInSpecificCell
    {
        public TransportersArrivalAction_LandInSpecificCell_Mechanoid()
        {
        }

        public TransportersArrivalAction_LandInSpecificCell_Mechanoid(MapParent mapParent, IntVec3 cell)
        {
            this.mapParent = mapParent;
            this.cell = cell;
        }

        public override void Arrived(List<ActiveTransporterInfo> transporters, PlanetTile tile)
        {
            TransportersArrivalActionUtility.DropTravellingDropPods(transporters, cell, mapParent.Map);
        }
    }
}
