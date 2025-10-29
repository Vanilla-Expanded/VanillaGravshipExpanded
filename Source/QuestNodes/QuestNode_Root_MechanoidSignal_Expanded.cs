
using KTrie;
using PipeSystem;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System.Collections.Generic;
using UnityEngine.UIElements;
using Verse;
using Verse.AI.Group;
using Verse.AI;
using static UnityEngine.GraphicsBuffer;
using UnityEngine;

namespace VanillaGravshipExpanded
{
    public class QuestNode_Root_MechanoidSignal_Expanded : QuestNode
    {
        private const int SkyfallerDelayTicks = 300;

       
        public override void RunInt()
        {
            Quest quest = QuestGen.quest;
            Map map = QuestGen_Get.GetMap();
            string dropPodsSpawnedSignal = QuestGenUtility.HardcodedSignalWithQuestID("dropPodsSpawned");
            string inSignal = QuestGenUtility.HardcodedSignalWithQuestID("gravEngine.Inspected");
            QuestUtility.AddQuestTag(map.Parent, QuestGenUtility.HardcodedTargetQuestTagWithQuestID("gravEngine"));
            CellFinder.TryFindRandomCell(map, (IntVec3 c) => DropCellFinder.IsGoodDropSpot(c, map, allowFogged: false, canRoofPunch: false) && CanLandHere(c, map, VGEDefOf.VGE_StartingGravjumperDamaged), out IntVec3 spawnCell);
            var target = Gen.YieldSingle(new GlobalTargetInfo(spawnCell, map));
            quest.Delay(300, delegate
            {
                var landingStructure = (LandingStructure)ThingMaker.MakeThing(VGEDefOf.VGE_LandingStructure);
                landingStructure.layoutDef = VGEDefOf.VGE_StartingGravjumperDamaged;

                QuestPart_SpawnThing questPart_SpawnThing = new QuestPart_SpawnThing
                {
                    thing = landingStructure,
                    mapParentOfPawn = map.mapPawns.FreeColonistsSpawned.RandomElement(),
                    inSignal = QuestGen.slate.Get<string>("inSignal"),
                    cell = spawnCell,
                    questLookTarget = false
                };               
                quest.AddPart(questPart_SpawnThing);

                List<PawnKindDef> mechTypes = new List<PawnKindDef>
                {
                VGEDefOf.VGE_Astropede,
                VGEDefOf.VGE_Hunter
                };
                float threatPoints = StorytellerUtility.DefaultThreatPointsNow(map);
                int chunkAmount = Mathf.Max((int)(threatPoints / 900),1);
                for(int i=0; i<chunkAmount; i++)
                {
                    Skyfaller skyfaller = SkyfallerMaker.MakeSkyfaller(ThingDefOf.ShipChunkIncoming_SmallExplosion, ChunkContents(quest, mechTypes));
                    skyfaller.contentsCanOverlap = false;
                    skyfaller.moveAside = true;
                    QuestPart_SpawnThing questPart_SpawnThing2 = new QuestPart_SpawnThing
                    {
                        thing = skyfaller,
                        mapParentOfPawn = map.mapPawns.FreeColonistsSpawned.RandomElement(),
                        inSignal = QuestGen.slate.Get<string>("inSignal"),
                        cell = CellFinder.RandomClosewalkCellNear(spawnCell, map, 30),
                        questLookTarget = false
                    };
                    quest.AddPart(questPart_SpawnThing2);
                }
                

                quest.Letter(LetterDefOf.PositiveEvent, null, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, Gen.YieldSingle(target), filterDeadPawnsFromLookTargets: false, "[gravEngineSpawnedLetterText]", null, "[gravEngineSpawnedLetterLabel]");
                quest.SignalPass(null, null, dropPodsSpawnedSignal);
            });
            quest.LookTargets(target);
            QuestPart_Choice questPart_Choice = quest.RewardChoice();
            QuestPart_Choice.Choice item = new QuestPart_Choice.Choice
            {
                rewards =
            {
                (Reward)new Reward_DefinedThingDef
                {
                    thingDef = VGEDefOf.VGE_GravjumperEngine
                }
            }
            };
            questPart_Choice.choices.Add(item);
            quest.End(QuestEndOutcome.Success, 0, null, inSignal);
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

        private List<Thing> ChunkContents(Quest quest, List<PawnKindDef> mechTypes)
        {
            List<Thing> list = new List<Thing>();
           
            list.Add(ThingMaker.MakeThing(ThingDefOf.ShipChunk_Mech));
           
            PawnKindDef kindDef;
            for (int remaining = 900; remaining > 0;)
            {
                if (mechTypes.TryRandomElement(out kindDef))
                {
                    Pawn pawn = PawnGenerator.GeneratePawn(kindDef, Faction.OfMechanoids);
                    list.Add(pawn);
                    remaining -= (int)kindDef.combatPower;

                }
            }
         
          
            return list;
        }

        public override bool TestRunInt(Slate slate)
        {
            
            if (QuestGen_Get.GetMap() == null)
            {
                return false;
            }
            if (GravshipUtility.PlayerHasGravEngine())
            {
                return false;
            }
            return true;
        }
    }
}
