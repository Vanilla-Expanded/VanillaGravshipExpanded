using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using RimWorld;
using UnityEngine;

namespace VanillaGravshipExpanded
{
    public class JobDriver_MaintainGrav : JobDriver
    {

        protected float ticksToNextRepair;

        private const float WarmupTicks = 80f;

        private const float TicksBetweenRepairs = 20f;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(this.job.GetTarget(TargetIndex.A).Thing, this.job, 1, -1, null, true);
        }
        private CompGravMaintainable comp => job.GetTarget(TargetIndex.A).Thing.TryGetComp<CompGravMaintainable>();

        public override IEnumerable<Toil> MakeNewToils()
        {
            Thing building = this.job.GetTarget(TargetIndex.A).Thing;
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOnBurningImmobile(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);

            Toil repair = ToilMaker.MakeToil("MakeNewToils");
            repair.initAction = delegate
            {
                ticksToNextRepair = 80f;
            };
            repair.tickAction = delegate
            {
                Pawn actor = repair.actor;
                if (actor.skills != null)
                {
                    actor.skills.Learn(SkillDefOf.Construction, 0.025f);
                }

                actor.rotationTracker.FaceTarget(actor.CurJob.GetTarget(TargetIndex.A));

                float num = actor.GetStatValue(StatDefOf.ConstructionSpeed) * 1.7f;
                ticksToNextRepair -= num;
                if (ticksToNextRepair <= 0f)
                {
                    ticksToNextRepair += 20f;
                    comp.maintenance += 0.01f;
                    if (comp.maintenance >= 1)
                    {
                        comp.maintenance = 1;
                        actor.records.Increment(RecordDefOf.ThingsRepaired);
                        actor.jobs.EndCurrentJob(JobCondition.Succeeded);
                    }


                }
            };
            repair.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            repair.WithEffect(base.TargetThingA.def.repairEffect, TargetIndex.A);
            repair.defaultCompleteMode = ToilCompleteMode.Never;
            repair.activeSkill = () => SkillDefOf.Construction;
            repair.handlingFacing = true;
            yield return repair;



        }
    }
}
