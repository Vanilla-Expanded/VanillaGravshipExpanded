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

        public float statValuePawn;
        public float statValueObject;

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
                statValuePawn = repair.actor.GetStatValue(VGEDefOf.VGE_GravshipMaintenance);
                statValueObject = building.GetStatValue(VGEDefOf.VGE_MaintenanceSensitivity);
            };
            repair.tickAction = delegate
            {
                Pawn actor = repair.actor;
               
                actor.rotationTracker.FaceTarget(actor.CurJob.GetTarget(TargetIndex.A));

                comp.maintenance += (0.001f * statValuePawn) / statValueObject;


                if (comp.maintenance >= 1)
                {
                    comp.maintenance = 1;
                    actor.records.Increment(RecordDefOf.ThingsRepaired);
                    actor.jobs.EndCurrentJob(JobCondition.Succeeded);
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
