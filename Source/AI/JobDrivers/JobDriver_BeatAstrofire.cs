
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace VanillaGravshipExpanded
{
    public class JobDriver_BeatAstrofire : JobDriver
    {
        protected Astrofire TargetFire => (Astrofire)job.targetA.Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        public  override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            Toil beat = ToilMaker.MakeToil("MakeNewToils");
            Toil approach = ToilMaker.MakeToil("MakeNewToils");
            approach.initAction = delegate
            {
                if (base.Map.reservationManager.CanReserve(pawn, TargetFire))
                {
                    pawn.Reserve(TargetFire, job);
                }
                pawn.pather.StartPath(TargetFire, PathEndMode.Touch);
            };
            approach.tickAction = delegate
            {
                if (pawn.pather.Moving && pawn.pather.nextCell != TargetFire.Position)
                {
                    StartBeatingFireIfAnyAt(pawn.pather.nextCell, beat);
                }
                if (pawn.Position != TargetFire.Position)
                {
                    StartBeatingFireIfAnyAt(pawn.Position, beat);
                }
            };
            approach.FailOnDespawnedOrNull(TargetIndex.A);
            approach.defaultCompleteMode = ToilCompleteMode.PatherArrival;
            approach.atomicWithPrevious = true;
            yield return approach;
            beat.tickAction = delegate
            {
                if (!pawn.CanReachImmediate(TargetFire, PathEndMode.Touch))
                {
                    JumpToToil(approach);
                }
                else if (!(pawn.Position != TargetFire.Position) || !StartBeatingFireIfAnyAt(pawn.Position, beat))
                {
                    TryBeatFire(TargetFire);
                    if (TargetFire.Destroyed)
                    {
                        pawn.records.Increment(RecordDefOf.FiresExtinguished);
                        pawn.jobs.EndCurrentJob(JobCondition.Succeeded);
                    }
                }
            };
            beat.FailOnDespawnedOrNull(TargetIndex.A);
            beat.defaultCompleteMode = ToilCompleteMode.Never;
            yield return beat;
        }

        public bool TryBeatFire(Astrofire targetFire)
        {

            if (pawn.stances.FullBodyBusy)
            {
                return false;
            }
            else
            {
                return pawn.natives.BeatFireVerb.TryStartCastOn(targetFire);
            }

        }

        private bool StartBeatingFireIfAnyAt(IntVec3 cell, Toil nextToil)
        {
            List<Thing> thingList = cell.GetThingList(base.Map);
            for (int i = 0; i < thingList.Count; i++)
            {
                Astrofire fire = thingList[i] as Astrofire;
                if (fire != null && fire.parent == null)
                {
                    job.targetA = fire;
                    pawn.pather.StopDead();
                    JumpToToil(nextToil);
                    return true;
                }
            }
            return false;
        }
    }
}