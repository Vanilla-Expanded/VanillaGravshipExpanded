
using RimWorld;
using System.Collections.Generic;
using Verse;
using UnityEngine;
namespace VanillaGravshipExpanded
{
    public static class AstrofireUtility
    {

        private static readonly List<Astrofire> fireList = new List<Astrofire>();

        private static readonly SimpleCurve ChanceToCatchFirePerSecondForPawnFromFlammability = new SimpleCurve
{
    new CurvePoint(0f, 0f),
    new CurvePoint(0.1f, 0.07f),
    new CurvePoint(0.3f, 1f),
    new CurvePoint(1f, 1f)
};

        public static bool IsAstroBurning(this TargetInfo t)
        {
            if (t.HasThing)
            {
                return t.Thing.IsAstroBurning();
            }
            return t.Cell.ContainsStaticAstrofire(t.Map);
        }

        public static bool IsAstroBurning(this Thing t)
        {
            if (t.Destroyed || !t.Spawned)
            {
                return false;
            }
            if (t.def.size == IntVec2.One)
            {
                if (t is Pawn)
                {
                    return t.HasAttachment(VGEDefOf.VGE_Astrofire);
                }
                return t.Position.ContainsStaticAstrofire(t.Map);
            }
            foreach (IntVec3 item in t.OccupiedRect())
            {
                if (item.ContainsStaticAstrofire(t.Map))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool ContainsStaticAstrofire(this IntVec3 c, Map map)
        {
            List<Thing> list = map.thingGrid.ThingsListAt(c);
            for (int i = 0; i < list.Count; i++)
            {
                Astrofire fire = list[i] as Astrofire;
                if (fire != null && fire.parent == null)
                {
                    return true;
                }
            }
            return false;
        }

        public static List<Astrofire> GetAstrofiresNearCell(this IntVec3 cell, Map map)
        {
            fireList.Clear();
            Room room = RegionAndRoomQuery.RoomAt(cell, map);
            if (room == null || room.Dereferenced || room.Fogged || room.IsHuge || room.TouchesMapEdge)
            {
                Region region = cell.GetRegion(map);
                if (region == null)
                {
                    List<Thing> list = map.thingGrid.ThingsListAt(cell);
                    for (int i = 0; i < list.Count; i++)
                    {
                        Astrofire fire = list[i] as Astrofire;
                        if (fire != null && fire.parent == null)
                        {
                            fireList.Add(fire);
                        }
                    }
                }
                else
                {
                    region.ListerThings.GetThingsOfType(fireList);
                }
            }
            else
            {
                List<Thing> containedAndAdjacentThings = room.ContainedAndAdjacentThings;
                for (int j = 0; j < containedAndAdjacentThings.Count; j++)
                {
                    Astrofire fire2 = containedAndAdjacentThings[j] as Astrofire;
                    if (fire2 != null)
                    {
                        fireList.Add(fire2);
                    }
                }
            }
            fireList.Shuffle();
            fireList.Swap(0, fireList.FindIndex(0, (Astrofire f) => f.Position == cell));
            return fireList;
        }

        public static float ChanceToAttachAstrofireFromEvent(Thing t)
        {
            return ChanceToAttachAstrofireCumulative(t, 60f);
        }

        public static float ChanceToAttachAstrofireCumulative(Thing t, float freqInTicks)
        {
            if (!t.CanEverAttachFire())
            {
                return 0f;
            }
            if (t.HasAttachment(ThingDefOf.Fire) || t.HasAttachment(VGEDefOf.VGE_Astrofire))
            {
                return 0f;
            }
            float num = ChanceToCatchFirePerSecondForPawnFromFlammability.Evaluate(t.GetStatValue(StatDefOf.Flammability));
            return 1f - Mathf.Pow(1f - num, freqInTicks / 60f);
        }

        public static void TryAttachAstrofire(this Thing t, float fireSize, Thing instigator)
        {
            if (t.CanEverAttachFire() && !t.HasAttachment(ThingDefOf.Fire) && !t.HasAttachment(VGEDefOf.VGE_Astrofire))
            {
                Astrofire obj = (Astrofire)ThingMaker.MakeThing(VGEDefOf.VGE_Astrofire);
                obj.fireSize = fireSize;
                obj.instigator = instigator;
                obj.AttachTo(t);
                GenSpawn.Spawn(obj, t.Position, t.Map, Rot4.North);
                Pawn pawn = t as Pawn;
                if (pawn != null)
                {
                    pawn.jobs.StopAll();
                    pawn.records.Increment(RecordDefOf.TimesOnFire);
                }
            }
        }

        public static float ChanceToStartAstrofireIn(IntVec3 c, Map map, SimpleCurve flammabilityChanceCurve = null)
        {
            List<Thing> thingList = c.GetThingList(map);
            float num = c.TerrainFlammability(map);
            for (int i = 0; i < thingList.Count; i++)
            {
                Thing thing = thingList[i];
                if (thing is Astrofire || thing is Fire)
                {
                    return 0f;
                }
                if (thing.def.category != ThingCategory.Pawn && thingList[i].FlammableNow)
                {
                    num = Mathf.Max(num, thing.GetStatValue(StatDefOf.Flammability));
                }
            }
            if (flammabilityChanceCurve != null)
            {
                num = flammabilityChanceCurve.Evaluate(num);
            }
            if (num > 0f)
            {
                Building edifice = c.GetEdifice(map);
                if (edifice != null && edifice.def.passability == Traversability.Impassable && edifice.OccupiedRect().ContractedBy(1).Contains(c))
                {
                    return 0f;
                }
                List<Thing> thingList2 = c.GetThingList(map);
                for (int j = 0; j < thingList2.Count; j++)
                {
                    if (thingList2[j].def.category == ThingCategory.Filth && !thingList2[j].def.filth.allowsFire)
                    {
                        return 0f;
                    }
                }
            }
            return num;
        }

        public static bool TryStartAstrofireIn(IntVec3 c, Map map, float fireSize, Thing instigator, SimpleCurve flammabilityChanceCurve = null)
        {
            if (ChanceToStartAstrofireIn(c, map, flammabilityChanceCurve) <= 0f)
            {
                return false;
            }
            Astrofire obj = (Astrofire)ThingMaker.MakeThing(VGEDefOf.VGE_Astrofire);
            obj.fireSize = fireSize;
            obj.instigator = instigator;
            GenSpawn.Spawn(obj, c, map, Rot4.North);
            return true;
        }

    }


}
