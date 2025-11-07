using RimWorld;
using RimWorld.Planet;
using Verse;

namespace VanillaGravshipExpanded
{
    public class QuestPart_SpawnThingInteractive : QuestPart
    {
        public string inSignal;
        public Thing thing;
        public Faction factionForFindingSpot;
        public MapParent mapParent;
        public bool questLookTarget = true;
        public bool lookForSafeSpot;
        public bool tryLandInShipLandingZone;
        public Thing tryLandNearThing;
        public Pawn mapParentOfPawn;
        public EffecterDef spawnEffecter;
        public bool canRoofPunch = true;
        public string outSignalResult;

        private bool spawned;

        public MapParent MapParent
        {
            get
            {
                if (mapParentOfPawn != null)
                {
                    return mapParentOfPawn.MapHeld?.Parent;
                }
                return mapParent;
            }
        }

        public override void Notify_QuestSignalReceived(Signal signal)
        {
            base.Notify_QuestSignalReceived(signal);
            if (signal.tag != inSignal)
            {
                return;
            }

            Map map = MapParent?.Map;
            if (map == null)
            {
                return;
            }

            TargetingParameters parms = new TargetingParameters
            {
                canTargetLocations = true,
                validator = (TargetInfo target) => CanLandHere(target.Cell, map, VGEDefOf.VGE_StartingGravjumperDamaged)
            };
            Find.WindowStack.WindowOfType<MainTabWindow_Quests>()?.Close();
            Find.Targeter.BeginTargeting(parms, delegate(LocalTargetInfo target)
            {
                IntVec3 spawnCell = target.Cell;
                SpawnThingAt(spawnCell, map);
            }, actionWhenFinished: delegate
            {
                if (spawned) return;

                if (CellFinder.TryFindRandomCell(map, (IntVec3 c) => DropCellFinder.IsGoodDropSpot(c, map, allowFogged: false, canRoofPunch: false) && CanLandHere(c, map, VGEDefOf.VGE_StartingGravjumperDamaged), out IntVec3 spawnCell))
                {
                    SpawnThingAt(spawnCell, map);
                }
                else if (DropCellFinder.FindSafeLandingSpot(out spawnCell, factionForFindingSpot, map, 35, 15, 25, thing.def.size))
                {
                    SpawnThingAt(spawnCell, map);
                }
                else
                {
                    spawnCell = DropCellFinder.RandomDropSpot(map);
                    SpawnThingAt(spawnCell, map);
                }
            });
        }

        private void SpawnThingAt(IntVec3 spawnCell, Map map)
        {
            GenPlace.TryPlaceThing(thing, spawnCell, map, ThingPlaceMode.Near);
            spawned = true;
            if (spawnEffecter != null)
            {
                spawnEffecter.SpawnMaintained(thing, thing.Map);
            }

            if (!outSignalResult.NullOrEmpty())
            {
                SignalArgs args = new SignalArgs();
                args.Add(thing.Named("SUBJECT"));
                if (questLookTarget)
                {
                    args.Add(new LookTargets(thing).Named("LOOKTARGETS"));
                }
                Find.SignalManager.SendSignal(new Signal(outSignalResult, args));
            }
        }

        private bool CanLandHere(IntVec3 cell, Map map, KCSG.StructureLayoutDef structure)
        {
            var cellRect = CellRect.CenteredOn(cell, structure.Sizes.x, structure.Sizes.z);

            foreach (IntVec3 cleanCell in cellRect.Cells)
            {

                if (!cleanCell.InBounds(map) || map.areaManager.Home[cleanCell])
                {
                    return false;
                }
                if (cleanCell.GetEdifice(map) != null)
                {
                    return false;
                }
                TerrainDef terrain = cleanCell.GetTerrain(map);
                if (terrain.passability == Traversability.Impassable)
                {
                    return false;
                }
                Thing thing2 = map.thingGrid.ThingAt(cleanCell, ThingDefOf.SteamGeyser);
                if (thing2 != null)
                {
                    return false;
                }
            }
            return true;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref inSignal, "inSignal");
            Scribe_Values.Look(ref spawned, "spawned", defaultValue: false);
            if (!spawned && (thing == null || !(thing is Pawn)))
            {
                Scribe_Deep.Look(ref thing, "thing");
            }
            else
            {
                Scribe_References.Look(ref thing, "thing");
            }
            Scribe_References.Look(ref mapParent, "mapParent");
            Scribe_Values.Look(ref lookForSafeSpot, "lookForSafeSpot", defaultValue: false);
            Scribe_References.Look(ref factionForFindingSpot, "factionForFindingSpot");
            Scribe_Values.Look(ref questLookTarget, "questLookTarget", defaultValue: true);
            Scribe_Values.Look(ref tryLandInShipLandingZone, "tryLandInShipLandingZone", defaultValue: false);
            Scribe_References.Look(ref tryLandNearThing, "tryLandNearThing");
            Scribe_References.Look(ref mapParentOfPawn, "mapParentOfPawn");
            Scribe_Defs.Look(ref spawnEffecter, "spawnEffecter");
            Scribe_Values.Look(ref canRoofPunch, "canRoofPunch", defaultValue: true);
            Scribe_Values.Look(ref outSignalResult, "outSignalResult");
        }
    }
}
