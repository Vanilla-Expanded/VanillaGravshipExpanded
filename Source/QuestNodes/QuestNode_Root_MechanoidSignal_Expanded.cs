using RimWorld;
using RimWorld.QuestGen;
using System.Collections.Generic;
using Verse;
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
            string inSignal = QuestGen.slate.Get<string>("inSignal");
            string landingStructureSpawnedSignal = QuestGen.GenerateNewSignal("LandingStructureSpawned");

            QuestUtility.AddQuestTag(map.Parent, QuestGenUtility.HardcodedTargetQuestTagWithQuestID("gravEngine"));

            var landingStructure = (LandingStructure)ThingMaker.MakeThing(VGEDefOf.VGE_LandingStructure);
            landingStructure.layoutDef = VGEDefOf.VGE_StartingGravjumperDamaged;

            var questPart_SpawnThingInteractive = new QuestPart_SpawnThingInteractive
            {
                thing = landingStructure,
                mapParent = map.Parent,
                inSignal = inSignal,
                questLookTarget = true,
                outSignalResult = landingStructureSpawnedSignal
            };
            quest.AddPart(questPart_SpawnThingInteractive);

            quest.Letter(LetterDefOf.PositiveEvent, inSignal: landingStructureSpawnedSignal, label: "[gravEngineSpawnedLetterLabel]", text: "[gravEngineSpawnedLetterText]");

            quest.Delay(SkyfallerDelayTicks * 10, delegate
            {
                List<PawnKindDef> mechTypes = new List<PawnKindDef>
                {
                    VGEDefOf.VGE_Astropede,
                    VGEDefOf.VGE_Hunter
                };
                float threatPoints = StorytellerUtility.DefaultThreatPointsNow(map);
                int chunkAmount = Mathf.Max((int)(threatPoints / 1000), 1);
                for (int i = 0; i < chunkAmount; i++)
                {
                    Skyfaller skyfaller = SkyfallerMaker.MakeSkyfaller(ThingDefOf.ShipChunkIncoming_SmallExplosion, ChunkContents(quest, mechTypes));
                    skyfaller.contentsCanOverlap = false;
                    skyfaller.moveAside = true;
                    QuestPart_SpawnThing questPart_SpawnThing2 = new QuestPart_SpawnThing
                    {
                        thing = skyfaller,
                        mapParent = map.Parent,
                        inSignal = landingStructureSpawnedSignal,
                        tryLandNearThing = landingStructure,
                        lookForSafeSpot = true
                    };
                    quest.AddPart(questPart_SpawnThing2);
                }
            }, landingStructureSpawnedSignal);

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
            quest.End(QuestEndOutcome.Success, 0, null, landingStructureSpawnedSignal);
        }

        private List<Thing> ChunkContents(Quest quest, List<PawnKindDef> mechTypes)
        {
            List<Thing> list = new List<Thing>();

            list.Add(ThingMaker.MakeThing(ThingDefOf.ShipChunk_Mech));

            PawnKindDef kindDef;
            for (int remaining = 1000; remaining > 0;)
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
