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

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref this.statValuePawn, "statValuePawn", 1, false);
            Scribe_Values.Look(ref this.statValueObject, "statValueObject", 1, false);
        }

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
                statValueObject = Mathf.Max(building.GetStatValue(VGEDefOf.VGE_MaintenanceSensitivity) + building.GetStatValue(VGEDefOf.VGE_MaintenanceDifficulty), 1f);
            };
            repair.tickIntervalAction = delegate (int delta)
            {
                Pawn actor = repair.actor;
               
                actor.rotationTracker.FaceTarget(actor.CurJob.GetTarget(TargetIndex.A));

                comp.maintenance += 0.001f * statValuePawn * delta / statValueObject;


                if (comp.maintenance >= 1)
                {
                    comp.maintenance = 1;
                    actor.records.Increment(RecordDefOf.ThingsRepaired);
                    actor.jobs.EndCurrentJob(JobCondition.Succeeded);
                }
                if (actor.skills != null)
                {
                    actor.skills.Learn(SkillDefOf.Construction, 0.05f * (float)delta);
                }



            };
            repair.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            repair.WithEffect(base.TargetThingA?.def.repairEffect, TargetIndex.A);
            repair.defaultCompleteMode = ToilCompleteMode.Never;
            repair.activeSkill = () => SkillDefOf.Construction;
            repair.handlingFacing = true;
            yield return repair;



        }
    }
}
